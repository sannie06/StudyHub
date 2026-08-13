using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StudyHub.Infrastructure.Services.Ai
{
    public static class AiIntents
    {
        public const string GeneralChat = "GENERAL_CHAT";
        public const string KnowledgeQa = "KNOWLEDGE_QA";
        public const string TaskPrioritization = "TASK_PRIORITIZATION";
        public const string TaskQuery = "TASK_QUERY";
        public const string TaskHelp = "TASK_HELP";
        public const string WorkloadAnalysis = "WORKLOAD_ANALYSIS";
        public const string StudyPlan = "STUDY_PLAN";
        public const string ScheduleQuery = "SCHEDULE_QUERY";
        public const string ExamPreparation = "EXAM_PREPARATION";
        public const string StudyRecommendation = "STUDY_RECOMMENDATION";
        public const string ClarificationRequired = "CLARIFICATION_REQUIRED";
        public const string DocumentQa = "DOCUMENT_QA";
    }

    public class AiIntentResult
    {
        public string Intent { get; set; } = AiIntents.GeneralChat;
        public double Confidence { get; set; } = 1.0;
        public string? ExtractedTaskTitle { get; set; }
        public string? ExtractedSubjectName { get; set; }
        public int? ExtractedNumberOfDays { get; set; }
        public string NormalizedQuery { get; set; } = string.Empty;
        public AiDateResolution? DateResolution { get; set; }
    }

    public class AiIntentDetector
    {
        private readonly AiDateResolver _dateResolver = new();

        public AiIntentResult DetectIntent(string rawMessage, string? promptType = null)
        {
            var message = (rawMessage ?? string.Empty).Trim();
            var normalized = Normalize(message);

            var result = new AiIntentResult
            {
                NormalizedQuery = normalized,
                DateResolution = _dateResolver.Resolve(message)
            };

            // 1. Honor explicit prompt types from UI buttons if specified
            if (!string.IsNullOrEmpty(promptType))
            {
                switch (promptType.ToLowerInvariant())
                {
                    case "todayschedule":
                        result.Intent = AiIntents.ScheduleQuery;
                        return result;
                    case "upcomingdeadlines":
                        result.Intent = AiIntents.TaskQuery;
                        return result;
                    case "prioritytasks":
                        result.Intent = AiIntents.TaskPrioritization;
                        return result;
                    case "workloadanalysis":
                        result.Intent = AiIntents.WorkloadAnalysis;
                        return result;
                }
            }

            // Extract potential target task in quotes: "Task name"
            var quoteMatch = Regex.Match(message, "\"([^\"]+)\"");
            if (quoteMatch.Success)
            {
                result.ExtractedTaskTitle = quoteMatch.Groups[1].Value.Trim();
            }

            // Extract potential days: "3 ngày", "5 ngày", "7 ngày"
            var daysMatch = Regex.Match(normalized, @"(\d+)\s*(ngay|day)");
            if (daysMatch.Success && int.TryParse(daysMatch.Groups[1].Value, out var days))
            {
                result.ExtractedNumberOfDays = days;
            }

            // 2. Score intents using pattern matching and keyword analysis
            var scores = new Dictionary<string, int>
            {
                [AiIntents.ScheduleQuery] = ScoreScheduleQuery(normalized),
                [AiIntents.TaskPrioritization] = ScoreTaskPrioritization(normalized),
                [AiIntents.TaskQuery] = ScoreTaskQuery(normalized),
                [AiIntents.TaskHelp] = ScoreTaskHelp(normalized, quoteMatch.Success),
                [AiIntents.WorkloadAnalysis] = ScoreWorkloadAnalysis(normalized),
                [AiIntents.ExamPreparation] = ScoreExamPreparation(normalized),
                [AiIntents.StudyPlan] = ScoreStudyPlan(normalized),
                [AiIntents.StudyRecommendation] = ScoreStudyRecommendation(normalized),
                [AiIntents.KnowledgeQa] = ScoreKnowledgeQa(normalized),
                [AiIntents.DocumentQa] = ScoreDocumentQa(normalized)
            };

            // Find highest scored intent
            var highest = AiIntents.GeneralChat;
            var maxScore = 0;

            foreach (var kvp in scores)
            {
                if (kvp.Value > maxScore)
                {
                    maxScore = kvp.Value;
                    highest = kvp.Key;
                }
            }

            // Set result intent if score threshold met
            result.Intent = maxScore >= 2 ? highest : AiIntents.GeneralChat;
            result.Confidence = maxScore >= 5 ? 0.95 : (maxScore >= 2 ? 0.75 : 0.5);

            return result;
        }

        private static int ScoreScheduleQuery(string q)
        {
            int s = 0;
            if (q.Contains("lich hoc") || q.Contains("thoi khoa bieu") || q.Contains("lich ngay mai") || q.Contains("lich hom nay")) s += 5;
            if (q.Contains("ngay mai toi co") || q.Contains("ngay mai co lich") || q.Contains("hom nay co lich") || q.Contains("co lich gi")) s += 4;
            if (q.Contains("phong hoc") || q.Contains("tiet hoc") || q.Contains("gio hoc") || q.Contains("lich thi")) s += 3;
            return s;
        }

        private static int ScoreTaskQuery(string q)
        {
            int s = 0;
            // Overdue task query
            if (q.Contains("co task nao qua han") || q.Contains("co viec nao qua han") || q.Contains("task nao dang qua han") || 
                q.Contains("viec nao dang qua han") || q.Contains("danh sach task qua han") || q.Contains("danh sach viec qua han") || 
                q.Contains("cong viec qua han") || q.Contains("task qua han") || q.Contains("da qua deadline") || q.Contains("da qua han"))
            {
                s += 7;
            }
            // Today task query
            if (q.Contains("co task nao can lam") || q.Contains("co task nao hom nay") || q.Contains("danh sach task hom nay") || 
                q.Contains("danh sach cong viec hom nay") || q.Contains("hom nay co task nao") || q.Contains("hom nay co viec gi") || 
                q.Contains("task can lam hom nay") || q.Contains("task hom nay") || q.Contains("viec can lam hom nay") ||
                q.Contains("can lam hom nay"))
            {
                s += 7;
            }
            // Remaining / Uncompleted tasks query
            if (q.Contains("chua hoan thanh") || q.Contains("chua xong") || q.Contains("chua lam") || 
                q.Contains("con nhung viec") || q.Contains("con task nao") || q.Contains("con viec nao") || 
                q.Contains("con viec gi") || q.Contains("toi con") || q.Contains("hom nay con") || 
                q.Contains("viec chua lam") || q.Contains("task chua lam") || q.Contains("nhung viec nao"))
            {
                s += 7;
            }
            // General task list & Upcoming Deadlines query
            if (q.Contains("danh sach task") || q.Contains("danh sach cong viec") || q.Contains("co nhung task nao") || 
                q.Contains("co nhung cong viec nao") || q.Contains("xem deadline") || q.Contains("deadline sap toi"))
            {
                s += 6;
            }
            return s;
        }

        private static int ScoreTaskPrioritization(string q)
        {
            int s = 0;
            // Direct & Action-oriented prioritization queries (e.g. "Gợi ý ưu tiên công việc", "Hôm nay tôi nên làm gì?", "Tôi nên làm gì trước?", "Việc nào cần làm ngay?")
            if (q.Contains("nen lam task nao") || q.Contains("nen lam gi truoc") || q.Contains("nen lam gi") ||
                q.Contains("toi nen lam gi") || q.Contains("hom nay nen lam gi") || q.Contains("lam gi hom nay") ||
                q.Contains("goi y uu tien") || q.Contains("uu tien cong viec") || q.Contains("de xuat uu tien") ||
                q.Contains("uu tien truoc") || q.Contains("task uu tien") || q.Contains("nen uu tien") || 
                q.Contains("uu tien task nao") || q.Contains("uu tien viec nao") || q.Contains("can uu tien truoc") ||
                q.Contains("can lam ngay") || q.Contains("lam ngay") || q.Contains("khan cap nhat") ||
                q.Contains("quan trong nhat") || q.Contains("gap nhat") || q.Contains("task nao quan trong") ||
                q.Contains("task nao gap"))
            {
                s += 6;
            }
            // Inverted sentence structures & direct short questions
            if (q.Contains("task nao toi nen lam truoc") || q.Contains("task nao nen lam truoc") || 
                q.Contains("task nao can uu tien truoc") || q.Contains("task nao uu tien truoc") ||
                q.Contains("task nao can lam truoc") || q.Contains("task nao nen lam dau tien") ||
                q.Contains("viec nao nen lam truoc") || q.Contains("viec nao can lam truoc") ||
                q.Contains("viec nao can lam ngay") || q.Contains("viec nao quan trong") ||
                q.Contains("nen lam truoc") || q.Contains("task nao lam truoc") || 
                q.Contains("nen chon task nao") || q.Contains("tap trung vao task nao"))
            {
                s += 6;
            }
            // Time-scoped prioritization (e.g. "trong 3 ngay toi", "tuan nay nen lam task nao")
            if ((q.Contains("ngay toi") || q.Contains("ngay nua") || q.Contains("tuan nay")) && (q.Contains("uu tien") || q.Contains("nen lam") || q.Contains("tap trung")))
            {
                s += 6;
            }
            if (q.Contains("uu tien xu ly task qua han") || q.Contains("uu tien task qua han nao") ||
                (q.Contains("qua han") && (q.Contains("nen lam task nao") || q.Contains("nen lam gi") || q.Contains("lam task nao truoc"))))
            {
                s += 7;
            }
            if (q.Contains("deadline nao gan") || q.Contains("sap het han") || q.Contains("can hoan thanh truoc"))
            {
                s += 3;
            }
            return s;
        }

        private static int ScoreTaskHelp(string q, bool hasQuotedTask)
        {
            int s = 0;
            if (hasQuotedTask && (q.Contains("hoan thanh cong viec") || q.Contains("huong dan toi") || q.Contains("ke hoach de hoan thanh"))) s += 7;
            if (q.Contains("task nay toi phai lam the nao") || q.Contains("task nay lam the nao") || q.Contains("giup toi lam task")) s += 6;
            if (q.Contains("lam the nao de hoan thanh task") || q.Contains("cach lam task")) s += 5;
            return s;
        }

        private static int ScoreWorkloadAnalysis(string q)
        {
            int s = 0;
            // Overload status queries
            if (q.Contains("qua tai") || q.Contains("co dang qua tai") || q.Contains("toi dang qua tai") || 
                q.Contains("bi qua tai") || q.Contains("co bi qua tai") || q.Contains("co qua tai khong"))
            {
                s += 6;
            }
            // Many tasks / heavy workload queries
            if (q.Contains("qua nhieu viec") || q.Contains("co qua nhieu viec") || q.Contains("qua nhieu task") || 
                q.Contains("co qua nhieu task") || q.Contains("nhieu task qua") || q.Contains("nhieu task khong") || 
                q.Contains("co nhieu task") || q.Contains("co nhieu viec") || q.Contains("khoi luong cong viec") || 
                q.Contains("khoi luong task") || q.Contains("tinh trang cong viec") || q.Contains("phan tich tien do") ||
                q.Contains("tien do hoc tap"))
            {
                s += 6;
            }
            // Count / Workload statistics queries
            if (q.Contains("bao nhieu task") || q.Contains("bao nhieu cong viec") || q.Contains("bao nhieu viec") || 
                q.Contains("co bao nhieu task") || q.Contains("co bao nhieu viec") || q.Contains("con bao nhieu task") || 
                q.Contains("con bao nhieu cong viec") || q.Contains("con bao nhieu viec") || q.Contains("tong so task") ||
                q.Contains("tong so cong viec") || q.Contains("dang co bao nhieu") || q.Contains("con bao nhieu"))
            {
                s += 6;
            }
            if (q.Contains("ap luc") || q.Contains("danh gia tien do") || q.Contains("danh gia workload"))
            {
                s += 4;
            }
            return s;
        }

        private static int ScoreExamPreparation(string q)
        {
            int s = 0;
            if (q.Contains("sap thi") || q.Contains("on thi") || q.Contains("thi cuoi ky") || q.Contains("thi giua ky") || q.Contains("chuan bi cho ky thi")) s += 6;
            if (q.Contains("lich thi sap toi") || q.Contains("chuan bi thi") || q.Contains("on tap mon") || q.Contains("toi thi")) s += 5;
            if (q.Contains("ngay thi") || q.Contains("moi ngay hoc")) s += 4;
            return s;
        }

        private static int ScoreStudyPlan(string q)
        {
            int s = 0;
            if (q.Contains("lap ke hoach") || q.Contains("ke hoach hoc tap") || q.Contains("ke hoach on tap") || q.Contains("len lich hoc")) s += 6;
            if (q.Contains("ke hoach on thi") || q.Contains("lo trinh hoc")) s += 5;
            if (q.Contains("phan bo thoi gian") || q.Contains("chia thoi gian hoc")) s += 3;
            return s;
        }

        private static int ScoreStudyRecommendation(string q)
        {
            int s = 0;
            if (q.Contains("goi y cach hoc") || q.Contains("phuong phap hoc") || q.Contains("hoc hieu qua") || q.Contains("cach hoc tot") ||
                q.Contains("hom nay nen hoc gi") || q.Contains("nen hoc gi hom nay") || q.Contains("nen hoc gi"))
            {
                s += 6;
            }
            if (q.Contains("loi khuyen hoc tap") || q.Contains("kinh nghiem hoc") || q.Contains("cach tap trung hoc")) s += 5;
            if (q.Contains("pomodoro la gi") || q.Contains("cach chia pomodoro")) s += 4;
            return s;
        }

        private static int ScoreKnowledgeQa(string q)
        {
            int s = 0;
            // Prioritization methodology & theoretical questions
            if (q.Contains("deadline hay do uu tien") || q.Contains("uu tien deadline hay") || 
                q.Contains("han hom nay nhung uu tien thap") || q.Contains("uu tien thap hay") || 
                q.Contains("uu tien cao nen lam truoc") || q.Contains("ma tran eisenhower") || 
                q.Contains("theo eisenhower") || q.Contains("neu deadline hom nay nhung uu tien thap") ||
                (q.Contains("deadline") && q.Contains("do uu tien") && q.Contains("quan trong hon")) ||
                (q.Contains("task qua han") && (q.Contains("co phai") || q.Contains("luon uu tien") || q.Contains("cao nhat khong"))))
            {
                s += 8;
            }
            if (q.Contains("la gi") || q.Contains("the nao la") || q.Contains("giai thich") || q.Contains("dinh nghia")) s += 6;
            if (q.Contains("interface") || q.Contains("abstract class") || q.Contains("join") || q.Contains("inner join") || q.Contains("sql") || q.Contains("java") || q.Contains("dijkstra") || q.Contains("algorithm")) s += 5;
            if (q.Contains("khac") && (q.Contains("nhu the nao") || q.Contains("khac nhau") || q.Contains("so sanh"))) s += 6;
            if (q.Contains("cho toi vi du") || q.Contains("vi du ve") || q.Contains("code vi du") || q.Contains("cho vi du")) s += 6;
            if (q.Contains("cach viet code") || q.Contains("cu phap") || q.Contains("su khac nhau giua") || q.Contains("phan biet")) s += 5;
            if (q.Contains("fix backend") || q.Contains("fix api") || q.Contains("huong dan code")) s += 4;
            return s;
        }

        private static int ScoreDocumentQa(string q)
        {
            int s = 0;
            if (q.Contains("trong tai lieu") || q.Contains("file de cuong") || q.Contains("tai lieu da tai") || q.Contains("tom tat file")) s += 6;
            return s;
        }

        private static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var normalized = text.ToLowerInvariant();

            // Replace Vietnamese diacritics for robust matching
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

            // Remove excessive special characters
            normalized = Regex.Replace(normalized, @"[^\w\s\d]", " ");
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

            return normalized;
        }
    }
}
