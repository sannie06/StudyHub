using System;
using System.Collections.Generic;
using System.Linq;
using StudyHub.Application.DTOs.Dashboard;

namespace StudyHub.Infrastructure.Services.Ai
{
    public class AiContextBuilder
    {
        public AiContext BuildContext(AiIntentResult intentResult, DashboardDto dashboard, int userId, AiScheduleContextResult? scheduleContext = null)
        {
            var context = new AiContext
            {
                Intent = intentResult.Intent,
                RawMessage = intentResult.NormalizedQuery,
                NormalizedQuery = intentResult.NormalizedQuery,
                UserProfile = dashboard.UserProfile != null ? new DashboardUserProfileDto
                {
                    MaNguoiDung = dashboard.UserProfile.MaNguoiDung,
                    HoTen = dashboard.UserProfile.HoTen,
                    VaiTro = dashboard.UserProfile.VaiTro
                } : null
            };

            var vnNow = DateTime.UtcNow.AddHours(7);
            var today = vnNow.Date;

            switch (intentResult.Intent)
            {
                case AiIntents.KnowledgeQa:
                case AiIntents.GeneralChat:
                    // Only user profile minimal data is needed
                    break;

                case AiIntents.ScheduleQuery:
                    if (scheduleContext != null)
                    {
                        context.TargetDate = scheduleContext.TargetStartDate;
                        context.TargetDateLabel = scheduleContext.TargetDateLabel;
                        context.DateScope = scheduleContext.Scope;
                        context.ScheduleClasses = scheduleContext.Classes;
                        context.ScheduleExams = scheduleContext.Exams;
                        context.ScheduleEvents = scheduleContext.Events;
                        context.ScheduleDeadlines = scheduleContext.Deadlines;
                    }
                    break;

                case AiIntents.TaskQuery:
                    context.Statistics = dashboard.Statistics;
                    var allQueryDeadlines = dashboard.UpcomingDeadlines ?? new List<DashboardTaskItemDto>();

                    var queryNormalized = intentResult.NormalizedQuery;
                    bool isAskingOverdueOnly = (queryNormalized.Contains("qua han") || queryNormalized.Contains("qua deadline")) && !queryNormalized.Contains("hom nay");
                    bool isAskingTodayOrAction = queryNormalized.Contains("hom nay") || queryNormalized.Contains("can lam") || 
                                                queryNormalized.Contains("chua hoan thanh") || queryNormalized.Contains("chua xong") || 
                                                queryNormalized.Contains("chua lam") || queryNormalized.Contains("con nhung") || 
                                                queryNormalized.Contains("con task") || queryNormalized.Contains("con viec") ||
                                                queryNormalized.Contains("toi con");

                    if (isAskingOverdueOnly)
                    {
                        // 1. Only Overdue tasks
                        context.OverdueTasks = allQueryDeadlines
                            .Where(t => t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date < today)
                            .OrderBy(t => t.HanHoanThanh)
                            .ThenByDescending(t => t.DoUuTien)
                            .ToList();
                    }
                    else if (isAskingTodayOrAction)
                    {
                        // 2. Tasks needed today: Both Overdue and Today's deadlines
                        context.OverdueTasks = allQueryDeadlines
                            .Where(t => t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date < today)
                            .OrderBy(t => t.HanHoanThanh)
                            .ThenByDescending(t => t.DoUuTien)
                            .ToList();

                        context.RelevantTasks = (dashboard.TodayTasks != null && dashboard.TodayTasks.Any())
                            ? dashboard.TodayTasks.Where(t => t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date == today).OrderByDescending(t => t.DoUuTien).ToList()
                            : allQueryDeadlines.Where(t => t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date == today).OrderByDescending(t => t.DoUuTien).ToList();
                    }
                    else
                    {
                        // 3. General task query: Provide all active groups
                        context.OverdueTasks = allQueryDeadlines
                            .Where(t => t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date < today)
                            .OrderBy(t => t.HanHoanThanh)
                            .ToList();

                        context.RelevantTasks = allQueryDeadlines
                            .Where(t => t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date == today)
                            .OrderByDescending(t => t.DoUuTien)
                            .ToList();

                        context.UpcomingDeadlines = allQueryDeadlines
                            .Where(t => t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date > today)
                            .OrderBy(t => t.HanHoanThanh)
                            .ToList();
                    }
                    break;

                case AiIntents.TaskPrioritization:
                    context.Statistics = dashboard.Statistics;
                    var allDeadlines = dashboard.UpcomingDeadlines ?? new List<DashboardTaskItemDto>();

                    // 1. Overdue tasks: deadline < today
                    context.OverdueTasks = allDeadlines
                        .Where(t => t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date < today)
                        .OrderBy(t => t.HanHoanThanh)
                        .ThenByDescending(t => t.DoUuTien)
                        .ToList();

                    // 2. Today tasks: deadline == today
                    context.RelevantTasks = (dashboard.TodayTasks != null && dashboard.TodayTasks.Any())
                        ? dashboard.TodayTasks.Where(t => t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date == today).OrderByDescending(t => t.DoUuTien).ToList()
                        : allDeadlines.Where(t => t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date == today).OrderByDescending(t => t.DoUuTien).ToList();

                    // 3. Upcoming future tasks: deadline > today (Respect explicit time range if present, e.g. "trong 3 ngày tới")
                    if (intentResult.ExtractedNumberOfDays.HasValue && intentResult.ExtractedNumberOfDays.Value > 0)
                    {
                        var maxScopeDate = today.AddDays(intentResult.ExtractedNumberOfDays.Value);
                        context.TargetDateLabel = $"trong {intentResult.ExtractedNumberOfDays.Value} ngày tới ({today:dd/MM} - {maxScopeDate:dd/MM/yyyy})";
                        context.UpcomingDeadlines = allDeadlines
                            .Where(t => t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date > today && t.HanHoanThanh.Value.Date <= maxScopeDate)
                            .OrderBy(t => t.HanHoanThanh)
                            .ThenByDescending(t => t.DoUuTien)
                            .ToList();
                    }
                    else
                    {
                        context.UpcomingDeadlines = allDeadlines
                            .Where(t => t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date > today)
                            .OrderBy(t => t.HanHoanThanh)
                            .ThenByDescending(t => t.DoUuTien)
                            .ToList();
                    }
                    break;

                case AiIntents.TaskHelp:
                    var taskTitle = intentResult.ExtractedTaskTitle;
                    
                    if (!string.IsNullOrEmpty(taskTitle))
                    {
                        var matchedTask = dashboard.TodayTasks?.FirstOrDefault(t => t.TieuDe.Equals(taskTitle, StringComparison.OrdinalIgnoreCase))
                                       ?? dashboard.UpcomingDeadlines?.FirstOrDefault(t => t.TieuDe.Equals(taskTitle, StringComparison.OrdinalIgnoreCase));
                        
                        context.FocusedTask = matchedTask;
                        context.FocusedTaskTitle = taskTitle;
                    }
                    else
                    {
                        // User asked "Task này tôi phải làm the nao?" without specifying the task
                        context.IsClarificationNeeded = true;
                        context.Intent = AiIntents.ClarificationRequired;
                        context.ClarificationReason = "Missing task name";
                        context.ClarificationPrompt = "Bạn muốn tôi hướng dẫn hoàn thành nhiệm vụ nào? Hãy nhập tên công việc hoặc chọn một nhiệm vụ dưới đây nhé!";
                        
                        if (dashboard.UpcomingDeadlines != null && dashboard.UpcomingDeadlines.Any())
                        {
                            context.SuggestedClarificationOptions = dashboard.UpcomingDeadlines.Take(3).Select(t => t.TieuDe).ToList();
                        }
                    }
                    break;

                case AiIntents.WorkloadAnalysis:
                    context.Statistics = dashboard.Statistics;
                    var allWorkloadTasks = dashboard.UpcomingDeadlines ?? new List<DashboardTaskItemDto>();
                    context.OverdueTasks = allWorkloadTasks.Where(t => t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date < today).ToList();
                    context.RelevantTasks = dashboard.TodayTasks?.ToList() ?? new List<DashboardTaskItemDto>();
                    context.UpcomingDeadlines = allWorkloadTasks.Where(t => t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date >= today).ToList();
                    context.RelevantExamSchedules = dashboard.NearestExamSchedules?.ToList() ?? new List<DashboardExamScheduleItemDto>();
                    break;

                case AiIntents.ExamPreparation:
                    context.RelevantExamSchedules = dashboard.NearestExamSchedules?.ToList() ?? new List<DashboardExamScheduleItemDto>();
                    context.UpcomingDeadlines = dashboard.UpcomingDeadlines?.ToList() ?? new List<DashboardTaskItemDto>();
                    context.RelevantTasks = dashboard.TodayTasks?.ToList() ?? new List<DashboardTaskItemDto>();
                    
                    // Check if question is a general exam preparation inquiry like "Hôm nay tôi nên học gì để chuẩn bị cho kỳ thi?"
                    if (intentResult.NormalizedQuery.Contains("hom nay") || intentResult.NormalizedQuery.Contains("chuan bi cho ky thi"))
                    {
                        // Direct preparation guidance based on actual upcoming exams in DB
                        break;
                    }

                    // Check if user provided partial info (e.g. "Tôi thi Toán ngày 20/08, mỗi ngày học 2 tiếng") but missing syllabus
                    if (intentResult.NormalizedQuery.Contains("20 08") || (intentResult.NormalizedQuery.Contains("ngay") && intentResult.NormalizedQuery.Contains("tieng")))
                    {
                        context.IsClarificationNeeded = true;
                        context.Intent = AiIntents.ClarificationRequired;
                        context.ClarificationReason = "Missing syllabus scope";
                        context.MissingInformation.Add("Phạm vi đề cương/chương thi");
                        context.ClarificationPrompt = "Tuyệt vời! Bạn có lịch thi ngày 20/08 và dành 2 giờ học mỗi ngày. Để mình phân bổ kế hoạch từng ngày chính xác nhất, bạn hãy cho mình biết thêm phạm vi kiến thức/đề cương gồm những chương nào nhé!";
                        context.SuggestedClarificationOptions.AddRange(new[] { "Ôn từ chương 1 đến 4", "Ôn phần bài tập lớn", "Toàn bộ đề cương" });
                        break;
                    }

                    // Check if a specific subject was mentioned (e.g. "Toán")
                    var matchedExam = dashboard.NearestExamSchedules?
                        .FirstOrDefault(e => intentResult.NormalizedQuery.Contains(e.TenMonHoc.ToLowerInvariant()));

                    if (matchedExam != null)
                    {
                        context.FocusedExam = matchedExam;
                        context.RelevantTasks = dashboard.UpcomingDeadlines?
                            .Where(t => string.Equals(t.TenMonHoc, matchedExam.TenMonHoc, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                    }
                    else
                    {
                        // Subject is missing in database
                        if (intentResult.NormalizedQuery.Contains("toan") || intentResult.NormalizedQuery.Contains("mon"))
                        {
                            context.IsClarificationNeeded = true;
                            context.Intent = AiIntents.ClarificationRequired;
                            context.ClarificationReason = "Missing exam details in database";
                            context.MissingInformation.AddRange(new[] { "Ngày thi chính thức", "Phạm vi kiến thức thi", "Thời gian rảnh mỗi ngày" });
                            context.ClarificationPrompt = "Để mình có thể lập kế hoạch ôn thi sát thực tế nhất cho bạn, bạn chia sẻ thêm một số thông tin nhé:\n1. Ngày thi chính thức của bạn là ngày nào?\n2. Phạm vi kiến thức gồm những chương/phần nào?\n3. Mỗi ngày bạn có thể dành khoảng bao nhiêu giờ để ôn?";
                            context.SuggestedClarificationOptions.AddRange(new[] { "Thi vào tuần sau", "Ôn từ đầu đến cuối", "Dành 2 giờ/ngày" });
                        }
                    }
                    break;

                case AiIntents.StudyPlan:
                    context.UpcomingDeadlines = dashboard.UpcomingDeadlines?.ToList() ?? new List<DashboardTaskItemDto>();
                    context.RelevantExamSchedules = dashboard.NearestExamSchedules?.ToList() ?? new List<DashboardExamScheduleItemDto>();
                    context.RelevantClassSchedules = dashboard.TodayClassSchedules?.ToList() ?? new List<DashboardClassScheduleItemDto>();

                    // Check if question asks to plan for an exam without details (e.g., "Lập kế hoạch ôn thi Toán trong 5 ngày")
                    if (intentResult.NormalizedQuery.Contains("on thi") || intentResult.NormalizedQuery.Contains("toan"))
                    {
                        var hasSubjectInDb = dashboard.NearestExamSchedules?.Any(e => intentResult.NormalizedQuery.Contains(e.TenMonHoc.ToLowerInvariant())) ?? false;
                        
                        if (!hasSubjectInDb)
                        {
                            context.IsClarificationNeeded = true;
                            context.Intent = AiIntents.ClarificationRequired;
                            context.ClarificationReason = "Missing syllabus scope and exam date";
                            context.MissingInformation.AddRange(new[] { "Phạm vi đề cương/chương thi", "Thời gian học mỗi ngày (giờ)", "Mục tiêu điểm số" });
                            context.ClarificationPrompt = "Để mình lập kế hoạch ôn thi trong " + (intentResult.ExtractedNumberOfDays ?? 5) + " ngày chính xác và hiệu quả nhất, bạn hãy cho mình biết thêm:\n1. Phạm vi đề cương gồm những chương/chủ đề cụ thể nào?\n2. Bạn có thể dành khoảng bao nhiêu giờ mỗi ngày để ôn?\n3. Ngày thi cụ thể là khi nào?";
                            context.SuggestedClarificationOptions.AddRange(new[] { "Ôn 2 giờ mỗi ngày", "Ôn toàn bộ đề cương", "Cần qua môn an toàn" });
                        }
                    }
                    break;

                case AiIntents.StudyRecommendation:
                    context.Statistics = dashboard.Statistics;
                    context.UpcomingDeadlines = dashboard.UpcomingDeadlines?.ToList() ?? new List<DashboardTaskItemDto>();
                    context.RelevantExamSchedules = dashboard.NearestExamSchedules?.ToList() ?? new List<DashboardExamScheduleItemDto>();
                    break;

                case AiIntents.DocumentQa:
                    context.RelevantDocuments = dashboard.LatestDocuments?.ToList() ?? new List<DashboardDocumentItemDto>();
                    break;
            }

            return context;
        }
    }
}
