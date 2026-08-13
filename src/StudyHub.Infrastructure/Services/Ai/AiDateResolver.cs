using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace StudyHub.Infrastructure.Services.Ai
{
    public class AiDateResolution
    {
        public DateTime TargetStartDate { get; set; }
        public DateTime TargetEndDate { get; set; }
        public byte TargetThu { get; set; }
        public string Scope { get; set; } = "SingleDay"; // SingleDay, Week, Range
        public string Label { get; set; } = string.Empty;
        public string QueryType { get; set; } = "All"; // All, ClassSchedule, ExamSchedule, Event
    }

    public class AiDateResolver
    {
        public AiDateResolution Resolve(string rawMessage)
        {
            var vnNow = DateTime.UtcNow.AddHours(7);
            var today = vnNow.Date;
            var normalized = Normalize(rawMessage);

            var res = new AiDateResolution
            {
                TargetStartDate = today,
                TargetEndDate = today,
                TargetThu = ConvertDayOfWeekToThu(today.DayOfWeek),
                Scope = "SingleDay",
                Label = $"Hôm nay ({today:dd/MM/yyyy})"
            };

            // Detect Query Focus Type
            if (normalized.Contains("lich thi") || normalized.Contains("thi cuoi ky") || normalized.Contains("thi giua ky") || normalized.Contains("co lich thi"))
            {
                res.QueryType = "ExamSchedule";
            }
            else if (normalized.Contains("su kien") || normalized.Contains("hoat dong"))
            {
                res.QueryType = "Event";
            }
            else if (normalized.Contains("lich hoc") || normalized.Contains("tiet hoc") || normalized.Contains("mon hoc"))
            {
                res.QueryType = "ClassSchedule";
            }
            else
            {
                res.QueryType = "All";
            }

            // 1. Hôm qua (Yesterday)
            if (normalized.Contains("hom qua") || normalized.Contains("ngay hom qua"))
            {
                var yesterday = today.AddDays(-1);
                res.TargetStartDate = yesterday;
                res.TargetEndDate = yesterday;
                res.TargetThu = ConvertDayOfWeekToThu(yesterday.DayOfWeek);
                res.Scope = "SingleDay";
                res.Label = $"Hôm qua ({yesterday:dd/MM/yyyy})";
                return res;
            }

            // 2. Ngày mai (Tomorrow)
            if (normalized.Contains("ngay mai") || normalized.Contains("hom sau") || normalized.Contains("ngay tiep theo"))
            {
                var tomorrow = today.AddDays(1);
                res.TargetStartDate = tomorrow;
                res.TargetEndDate = tomorrow;
                res.TargetThu = ConvertDayOfWeekToThu(tomorrow.DayOfWeek);
                res.Scope = "SingleDay";
                res.Label = $"Ngày mai ({tomorrow:dd/MM/yyyy})";
                return res;
            }

            // 3. Tuần này (This week)
            if (normalized.Contains("tuan nay") || normalized.Contains("ca tuan") || normalized.Contains("trong tuan"))
            {
                int diffFromMonday = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
                if (diffFromMonday < 0) diffFromMonday += 7; // If Sunday (0) -> diffFromMonday = 6
                var monday = today.AddDays(-diffFromMonday);
                var sunday = monday.AddDays(6);

                res.TargetStartDate = monday;
                res.TargetEndDate = sunday;
                res.Scope = "Week";
                res.Label = $"Tuần này ({monday:dd/MM} - {sunday:dd/MM/yyyy})";
                return res;
            }

            // 4. Ngày cụ thể theo định dạng: 20/08, 20/08/2026, 20-08
            var dateRegex = Regex.Match(normalized, @"(?:ngay\s+)?(\d{1,2})[/.-](\d{1,2})(?:[/.-](\d{4}))?");
            if (dateRegex.Success)
            {
                if (int.TryParse(dateRegex.Groups[1].Value, out int day) &&
                    int.TryParse(dateRegex.Groups[2].Value, out int month))
                {
                    int year = today.Year;
                    if (dateRegex.Groups[3].Success && int.TryParse(dateRegex.Groups[3].Value, out int explicitYear))
                    {
                        year = explicitYear;
                    }

                    try
                    {
                        var specificDate = new DateTime(year, month, day);
                        res.TargetStartDate = specificDate;
                        res.TargetEndDate = specificDate;
                        res.TargetThu = ConvertDayOfWeekToThu(specificDate.DayOfWeek);
                        res.Scope = "SingleDay";
                        res.Label = $"Ngày {specificDate:dd/MM/yyyy}";
                        return res;
                    }
                    catch
                    {
                        // Invalid date, fallback to next checks
                    }
                }
            }

            // 5. Thứ cụ thể trong tuần: "thứ 2", "thứ hai", "thứ 6", "thứ sáu", "chủ nhật"
            byte? requestedThu = null;
            string thuName = string.Empty;

            if (normalized.Contains("thu 2") || normalized.Contains("thu hai")) { requestedThu = 2; thuName = "Thứ Hai"; }
            else if (normalized.Contains("thu 3") || normalized.Contains("thu ba")) { requestedThu = 3; thuName = "Thứ Ba"; }
            else if (normalized.Contains("thu 4") || normalized.Contains("thu tu")) { requestedThu = 4; thuName = "Thứ Tư"; }
            else if (normalized.Contains("thu 5") || normalized.Contains("thu nam")) { requestedThu = 5; thuName = "Thứ Năm"; }
            else if (normalized.Contains("thu 6") || normalized.Contains("thu sau")) { requestedThu = 6; thuName = "Thứ Sáu"; }
            else if (normalized.Contains("thu 7") || normalized.Contains("thu bay")) { requestedThu = 7; thuName = "Thứ Bảy"; }
            else if (normalized.Contains("chu nhat") || normalized.Contains("cn")) { requestedThu = 8; thuName = "Chủ Nhật"; }

            if (requestedThu.HasValue)
            {
                // Calculate Monday of current week
                int diffFromMonday = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
                if (diffFromMonday < 0) diffFromMonday += 7;
                var monday = today.AddDays(-diffFromMonday);

                // requestedThu: 2 -> monday + 0 days, 3 -> monday + 1 days, ..., 8 -> monday + 6 days
                var targetThuDate = monday.AddDays(requestedThu.Value - 2);

                res.TargetStartDate = targetThuDate;
                res.TargetEndDate = targetThuDate;
                res.TargetThu = requestedThu.Value;
                res.Scope = "SingleDay";
                res.Label = $"{thuName} ({targetThuDate:dd/MM/yyyy})";
                return res;
            }

            // Default: Hôm nay
            return res;
        }

        private static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var text = input.ToLowerInvariant().Trim();
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'd');
        }

        private static byte ConvertDayOfWeekToThu(DayOfWeek dow)
        {
            return dow switch
            {
                DayOfWeek.Monday => 2,
                DayOfWeek.Tuesday => 3,
                DayOfWeek.Wednesday => 4,
                DayOfWeek.Thursday => 5,
                DayOfWeek.Friday => 6,
                DayOfWeek.Saturday => 7,
                DayOfWeek.Sunday => 8,
                _ => 2
            };
        }
    }
}
