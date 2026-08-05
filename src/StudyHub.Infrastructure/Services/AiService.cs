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

namespace StudyHub.Infrastructure.Services
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IDashboardService _dashboardService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiService> _logger;

        public AiService(
            HttpClient httpClient,
            IDashboardService dashboardService,
            IConfiguration configuration,
            ILogger<AiService> logger)
        {
            _httpClient = httpClient;
            _dashboardService = dashboardService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AiChatResponse> ChatAsync(int userId, AiChatRequest request)
        {
            var dashboard = await _dashboardService.GetDashboardDataAsync(userId);
            var contextText = BuildUserContextPrompt(dashboard);

            var userQuery = request.Message.Trim();

            // Check intent for pre-built prompt types
            if (string.Equals(request.PromptType, "TodaySchedule", StringComparison.OrdinalIgnoreCase) ||
                userQuery.Contains("lịch hôm nay", StringComparison.OrdinalIgnoreCase))
            {
                userQuery = "Hôm nay tôi có lịch học và sự kiện gì?";
            }
            else if (string.Equals(request.PromptType, "UpcomingDeadlines", StringComparison.OrdinalIgnoreCase) ||
                     userQuery.Contains("deadline sắp tới", StringComparison.OrdinalIgnoreCase))
            {
                userQuery = "Liệt kê các deadline công việc sắp tới cần hoàn thành?";
            }
            else if (string.Equals(request.PromptType, "PriorityTasks", StringComparison.OrdinalIgnoreCase) ||
                     userQuery.Contains("task ưu tiên", StringComparison.OrdinalIgnoreCase) ||
                     userQuery.Contains("hôm nay nên học gì", StringComparison.OrdinalIgnoreCase))
            {
                userQuery = "Dựa trên danh sách môn học và deadline, tôi nên tập trung học và làm task nào ưu tiên hôm nay?";
            }

            var prompt = $"{contextText}\n\nCâu hỏi của sinh viên/học sinh: {userQuery}";
            var aiReply = await CallGeminiApiAsync(prompt);

            if (string.IsNullOrEmpty(aiReply))
            {
                aiReply = GenerateContextualFallbackReply(dashboard, userQuery);
            }

            var workloadLevel = CalculateWorkloadLevel(dashboard);

            return new AiChatResponse
            {
                Reply = aiReply,
                ActionSuggestions = new List<string>
                {
                    "Hôm nay nên học gì?",
                    "Xem deadline sắp tới",
                    "Phân tích mức độ quá tải",
                    "Sinh kế hoạch học tập 7 ngày"
                },
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

            for (int d = 1; d <= days; d++)
            {
                var targetTask = dashboard.UpcomingDeadlines.Skip((d - 1) % Math.Max(1, dashboard.UpcomingDeadlines.Count)).FirstOrDefault();
                var taskName = targetTask != null ? $"Ôn tập & hoàn thành: {targetTask.TieuDe}" : $"Ôn lại kiến thức các môn học ({request.Goal})";
                
                items.Add(new StudyPlanItemDto
                {
                    Day = $"Ngày {d}",
                    TaskName = taskName,
                    Duration = "90 phút",
                    FocusArea = targetTask?.TenMonHoc ?? "Kiến thức chuyên ngành"
                });
            }

            return new StudyPlanResponse
            {
                Title = $"Kế hoạch học tập {request.NumberOfDays} ngày: {request.Goal}",
                Advice = string.IsNullOrEmpty(aiAdvice) ? "Phân bổ thời gian học tập 90-120 phút mỗi ngày. Nghỉ giải lao 10 phút sau mỗi 25 phút Pomodoro." : aiAdvice,
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

        private string BuildUserContextPrompt(DashboardDto dashboard)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DỮ LIỆU THỰC TẾ CỦA HỌC SINH/SINH VIÊN TRÊN STUDYHUB:");
            sb.AppendLine($"Họ tên: {dashboard.UserProfile.HoTen}");
            sb.AppendLine($"Thống kê: {dashboard.Statistics.TongSoMonHoc} môn học, {dashboard.Statistics.CongViecHoanThanh}/{dashboard.Statistics.TongSoCongViec} công việc đã xong, {dashboard.Statistics.DeadlineHomNay} deadline hôm nay.");
            
            if (dashboard.TodayTasks.Any())
            {
                sb.AppendLine("Công việc hôm nay: " + string.Join("; ", dashboard.TodayTasks.Select(t => $"{t.TieuDe} (Ưu tiên: {t.DoUuTien})")));
            }
            if (dashboard.UpcomingDeadlines.Any())
            {
                sb.AppendLine("Deadline sắp tới: " + string.Join("; ", dashboard.UpcomingDeadlines.Select(d => $"{d.TieuDe} (Hạn: {d.HanHoanThanh:dd/MM})")));
            }
            if (dashboard.TodayClassSchedules.Any())
            {
                sb.AppendLine("Lịch học hôm nay: " + string.Join("; ", dashboard.TodayClassSchedules.Select(c => $"{c.TenMonHoc} tại {c.PhongHoc}")));
            }
            if (dashboard.NearestExamSchedules.Any())
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
                if (string.IsNullOrEmpty(apiKey))
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

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonStr);
                    var candidates = doc.RootElement.GetProperty("candidates");
                    if (candidates.GetArrayLength() > 0)
                    {
                        var parts = candidates[0].GetProperty("content").GetProperty("parts");
                        if (parts.GetArrayLength() > 0)
                        {
                            return parts[0].GetProperty("text").GetString() ?? string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
            }

            return string.Empty;
        }

        private string GenerateContextualFallbackReply(DashboardDto dashboard, string userQuery)
        {
            if (userQuery.Contains("lịch", StringComparison.OrdinalIgnoreCase))
            {
                if (!dashboard.TodayClassSchedules.Any())
                    return "Hôm nay bạn không có lịch học cố định nào trên lớp.";
                return "Lịch học hôm nay của bạn:\n" + string.Join("\n", dashboard.TodayClassSchedules.Select(c => $"• {c.TenMonHoc} (Phòng {c.PhongHoc}) - Giảng viên: {c.GiangVien}"));
            }

            if (userQuery.Contains("deadline", StringComparison.OrdinalIgnoreCase))
            {
                if (!dashboard.UpcomingDeadlines.Any())
                    return "Bạn hiện không có deadline nào sắp tới trong danh sách.";
                return "Các deadline sắp tới cần lưu ý:\n" + string.Join("\n", dashboard.UpcomingDeadlines.Select(d => $"• {d.TieuDe} (Môn {d.TenMonHoc ?? "Chung"}) - Hạn: {d.HanHoanThanh:dd/MM/yyyy}"));
            }

            if (userQuery.Contains("học gì", StringComparison.OrdinalIgnoreCase) || userQuery.Contains("ưu tiên", StringComparison.OrdinalIgnoreCase))
            {
                var topTask = dashboard.UpcomingDeadlines.FirstOrDefault() ?? dashboard.TodayTasks.FirstOrDefault();
                if (topTask != null)
                {
                    return $"Dựa trên phân tích dữ liệu thực tế, hôm nay bạn nên tập trung làm task: '{topTask.TieuDe}' (Môn: {topTask.TenMonHoc ?? "Chưa rõ"}). Hãy sử dụng kỹ thuật Pomodoro 25 phút để đạt hiệu quả tối đa!";
                }
                return "Hiện tại các công việc của bạn đều ổn định. Bạn có thể dành thời gian đọc tài liệu mới hoặc luyện tập thêm!";
            }

            return $"Dựa trên thông tin hiện tại ({dashboard.Statistics.TongSoMonHoc} môn học, {dashboard.Statistics.CongViecChuaHoanThanh} task chưa xong), tôi khuyên bạn nên kiểm tra danh sách deadline và hoàn thành các nhiệm vụ ưu tiên trước.";
        }

        private string CalculateWorkloadLevel(DashboardDto dashboard)
        {
            if (dashboard.Statistics.DeadlineHomNay > 2 || dashboard.Statistics.CongViecChuaHoanThanh > 8)
                return "High";
            if (dashboard.Statistics.CongViecChuaHoanThanh > 3)
                return "Moderate";
            return "Low";
        }
    }
}
