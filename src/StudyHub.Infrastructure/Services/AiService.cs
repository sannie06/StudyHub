using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Ai;
using StudyHub.Application.DTOs.Dashboard;
using StudyHub.Infrastructure.Services.Ai;

namespace StudyHub.Infrastructure.Services
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IDashboardService _dashboardService;
        private readonly IAiScheduleContextProvider _scheduleContextProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiService> _logger;

        private readonly AiIntentDetector _intentDetector;
        private readonly AiContextBuilder _contextBuilder;
        private readonly AiPromptBuilder _promptBuilder;

        public AiService(
            HttpClient httpClient,
            IDashboardService dashboardService,
            IAiScheduleContextProvider scheduleContextProvider,
            IConfiguration configuration,
            ILogger<AiService> logger)
        {
            _httpClient = httpClient;
            _dashboardService = dashboardService;
            _scheduleContextProvider = scheduleContextProvider;
            _configuration = configuration;
            _logger = logger;

            _intentDetector = new AiIntentDetector();
            _contextBuilder = new AiContextBuilder();
            _promptBuilder = new AiPromptBuilder();
        }

        public async Task<AiChatResponse> ChatAsync(int userId, AiChatRequest request)
        {
            var userMessage = (request.Message ?? string.Empty).Trim();
            
            // 1. Detect Intent via Intent Engine (Including DateResolution for Schedule Queries)
            var intentResult = _intentDetector.DetectIntent(userMessage, request.PromptType);

            // 2. Dedicated Schedule Retrieval if Intent is SCHEDULE_QUERY (Decoupled from Dashboard)
            AiScheduleContextResult? scheduleContext = null;
            if (intentResult.Intent == AiIntents.ScheduleQuery)
            {
                var dateRes = intentResult.DateResolution ?? new AiDateResolver().Resolve(userMessage);
                scheduleContext = await _scheduleContextProvider.GetScheduleContextAsync(userId, dateRes);
            }

            // 3. Fetch User Dashboard Data (Retained for other intents: TaskPrioritization, Workload, etc.)
            var dashboard = await _dashboardService.GetDashboardDataAsync(userId);

            // 4. Build Selective Context based on Intent
            var aiContext = _contextBuilder.BuildContext(intentResult, dashboard, userId, scheduleContext);

            var workloadLevel = CalculateWorkloadLevel(dashboard);

            // 4. Guardrail: If critical information is missing, ask clarification instead of guessing
            if (aiContext.IsClarificationNeeded)
            {
                return new AiChatResponse
                {
                    Reply = aiContext.ClarificationPrompt ?? "Để mình hỗ trợ bạn chính xác nhất, bạn có thể chia sẻ thêm một số thông tin chi tiết được không?",
                    Intent = aiContext.Intent,
                    RequiredInformation = aiContext.MissingInformation,
                    ActionSuggestions = aiContext.SuggestedClarificationOptions.Any()
                        ? aiContext.SuggestedClarificationOptions
                        : new List<string> { "Cung cấp thêm chi tiết", "Xem deadline sắp tới", "Phân tích mức độ quá tải" },
                    WorkloadLevel = workloadLevel
                };
            }

            // 5. Build Intent-Targeted Prompt via AiPromptBuilder
            var prompt = _promptBuilder.BuildPrompt(aiContext, userMessage);
            var aiReply = await CallGeminiApiAsync(prompt);

            // 6. Intelligent Fallback if API fails or unreachable
            if (string.IsNullOrEmpty(aiReply))
            {
                aiReply = GenerateIntentAwareFallbackReply(aiContext, dashboard, userMessage);
            }

            return new AiChatResponse
            {
                Reply = aiReply,
                Intent = aiContext.Intent,
                ActionSuggestions = GenerateActionSuggestionsForIntent(aiContext.Intent),
                WorkloadLevel = workloadLevel
            };
        }

        public async Task<StudyPlanResponse> GenerateStudyPlanAsync(int userId, StudyPlanRequest request)
        {
            var dashboard = await _dashboardService.GetDashboardDataAsync(userId);
            var contextText = BuildUserContextPrompt(dashboard);

            var prompt = $"{contextText}\n\nYêu cầu: Hãy lập kế hoạch học tập chi tiết trong {request.NumberOfDays} ngày với mục tiêu: '{request.Goal}'. Đưa ra gợi ý từng ngày phân bổ thời gian hợp lý.";
            var aiAdvice = await CallGeminiApiAsync(prompt);

            var items = new List<StudyPlanItemDto>();
            var days = Math.Max(1, Math.Min(14, request.NumberOfDays));
            var tasks = dashboard.UpcomingDeadlines ?? new List<DashboardTaskItemDto>();

            var todayVn = DateTime.UtcNow.AddHours(7).Date;

            // Sắp xếp task: Quá hạn lên đầu -> Deadline gần -> Mức ưu tiên cao
            var sortedTasks = tasks
                .OrderBy(t => t.HanHoanThanh.HasValue ? t.HanHoanThanh.Value : DateTime.MaxValue)
                .ThenByDescending(t => t.DoUuTien)
                .ToList();

            for (int d = 1; d <= days; d++)
            {
                string dayLabel = $"Ngày {d}";
                string taskTitle;
                string duration;
                string focusArea;

                // Giai đoạn 4: Ngày cuối cùng của lộ trình (nếu từ 3 ngày trở lên) -> Tổng kết & Tự đánh giá
                if (d == days && days >= 3)
                {
                    taskTitle = "Tổng ôn kiến thức trọng tâm & Tự kiểm tra đánh giá";
                    duration = "60 phút";
                    focusArea = sortedTasks.FirstOrDefault()?.TenMonHoc ?? "Tổng hợp";
                }
                // Giai đoạn 3: Ngày áp chót (nếu từ 5 ngày trở lên) -> Luyện đề & Rà soát lỗi
                else if (d == days - 1 && days >= 5)
                {
                    taskTitle = "Luyện giải bài tập tổng hợp & Kiểm thử / Rà soát lỗi";
                    duration = "90 phút";
                    focusArea = sortedTasks.Skip(1).FirstOrDefault()?.TenMonHoc ?? sortedTasks.FirstOrDefault()?.TenMonHoc ?? "Thực hành";
                }
                // Giai đoạn 1 & 2: Phân bổ thực tế dựa theo từng task của sinh viên
                else
                {
                    if (sortedTasks.Any())
                    {
                        var taskIndex = (d - 1) % sortedTasks.Count;
                        var currentTask = sortedTasks[taskIndex];
                        focusArea = currentTask.TenMonHoc ?? "Chuyên ngành";

                        bool isOverdue = currentTask.HanHoanThanh.HasValue && currentTask.HanHoanThanh.Value.Date < todayVn;

                        // Lượt 1: Bắt đầu làm và xây dựng khung
                        if (d <= sortedTasks.Count)
                        {
                            if (isOverdue)
                            {
                                taskTitle = $"Tập trung dứt điểm task quá hạn: {currentTask.TieuDe}";
                                duration = "120 phút";
                            }
                            else
                            {
                                taskTitle = $"Hệ thống hóa lý thuyết & Bắt đầu làm: {currentTask.TieuDe}";
                                duration = "90 phút";
                            }
                        }
                        // Lượt 2+: Nâng cao, tối ưu và hoàn thiện chi tiết (thay vì lặp lại tên cũ)
                        else
                        {
                            taskTitle = $"Hoàn thiện chuyên sâu & Tối ưu bài làm: {currentTask.TieuDe}";
                            duration = "90 phút";
                        }
                    }
                    else
                    {
                        taskTitle = $"Nghiên cứu tài liệu & Thực hành chuyên đề ({request.Goal})";
                        duration = "90 phút";
                        focusArea = "Kiến thức chuyên ngành";
                    }
                }

                items.Add(new StudyPlanItemDto
                {
                    Day = dayLabel,
                    TaskName = taskTitle,
                    Duration = duration,
                    FocusArea = focusArea
                });
            }

            return new StudyPlanResponse
            {
                Title = $"Kế hoạch học tập {request.NumberOfDays} ngày: {request.Goal}",
                Advice = string.IsNullOrEmpty(aiAdvice) 
                    ? "Lộ trình được tối ưu theo 4 giai đoạn (Xử lý việc khẩn cấp → Thực hành trọng tâm → Luyện bài tổng hợp → Tổng ôn rà soát). Hãy áp dụng Pomodoro 25/5 để duy trì sự tập trung tối đa!" 
                    : aiAdvice,
                PlanItems = items
            };
        }

        public async Task<string> AnalyzeWorkloadAsync(int userId)
        {
            var dashboard = await _dashboardService.GetDashboardDataAsync(userId);
            var totalTasks = dashboard.Statistics.TongSoCongViec;
            var pendingTasks = dashboard.Statistics.CongViecChuaHoanThanh;
            var deadlinesToday = dashboard.Statistics.DeadlineHomNay;
            var examsCount = dashboard.NearestExamSchedules.Count;

            var sb = new StringBuilder();
            sb.AppendLine("=== PHÂN TÍCH MỨC ĐỘ QUÁ TẢI HỌC TẬP ===");
            sb.AppendLine($"- Tổng công việc hiện tại: {totalTasks}");
            sb.AppendLine($"- Công việc chưa hoàn thành: {pendingTasks}");
            sb.AppendLine($"- Deadline trong hôm nay: {deadlinesToday}");
            sb.AppendLine($"- Số lịch thi sắp tới: {examsCount}");
            sb.AppendLine();

            if (deadlinesToday > 3 || pendingTasks > 10 || examsCount > 2)
            {
                sb.AppendLine("MỨC ĐỘ: QUÁ TẢI CAO (HIGH OVERLOAD) 🚨");
                sb.AppendLine("Lời khuyên: Hãy ưu tiên giải quyết các deadline trong hôm nay. Chia nhỏ nhiệm vụ lớn thành các Pomodoro 25 phút. Đừng ngại hoãn lại các việc ít quan trọng.");
            }
            else if (pendingTasks > 4 || deadlinesToday > 0)
            {
                sb.AppendLine("MỨC ĐỘ: VỪA PHẢI (MODERATE) ⚠️");
                sb.AppendLine("Lời khuyên: Khối lượng công việc tương đối cân bằng. Giữ vững nhịp độ học tập hàng ngày.");
            }
            else
            {
                sb.AppendLine("MỨC ĐỘ: NHẸ NHÀNG (LOW) ✅");
                sb.AppendLine("Lời khuyên: Tiến độ rất tốt! Bạn có thể tranh thủ đọc trước tài liệu môn học mới hoặc tham gia nhóm học tập.");
            }

            return sb.ToString();
        }

        public async Task<string> GetStudyAdviceAsync(int userId)
        {
            var dashboard = await _dashboardService.GetDashboardDataAsync(userId);
            var prompt = $"{BuildUserContextPrompt(dashboard)}\n\nYêu cầu: Cho sinh viên 3 lời khuyên học tập thiết thực nhất dựa trên dữ liệu hiện tại.";
            var advice = await CallGeminiApiAsync(prompt);

            if (string.IsNullOrEmpty(advice))
            {
                advice = "1. Áp dụng phương pháp Pomodoro 25/5 để giữ sự tập trung cao độ.\n2. Lập danh sách 3 việc quan trọng nhất cần làm mỗi buổi sáng.\n3. Tham gia thảo luận trong các nhóm học tập để củng cố kiến thức.";
            }

            return advice;
        }



        private string GenerateIntentAwareFallbackReply(AiContext context, DashboardDto dashboard, string userQuery)
        {
            var q = userQuery.Trim();
            var qLower = q.ToLowerInvariant();
            var qNorm = NormalizeText(q);

            switch (context.Intent)
            {
                case AiIntents.KnowledgeQa:
                    // 1. So sánh Interface và Abstract Class
                    if (qNorm.Contains("interface") && (qNorm.Contains("abstract class") || qNorm.Contains("khac") || qNorm.Contains("so sanh")))
                    {
                        return "⚖️ **So sánh Interface và Abstract Class trong Java:**\n\n" +
                               "| Tiêu chí | Interface | Abstract Class |\n" +
                               "| :--- | :--- | :--- |\n" +
                               "| **Đa kế thừa** | Hỗ trợ implement nhiều Interface | Chỉ extends được 1 Abstract Class |\n" +
                               "| **Phương thức** | Chủ yếu là `abstract` (có thêm `default`/`static` từ Java 8) | Có thể có cả phương thức trừu tượng và cụ thể đầy đủ code |\n" +
                               "| **Biến/Thuộc tính** | Chỉ có hằng số `public static final` | Có thể khai báo biến `private`, `protected`, `instance variable` |\n" +
                               "| **Constructor** | Không có Constructor | Có Constructor để lớp con gọi `super()` |\n" +
                               "| **Mục đích** | Định nghĩa *hành vi* chuẩn (Can-do) | Định nghĩa *bản chất* đối tượng (Is-a) |\n\n" +
                               "💡 *Ví dụ:* `Bird` là một `Abstract Class` (Is-a Animal), nhưng implement `Flyable` là một `Interface` (Can-do Fly).";
                    }

                    // 2. Định nghĩa hoặc Ví dụ về Interface
                    if (qNorm.Contains("interface") || (qNorm.Contains("vi du") && qNorm.Contains("interface")))
                    {
                        return "☕ **Java Interface là gì?**\n\n" +
                               "• **Khái niệm:** `Interface` trong Java là một bản thiết kế trừu tượng (blueprint) định nghĩa các phương thức mà một lớp phải thực thi (implements), giúp đạt được tính trừu tượng hoàn toàn và đa kế thừa hành vi.\n\n" +
                               "• **Ví dụ mã nguồn Java:**\n" +
                               "```java\n" +
                               "// 1. Định nghĩa Interface\n" +
                               "public interface IPaymentService {\n" +
                               "    void processPayment(double amount); // Phương thức trừu tượng\n" +
                               "}\n\n" +
                               "// 2. Class thực thi Interface\n" +
                               "public class VnpayService implements IPaymentService {\n" +
                               "    @Override\n" +
                               "    public void processPayment(double amount) {\n" +
                               "        System.out.println(\"Thanh toán qua VNPay: \" + amount + \" VNĐ\");\n" +
                               "    }\n" +
                               "}\n" +
                               "```\n\n" +
                               "• **Giải thích:** Lớp `VnpayService` bắt buộc phải override phương thức `processPayment()`. Nhờ đó code linh hoạt và giảm phụ thuộc (Loose Coupling).\n\n" +
                               "👉 *Bạn có muốn mình giải thích thêm Interface khác Abstract Class như thế nào không?*";
                    }

                    // 3. Giải thích SQL JOIN / INNER JOIN
                    if (qNorm.Contains("join"))
                    {
                        return "🔍 **SQL JOIN là gì?**\n\n" +
                               "• **Định nghĩa:** `JOIN` trong SQL là mệnh đề dùng để kết hợp các bản ghi từ hai hoặc nhiều bảng trong cơ sở dữ liệu dựa trên một cột chung (thường là Khóa chính - Khóa ngoại).\n\n" +
                               "• **Các loại JOIN phổ biến:**\n" +
                               "1. **INNER JOIN:** Chỉ trả về các dòng có dữ liệu khớp ở cả 2 bảng.\n" +
                               "2. **LEFT JOIN (LEFT OUTER JOIN):** Trả về toàn bộ dòng của bảng bên trái, và các dòng khớp từ bảng bên phải (nếu không có thì trả về `NULL`).\n" +
                               "3. **RIGHT JOIN:** Trả về toàn bộ bảng bên phải và các dòng khớp bên trái.\n" +
                               "4. **FULL OUTER JOIN:** Trả về tất cả các dòng khi có dữ liệu khớp ở một trong hai bảng.\n\n" +
                               "• **Ví dụ Cú pháp:**\n" +
                               "```sql\n" +
                               "SELECT SinhVien.HoTen, LopHoc.TenLop\n" +
                               "FROM SinhVien\n" +
                               "INNER JOIN LopHoc ON SinhVien.MaLop = LopHoc.MaLop;\n" +
                               "```";
                    }

                    // 4. Hướng dẫn kỹ thuật Fix Backend API
                    if (qNorm.Contains("fix backend") || qNorm.Contains("fix api"))
                    {
                        return "🛠️ **Quy trình chuẩn đoán và sửa lỗi Backend API:**\n\n" +
                               "1. **Định vị mã lỗi HTTP:**\n" +
                               "   • `400 Bad Request`: Kiểm tra tên trường và kiểu dữ liệu JSON trong DTO.\n" +
                               "   • `401 Unauthorized`: Kiểm tra Bearer Token JWT trong Authorization header.\n" +
                               "   • `500 Internal Server Error`: Mở Console/Log xem stack trace để tìm dòng code bị `NullReferenceException` hoặc lỗi SQL.\n\n" +
                               "2. **Kiểm tra truy vấn Database:** Xác minh chuỗi Connection String và câu lệnh LINQ / SQL.\n" +
                               "3. **Kiểm thử độc lập:** Dùng Swagger / Postman gửi thử dữ liệu đầu vào để cô lập phạm vi lỗi.";
                    }

                    // 5. Câu hỏi lý thuyết: Task quá hạn có phải luôn ưu tiên cao nhất không?
                    if (qNorm.Contains("task qua han") && (qNorm.Contains("co phai") || qNorm.Contains("luon uu tien") || qNorm.Contains("cao nhat khong")))
                    {
                        return "💡 **Không phải lúc nào task quá hạn cũng là ưu tiên cao nhất một cách tuyệt đối.**\n\n" +
                               "Trong quản lý công việc và Ma trận Eisenhower, bạn cần phân biệt:\n" +
                               "1. **Tính khẩn cấp (Urgency):** Task quá hạn có tính khẩn cấp cao vì đã trễ hạn, cần xử lý sớm để tránh dồn ứ tồn đọng.\n" +
                               "2. **Tầm quan trọng (Importance / Impact):** Nếu task quá hạn chỉ là việc phụ (ít ảnh hưởng) trong khi bạn có một task quan trọng sống còn (như thi cử, nộp đồ án tốt nghiệp) sắp đến hạn hôm nay, bạn phải ưu tiên bảo vệ task quan trọng trước.\n\n" +
                               "👉 **Quy tắc thực tế:** Hãy ưu tiên dứt điểm task quá hạn nếu nó quan trọng hoặc có thể giải quyết nhanh (Quick Win), nhưng đừng để một task quá hạn không quan trọng làm bạn bỏ lỡ deadline của nhiệm vụ trọng yếu!";
                    }

                    // 6. Nguyên tắc ưu tiên Task (Ma trận Eisenhower, Deadline vs Priority)
                    if ((qNorm.Contains("deadline") && (qNorm.Contains("uu tien") || qNorm.Contains("quan trong hon"))) || 
                        qNorm.Contains("eisenhower") || 
                        (qNorm.Contains("thap") && qNorm.Contains("cao")) ||
                        qNorm.Contains("han hom nay") ||
                        qNorm.Contains("task nao toi nen lam truoc") ||
                        qNorm.Contains("task nao nen lam truoc") ||
                        qNorm.Contains("uu tien deadline hay"))
                    {
                        return "🔥 **Bạn nên làm task có deadline hôm nay trước**, dù mức ưu tiên ban đầu thấp hơn.\n\n" +
                               "• **Lý do:** Deadline thể hiện tính **khẩn cấp**. Khi một task có hạn chót là hôm nay còn task kia đến tuần sau, task hôm nay cần được hoàn thành ngay để tránh bị quá hạn ngay lập tức.\n\n" +
                               "• **Nguyên tắc Ma trận Eisenhower:** Yếu tố **Khẩn cấp (Deadline)** luôn luôn được ưu tiên giải quyết trước yếu tố **Quan trọng (Độ ưu tiên)**. Mức độ ưu tiên (`DoUuTien`) chỉ được dùng để phân định thứ tự giữa các công việc có **cùng thời hạn deadline**.\n\n" +
                               "• Sau khi hoàn thành xong task hôm nay, bạn mới tiếp tục xử lý task tuần sau dựa trên mức độ ưu tiên của nó.\n\n" +
                               "💡 *Lời khuyên:* Hãy giải quyết dứt điểm các task cận hạn trong ngày hôm nay trước, sau đó dành thời gian tập trung cho các task quan trọng của tuần tới!";
                    }

                    return "💡 **Hướng dẫn kiến thức học thuật:** Bạn có thể hỏi tôi bất kỳ câu hỏi về: *Khái niệm lập trình (Java, C#, TypeScript)*, *Cơ sở dữ liệu (SQL, Index, Trigger)*, hoặc *Giải thuật (Dijkstra, DFS/BFS)*!";

                case AiIntents.TaskQuery:
                    bool isOnlyOverdueQuery = (qNorm.Contains("qua han") || qNorm.Contains("qua deadline")) && !qNorm.Contains("hom nay");
                    bool isTodayOrActionQuery = qNorm.Contains("hom nay") || qNorm.Contains("can lam") || 
                                                qNorm.Contains("chua hoan thanh") || qNorm.Contains("chua xong") || 
                                                qNorm.Contains("chua lam") || qNorm.Contains("con nhung") || 
                                                qNorm.Contains("con task") || qNorm.Contains("con viec") ||
                                                qNorm.Contains("toi con");

                    var sbTaskQuery = new StringBuilder();

                    if (isOnlyOverdueQuery)
                    {
                        var overdueList = context.OverdueTasks ?? new List<DashboardTaskItemDto>();
                        if (!overdueList.Any())
                        {
                            return "🎉 **Tuyệt vời!** Bạn hiện không có công việc nào bị quá hạn. Tiến độ học tập của bạn đang rất tốt!";
                        }

                        sbTaskQuery.AppendLine($"🚨 **Bạn có {overdueList.Count} công việc đã quá hạn:**\n");
                        foreach (var t in overdueList)
                        {
                            sbTaskQuery.AppendLine($"• **{t.TieuDe}** (Môn: {t.TenMonHoc ?? "Chung"}) — Quá hạn từ `{t.HanHoanThanh:dd/MM/yyyy}` (Ưu tiên: {FormatTaskPriority(t.DoUuTien)})");
                        }
                        sbTaskQuery.AppendLine("\n👉 Hãy cố gắng hoàn thành sớm các công việc này để tránh tồn đọng nhé!");
                        return sbTaskQuery.ToString().TrimEnd();
                    }

                    if (isTodayOrActionQuery)
                    {
                        var overdueList = context.OverdueTasks ?? new List<DashboardTaskItemDto>();
                        var todayList = context.RelevantTasks ?? new List<DashboardTaskItemDto>();
                        int totalTasks = overdueList.Count + todayList.Count;

                        if (totalTasks == 0)
                        {
                            return "🎉 **Hôm nay bạn không có công việc nào cần xử lý!** Bạn có thể xem trước bài học hoặc nghỉ ngơi nhé.";
                        }

                        sbTaskQuery.AppendLine($"📋 Hôm nay bạn còn **{totalTasks} công việc cần xử lý**:\n");

                        if (overdueList.Any())
                        {
                            sbTaskQuery.AppendLine("🚨 **Quá hạn:**");
                            foreach (var t in overdueList)
                            {
                                sbTaskQuery.AppendLine($"• **{t.TieuDe}** — Hạn: `{t.HanHoanThanh:dd/MM/yyyy}`");
                            }
                            sbTaskQuery.AppendLine();
                        }

                        if (todayList.Any())
                        {
                            sbTaskQuery.AppendLine("🔴 **Deadline hôm nay:**");
                            foreach (var t in todayList)
                            {
                                sbTaskQuery.AppendLine($"• **{t.TieuDe}** — Hạn: `{t.HanHoanThanh:dd/MM/yyyy}`");
                            }
                        }

                        return sbTaskQuery.ToString().TrimEnd();
                    }

                    // General task query
                    var allOverdue = context.OverdueTasks ?? new List<DashboardTaskItemDto>();
                    var allToday = context.RelevantTasks ?? new List<DashboardTaskItemDto>();
                    var allUpcoming = context.UpcomingDeadlines ?? new List<DashboardTaskItemDto>();

                    if (!allOverdue.Any() && !allToday.Any() && !allUpcoming.Any())
                    {
                        return "🎉 Bạn hiện không có công việc nào đang chờ xử lý!";
                    }

                    sbTaskQuery.AppendLine("📋 **Tổng hợp danh sách công việc của bạn:**\n");
                    if (allOverdue.Any())
                    {
                        sbTaskQuery.AppendLine($"🚨 **Quá hạn ({allOverdue.Count}):** " + string.Join(", ", allOverdue.Select(t => t.TieuDe)));
                    }
                    if (allToday.Any())
                    {
                        sbTaskQuery.AppendLine($"🔴 **Hôm nay ({allToday.Count}):** " + string.Join(", ", allToday.Select(t => t.TieuDe)));
                    }
                    if (allUpcoming.Any())
                    {
                        sbTaskQuery.AppendLine($"⏰ **Sắp tới ({allUpcoming.Count}):** " + string.Join(", ", allUpcoming.Take(3).Select(t => $"{t.TieuDe} ({t.HanHoanThanh:dd/MM})")));
                    }
                    return sbTaskQuery.ToString().TrimEnd();

                case AiIntents.ScheduleQuery:
                    var label = !string.IsNullOrEmpty(context.TargetDateLabel) ? context.TargetDateLabel : "thời gian được hỏi";
                    var hasClasses = context.ScheduleClasses != null && context.ScheduleClasses.Any();
                    var hasExams = context.ScheduleExams != null && context.ScheduleExams.Any();
                    var hasEvents = context.ScheduleEvents != null && context.ScheduleEvents.Any();

                    bool isAskingExamOnly = qLower.Contains("lich thi") || qLower.Contains("thi cuoi ky") || qLower.Contains("thi giua ky") || qLower.Contains("co lich thi");
                    bool isAskingEventOnly = qLower.Contains("su kien") || qLower.Contains("hoat dong");
                    bool isAskingClassOnly = qLower.Contains("lich hoc") || qLower.Contains("tiet hoc") || qLower.Contains("mon hoc");

                    // 1. If user specifically asked about Exam Schedule
                    if (isAskingExamOnly)
                    {
                        if (!hasExams)
                        {
                            return $"🎯 **{label}:** Bạn không có lịch thi nào trong khoảng thời gian này.";
                        }

                        var sbSchedExam = new StringBuilder();
                        sbSchedExam.AppendLine($"🎯 **Lịch thi của bạn ({label}):**\n");
                        foreach (var ex in context.ScheduleExams!)
                        {
                            sbSchedExam.AppendLine($"• **{ex.TenMonHoc}** — {ex.HinhThucThi} tại phòng `{ex.PhongThi}`");
                        }
                        return sbSchedExam.ToString().TrimEnd();
                    }

                    // 2. If user specifically asked about Events
                    if (isAskingEventOnly)
                    {
                        if (!hasEvents)
                        {
                            return $"📌 **{label}:** Bạn không có sự kiện hay hoạt động nào.";
                        }

                        var sbSchedEvent = new StringBuilder();
                        sbSchedEvent.AppendLine($"📌 **Sự kiện / Hoạt động ({label}):**\n");
                        foreach (var ev in context.ScheduleEvents!)
                        {
                            sbSchedEvent.AppendLine($"• **{ev.TieuDe}** tại `{ev.DiaDiem}` ({ev.ThoiGianBatDau:HH:mm} - {ev.ThoiGianKetThuc:HH:mm})");
                        }
                        return sbSchedEvent.ToString().TrimEnd();
                    }

                    // 3. If user specifically asked about Class Schedule
                    if (isAskingClassOnly)
                    {
                        if (!hasClasses)
                        {
                            return $"📅 **{label}:** Bạn không có lịch học cố định nào trên lớp.";
                        }

                        var sbSchedClass = new StringBuilder();
                        sbSchedClass.AppendLine($"📚 **Lịch học trên lớp ({label}):**\n");
                        foreach (var c in context.ScheduleClasses!)
                        {
                            string thuStr = c.Thu switch { 2 => "Thứ 2", 3 => "Thứ 3", 4 => "Thứ 4", 5 => "Thứ 5", 6 => "Thứ 6", 7 => "Thứ 7", 8 => "Chủ Nhật", _ => "" };
                            var thuDisplay = context.DateScope == "Week" ? $" ({thuStr})" : "";
                            sbSchedClass.AppendLine($"• **{c.TenMonHoc}**{thuDisplay} — Tiết {c.TietBatDau}-{c.TietKetThuc} tại phòng `{c.PhongHoc}` (GV: {c.GiangVien})");
                        }
                        return sbSchedClass.ToString().TrimEnd();
                    }

                    // 4. General query ("có lịch gì", "có gì"): Show combined schedule
                    if (!hasClasses && !hasExams && !hasEvents)
                    {
                        return $"📅 **{label}:** Bạn không có lịch học, lịch thi hay sự kiện nào. Đây là khoảng thời gian lý tưởng để bạn tự học hoặc nghỉ ngơi!";
                    }

                    var sbSched = new StringBuilder();
                    sbSched.AppendLine($"📅 **Lịch trình tổng hợp ({label}):**\n");

                    if (hasClasses)
                    {
                        sbSched.AppendLine("📚 **Lịch học trên lớp:**");
                        foreach (var c in context.ScheduleClasses!)
                        {
                            string thuStr = c.Thu switch { 2 => "Thứ 2", 3 => "Thứ 3", 4 => "Thứ 4", 5 => "Thứ 5", 6 => "Thứ 6", 7 => "Thứ 7", 8 => "Chủ Nhật", _ => "" };
                            var thuDisplay = context.DateScope == "Week" ? $" ({thuStr})" : "";
                            sbSched.AppendLine($"• **{c.TenMonHoc}**{thuDisplay} — Tiết {c.TietBatDau}-{c.TietKetThuc} tại phòng `{c.PhongHoc}` (GV: {c.GiangVien})");
                        }
                        sbSched.AppendLine();
                    }

                    if (hasExams)
                    {
                        sbSched.AppendLine("🎯 **Lịch thi:**");
                        foreach (var ex in context.ScheduleExams!)
                        {
                            sbSched.AppendLine($"• **{ex.TenMonHoc}** — {ex.HinhThucThi} tại phòng `{ex.PhongThi}`");
                        }
                        sbSched.AppendLine();
                    }

                    if (hasEvents)
                    {
                        sbSched.AppendLine("📌 **Sự kiện / Hoạt động:**");
                        foreach (var ev in context.ScheduleEvents!)
                        {
                            sbSched.AppendLine($"• **{ev.TieuDe}** tại `{ev.DiaDiem}` ({ev.ThoiGianBatDau:HH:mm} - {ev.ThoiGianKetThuc:HH:mm})");
                        }
                    }

                    return sbSched.ToString().TrimEnd();

                case AiIntents.TaskPrioritization:
                    var hasOverdue = context.OverdueTasks != null && context.OverdueTasks.Any();
                    var hasToday = context.RelevantTasks != null && context.RelevantTasks.Any();
                    var hasUpcoming = context.UpcomingDeadlines != null && context.UpcomingDeadlines.Any();

                    if (!hasOverdue && !hasToday && !hasUpcoming)
                    {
                        return "🎉 **Tuyệt vời!** Bạn không có công việc nào bị quá hạn hay deadline cận kề lúc này. Hãy dành thời gian tự học hoặc nghỉ ngơi nhé!";
                    }

                    // 1. Determine the top #1 task to recommend doing first
                    DashboardTaskItemDto? topTask = null;
                    string topReason = string.Empty;

                    if (hasOverdue)
                    {
                        topTask = context.OverdueTasks!.First();
                        topReason = $"Task này đã quá hạn từ {topTask.HanHoanThanh:dd/MM/yyyy} nên cần được xử lý trước để loại bỏ công việc tồn đọng.";
                    }
                    else if (hasToday)
                    {
                        topTask = context.RelevantTasks!.First();
                        topReason = $"Task này có deadline là **HÔM NAY** ({topTask.HanHoanThanh:dd/MM/yyyy}), cần hoàn thành ngay để tránh bị quá hạn.";
                    }
                    else if (hasUpcoming)
                    {
                        topTask = context.UpcomingDeadlines!.First();
                        topReason = $"Task này có deadline gần nhất ({topTask.HanHoanThanh:dd/MM/yyyy}).";
                    }

                    // Check if this is a direct/concise question like "Task nào cần ưu tiên trước?"
                    bool isDirectTopQuery = (qNorm.Contains("task nao can uu tien truoc") || qNorm.Contains("task nao uu tien truoc") ||
                                             qNorm.Contains("task nao can lam truoc") || qNorm.Contains("task nao nen lam dau tien") ||
                                             qNorm.Contains("viec nao can lam truoc") || qNorm.Contains("viec nao nen lam truoc") ||
                                             qNorm.Contains("task nao lam truoc") || qNorm.Contains("viec nao lam truoc") ||
                                             qNorm.Contains("toi nen lam task nao truoc") || qNorm.Contains("toi nen lam viec nao truoc") ||
                                             (qNorm.Contains("task nao") && qNorm.Contains("uu tien truoc")) ||
                                             qNorm.Equals("task nao can uu tien truoc") || qNorm.Equals("task nao uu tien truoc") ||
                                             qNorm.Equals("toi nen lam task nao truoc") || qNorm.Equals("viec nao can lam truoc"))
                                             && !qNorm.Contains("hom nay") && !qNorm.Contains("ngay toi") && !qNorm.Contains("tuan nay");

                    if (isDirectTopQuery && topTask != null)
                    {
                        var sbShort = new StringBuilder();
                        sbShort.AppendLine($"👉 **Bạn nên làm \"{topTask.TieuDe}\" trước.**\n");
                        sbShort.AppendLine($"**Lý do:** {topReason}\n");
                        sbShort.AppendLine("Tiếp theo, ưu tiên các task quá hạn còn lại theo deadline gần nhất.");
                        return sbShort.ToString().TrimEnd();
                    }

                    var sbPriority = new StringBuilder();
                    sbPriority.AppendLine("🎯 **Đề xuất thứ tự ưu tiên công việc hôm nay:**\n");

                    if (topTask != null)
                    {
                        sbPriority.AppendLine("🚨 **BẠN NÊN LÀM TRƯỚC TIÊN:**");
                        sbPriority.AppendLine($"👉 **{topTask.TieuDe}** (Môn: {topTask.TenMonHoc ?? "Chung"}, Ưu tiên: {FormatTaskPriority(topTask.DoUuTien)})");
                        sbPriority.AppendLine($"• **Lý do:** {topReason}\n");
                    }

                    // 2. Overdue list
                    if (hasOverdue)
                    {
                        sbPriority.AppendLine("⚠️ **Danh sách việc quá hạn:**");
                        foreach (var t in context.OverdueTasks!)
                        {
                            sbPriority.AppendLine($"• **{t.TieuDe}** (Môn: {t.TenMonHoc ?? "Chung"}) — Quá hạn: `{t.HanHoanThanh:dd/MM/yyyy}` (Ưu tiên: {FormatTaskPriority(t.DoUuTien)})");
                        }
                        sbPriority.AppendLine();
                    }

                    // 3. Today tasks list
                    if (hasToday)
                    {
                        sbPriority.AppendLine("🔴 **Việc cần hoàn thành hôm nay:**");
                        foreach (var t in context.RelevantTasks!)
                        {
                            sbPriority.AppendLine($"• **{t.TieuDe}** (Môn: {t.TenMonHoc ?? "Chung"}) — Hạn: **Hôm nay** (Ưu tiên: {FormatTaskPriority(t.DoUuTien)})");
                        }
                        sbPriority.AppendLine();
                    }

                    // 4. Upcoming tasks list
                    if (hasUpcoming)
                    {
                        sbPriority.AppendLine("⏰ **Nhiệm vụ sắp tới trong những ngày tiếp theo:**");
                        foreach (var t in context.UpcomingDeadlines!.Take(3))
                        {
                            sbPriority.AppendLine($"• **{t.TieuDe}** (Môn: {t.TenMonHoc ?? "Chung"}) — Hạn chót: `{t.HanHoanThanh:dd/MM/yyyy}` (Ưu tiên: {FormatTaskPriority(t.DoUuTien)})");
                        }
                    }

                    return sbPriority.ToString().TrimEnd();

                case AiIntents.ExamPreparation:
                    var sbExam = new StringBuilder();
                    sbExam.AppendLine("📚 **Hướng dẫn chuẩn bị cho kỳ thi:**\n");
                    
                    if (context.RelevantExamSchedules != null && context.RelevantExamSchedules.Any())
                    {
                        sbExam.AppendLine("📅 **Các môn thi sắp tới của bạn:**");
                        foreach (var ex in context.RelevantExamSchedules)
                        {
                            sbExam.AppendLine($"• **{ex.TenMonHoc}** — Ngày thi: `{ex.NgayThi:dd/MM/yyyy}` ({ex.HinhThucThi}, Phòng: {ex.PhongThi})");
                        }
                        sbExam.AppendLine("\n📌 **Lời khuyên ôn tập hôm nay:**\n" +
                                          "• Ưu tiên giải đề thi thử và tổng hợp công thức môn gần nhất.\n" +
                                          "• Áp dụng kỹ thuật lặp lại ngắt quãng (Spaced Repetition) để củng cố kiến thức.");
                    }
                    else
                    {
                        sbExam.AppendLine("Hiện tại hệ thống chưa ghi nhận lịch thi chính thức nào của bạn. Bạn có thể chia sẻ môn thi và ngày thi để mình hỗ trợ lên kế hoạch ôn nhé!");
                    }

                    return sbExam.ToString();

                case AiIntents.TaskHelp:
                    var taskName = context.FocusedTaskTitle ?? "công việc của bạn";
                    var subjectName = context.FocusedTask?.TenMonHoc ?? "Học phần";
                    return $"🚀 **Kế hoạch hành động 4 bước dứt điểm: \"{taskName}\"** (Môn: {subjectName})\n\n" +
                           "1. **Chuẩn bị mục tiêu (10 phút):** Liệt kê 3 tiêu chí cần đạt được của task.\n" +
                           "2. **Phiên Pomodoro 1 (25 phút):** Tập trung giải quyết 60% phần khung cốt lõi.\n" +
                           "3. **Phiên Pomodoro 2 (25 phút):** Hoàn thiện 40% còn lại và rà soát lỗi.\n" +
                           "4. **Hoàn tất:** Đánh dấu hoàn thành trên StudyHub để cập nhật tiến độ!";

                case AiIntents.WorkloadAnalysis:
                    return AnalyzeWorkloadDirectly(context, dashboard, qNorm);

                case AiIntents.StudyRecommendation:
                    return "💡 **Lời khuyên học tập hiệu quả từ StudyHub AI:**\n\n" +
                           "1. **Nguyên lý 25/5 Pomodoro:** Học tập trung 25 phút, nghỉ 5 phút giúp não bộ duy trì trạng thái tiếp thu tối đa.\n" +
                           "2. **Ma trận Eisenhower:** Ưu tiên xử lý các việc 'Khẩn cấp & Quan trọng' vào khung giờ vàng buổi sáng.\n" +
                           "3. **Học chủ động (Feynman Technique):** Cố gắng giải thích lại kiến thức cho bạn bè trong nhóm học tập để củng cố ghi nhớ.";

                default:
                    return "👋 Xin chào! Tôi là Cố vấn học tập thông minh StudyHub. Bạn có thể hỏi tôi về: *Lịch học hôm nay*, *Thứ tự ưu tiên task*, *Giải thích kiến thức Java/SQL*, hoặc *Phân tích mức độ quá tải*!";
            }
        }

        private string AnalyzeWorkloadDirectly(AiContext context, DashboardDto dashboard, string qNorm)
        {
            var total = dashboard.Statistics.TongSoCongViec;
            var pending = dashboard.Statistics.CongViecChuaHoanThanh;
            
            var todayVn = DateTime.UtcNow.AddHours(7).Date;
            var overdueCount = context.OverdueTasks?.Count ?? (dashboard.UpcomingDeadlines?.Count(t => t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date < todayVn) ?? 0);
            var todayCount = context.RelevantTasks?.Count ?? (dashboard.TodayTasks?.Count ?? 0);

            // 1. If user asks specifically for Task Count / Quantity
            bool isCountOnlyQuery = qNorm.Contains("bao nhieu task") || qNorm.Contains("bao nhieu cong viec") || 
                                   qNorm.Contains("bao nhieu viec") || qNorm.Contains("co bao nhieu") || 
                                   qNorm.Contains("con bao nhieu") || qNorm.Contains("dang co bao nhieu") ||
                                   qNorm.Contains("can hoan thanh");

            if (isCountOnlyQuery)
            {
                var sbCount = new StringBuilder();
                sbCount.AppendLine($"📋 Hiện tại bạn có **{pending} công việc chưa hoàn thành** trên tổng số **{total} công việc**.");
                
                if (overdueCount > 0 && todayCount > 0)
                {
                    sbCount.AppendLine($"🚨 Trong đó có **{overdueCount} công việc quá hạn** và **{todayCount} công việc có deadline hôm nay**.");
                }
                else if (overdueCount > 0)
                {
                    sbCount.AppendLine($"🚨 Trong đó có **{overdueCount} công việc đã quá hạn**.");
                }
                else if (todayCount > 0)
                {
                    sbCount.AppendLine($"🔴 Trong đó có **{todayCount} công việc có deadline hôm nay**.");
                }

                return sbCount.ToString().TrimEnd();
            }

            // 2. Overload evaluation queries
            if (overdueCount > 0)
            {
                var todaySuffix = todayCount > 0 ? $" và **{todayCount} công việc có deadline hôm nay**" : "";
                return $"⚠️ Bạn hiện có **{pending} công việc chưa hoàn thành**, trong đó có **{overdueCount} công việc đã quá hạn**{todaySuffix}.\n\n" +
                       $"• **Đánh giá:** Vì bạn đang có {overdueCount} task quá hạn, khối lượng công việc hiện tại cần được ưu tiên xử lý, đặc biệt là các task tồn đọng.\n" +
                       $"• **Khuyến nghị:** Hãy giải quyết các task quá hạn sớm nhất trước để giảm bớt áp lực, sau đó tập trung hoàn thành các công việc tiếp theo.";
            }

            if (todayCount > 2 || pending > 8)
            {
                return $"📊 **Đánh giá Workload: MỨC ĐỘ CAO (HIGH) 🚨**\n\n" +
                       $"• Bạn đang có **{pending} công việc chưa xong**, trong đó có **{todayCount} deadline cận kề**.\n" +
                       $"• **Khuyến nghị:** Tập trung xử lý các task khẩn cấp nhất, chia nhỏ thời gian học thành các phiên 25 phút Pomodoro.";
            }

            return $"📊 **Đánh giá Workload: MỨC ĐỘ CÂN BẰNG (MODERATE / LOW) ✅**\n\n" +
                   $"• Bạn có **{pending}/{total} công việc** đang tiến hành ổn định (không có task nào bị quá hạn).\n" +
                   $"• Hãy duy trì nhịp độ học tập hàng ngày nhé!";
        }

        private List<string> GenerateActionSuggestionsForIntent(string intent)
        {
            switch (intent)
            {
                case AiIntents.ScheduleQuery:
                    return new List<string> { "Lịch học ngày mai", "Lịch thi sắp tới", "Task cần làm hôm nay" };
                case AiIntents.TaskPrioritization:
                    return new List<string> { "Bắt đầu Pomodoro cho task ưu tiên", "Xem deadline môn học", "Phân tích quá tải" };
                case AiIntents.KnowledgeQa:
                    return new List<string> { "Giải thích thêm ví dụ", "Cách tối ưu code", "Bài tập thực hành" };
                case AiIntents.WorkloadAnalysis:
                    return new List<string> { "Lập kế hoạch giảm tải", "Xem task ưu tiên", "Bắt đầu học ngay" };
                default:
                    return new List<string> { "Hôm nay nên học gì?", "Xem deadline sắp tới", "Phân tích mức độ quá tải", "Sinh kế hoạch 7 ngày" };
            }
        }

        private string BuildUserContextPrompt(DashboardDto dashboard)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DỮ LIỆU THỰC TẾ CỦA HỌC SINH/SINH VIÊN TRÊN STUDYHUB:");
            sb.AppendLine($"Họ tên: {dashboard.UserProfile?.HoTen}");
            sb.AppendLine($"Thống kê: {dashboard.Statistics?.TongSoMonHoc} môn học, {dashboard.Statistics?.CongViecHoanThanh}/{dashboard.Statistics?.TongSoCongViec} công việc đã xong, {dashboard.Statistics?.DeadlineHomNay} deadline hôm nay.");
            
            if (dashboard.TodayTasks != null && dashboard.TodayTasks.Any())
            {
                sb.AppendLine("Công việc hôm nay: " + string.Join("; ", dashboard.TodayTasks.Select(t => $"{t.TieuDe} (Ưu tiên: {t.DoUuTien})")));
            }
            if (dashboard.UpcomingDeadlines != null && dashboard.UpcomingDeadlines.Any())
            {
                sb.AppendLine("Deadline sắp tới: " + string.Join("; ", dashboard.UpcomingDeadlines.Select(d => $"{d.TieuDe} (Hạn: {d.HanHoanThanh:dd/MM})")));
            }
            if (dashboard.TodayClassSchedules != null && dashboard.TodayClassSchedules.Any())
            {
                sb.AppendLine("Lịch học hôm nay: " + string.Join("; ", dashboard.TodayClassSchedules.Select(c => $"{c.TenMonHoc} tại {c.PhongHoc}")));
            }
            if (dashboard.NearestExamSchedules != null && dashboard.NearestExamSchedules.Any())
            {
                sb.AppendLine("Lịch thi sắp tới: " + string.Join("; ", dashboard.NearestExamSchedules.Select(e => $"{e.TenMonHoc} ngày {e.NgayThi:dd/MM} ({e.HinhThucThi})")));
            }

            return sb.ToString();
        }

        private async Task<string> CallGeminiApiAsync(string prompt)
        {
            try
            {
                var apiKey = _configuration["Gemini:ApiKey"] ?? _configuration["GoogleApiKey"];
                if (string.IsNullOrEmpty(apiKey) || apiKey.StartsWith("YOUR_"))
                {
                    _logger.LogWarning("Gemini API key is not configured.");
                    return string.Empty;
                }

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";
                var payload = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("x-goog-api-key", apiKey);
                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                var response = await _httpClient.SendAsync(request, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonStr);
                    if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                    {
                        var candidate = candidates[0];
                        if (candidate.TryGetProperty("content", out var contentElem) && contentElem.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                        {
                            return parts[0].GetProperty("text").GetString() ?? string.Empty;
                        }
                    }
                }
                else
                {
                    var errBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Gemini API returned status code {StatusCode}: {ErrorBody}", response.StatusCode, errBody);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
            }

            return string.Empty;
        }

        private string CalculateWorkloadLevel(DashboardDto dashboard)
        {
            if (dashboard.Statistics.DeadlineHomNay > 2 || dashboard.Statistics.CongViecChuaHoanThanh > 8)
                return "High";
            if (dashboard.Statistics.CongViecChuaHoanThanh > 3)
                return "Moderate";
            return "Low";
        }

        private static string FormatTaskPriority(byte priority)
        {
            return priority switch
            {
                0 => "Thấp",
                1 => "Trung bình",
                2 => "Cao",
                3 => "Khẩn cấp",
                _ => "Trung bình"
            };
        }

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var normalized = text.ToLowerInvariant();

            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                {
                    normalized = normalized.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
                }
            }

            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"[^\w\s\d]", " ");
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s+", " ").Trim();

            return normalized;
        }
    }
}
