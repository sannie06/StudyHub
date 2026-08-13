using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StudyHub.Infrastructure.Services.Ai
{
    public class AiPromptBuilder
    {
        public string BuildPrompt(AiContext context, string userQuery)
        {
            var sb = new StringBuilder();

            // ══════════════════════════════════════════════════════════════════════════
            // 1. SYSTEM ROLE & PERSONA
            // ══════════════════════════════════════════════════════════════════════════
            sb.AppendLine("BẠN LÀ STUDYHUB AI — TRỢ LÝ HỌC TẬP VÀ ĐỜI SỐNG CÁ NHÂN CỦA SINH VIÊN TRÊN HỆ THỐNG STUDYHUB.");
            sb.AppendLine("Phong cách: Tận tâm, thông minh, chuẩn mực học thuật, trả lời bằng tiếng Việt tự nhiên và súc tích.");
            sb.AppendLine();

            // ══════════════════════════════════════════════════════════════════════════
            // 2. CORE RESPONSE RULES (BẮT BUỘC TUÂN THỦ)
            // ══════════════════════════════════════════════════════════════════════════
            sb.AppendLine("QUY TẮC CỐT LÕI (CORE RULES):");
            sb.AppendLine("1. DIRECT ANSWER FIRST: Luôn luôn trả lời trực tiếp, chính xác và đầy đủ vào đúng câu hỏi của người dùng trước tiên.");
            sb.AppendLine("2. NO TOPIC DRIFT: Tuyệt đối không tự ý biến một câu hỏi cụ thể thành một chủ đề rộng hơn (Ví dụ: hỏi 'Java interface là gì?' thì PHẢI giải thích đúng Interface, cú pháp và code mẫu ví dụ; KHÔNG ĐƯỢC biến thành lộ trình học Java/Spring/JDBC).");
            sb.AppendLine("3. ACCURACY & NO FABRICATION: Không tự bịa thông tin lịch thi, đề cương hay dữ liệu người dùng. Nếu thiếu thông tin cần thiết, hãy hỏi lại người dùng.");
            sb.AppendLine("4. GROUNDED CONTEXT: Chỉ tham chiếu dữ liệu cá nhân của người dùng khi câu hỏi thực sự liên quan.");
            sb.AppendLine("5. STRUCTURED FORMAT: Định dạng Markdown sạch đẹp, có bullet points, in đậm từ khóa, code block có cú pháp ngôn ngữ.");
            sb.AppendLine();

            // ══════════════════════════════════════════════════════════════════════════
            // 3. INTENT & SELECTIVE CONTEXT
            // ══════════════════════════════════════════════════════════════════════════
            sb.AppendLine($"MỤC TIÊU PHÂN LOẠI (INTENT): {context.Intent}");

            if (context.UserProfile != null)
            {
                sb.AppendLine($"Sinh viên: {context.UserProfile.HoTen}");
            }

            switch (context.Intent)
            {
                case AiIntents.KnowledgeQa:
                    sb.AppendLine("NGỮ CẢNH: Câu hỏi học thuật/kỹ thuật thuần túy.");
                    sb.AppendLine("HƯỚNG DẪN TRẢ LỜI:");
                    sb.AppendLine("• Nếu hỏi định nghĩa: Nêu rõ khái niệm, mục đích sử dụng, cú pháp và 1 ví dụ code ngắn kèm giải thích.");
                    sb.AppendLine("• Nếu hỏi so sánh (vd Interface vs Abstract Class, INNER JOIN vs LEFT JOIN): Lập bảng hoặc gạch đầu dòng so sánh các tiêu chí chính.");
                    sb.AppendLine("• Nếu hỏi code: Viết code mẫu chuẩn Clean Code và giải thích ngắn gọn.");
                    break;

                case AiIntents.ScheduleQuery:
                    var dateLabel = !string.IsNullOrEmpty(context.TargetDateLabel) ? context.TargetDateLabel : "ngày được hỏi";
                    sb.AppendLine($"NGỮ CẢNH: Tra cứu lịch học, lịch thi và sự kiện cho: {dateLabel}.");
                    sb.AppendLine("HƯỚNG DẪN BẮT BUỘC DÀNH CHO TRỢ LÝ:");
                    sb.AppendLine("• Nếu người dùng hỏi về LỊCH HỌC: Chỉ sử dụng phần 'DỮ LIỆU LỊCH HỌC'. Tuyệt đối không lấy Lịch thi hay Sự kiện để trả lời. Nếu không có lịch học thì trả lời rõ ràng không có lịch học.");
                    sb.AppendLine("• Nếu người dùng hỏi về LỊCH THI: Chỉ sử dụng phần 'DỮ LIỆU LỊCH THI'. Tuyệt đối không lấy Lịch học để trả lời. Nếu không có lịch thi thì trả lời rõ ràng không có lịch thi.");
                    sb.AppendLine("• Nếu người dùng hỏi về SỰ KIỆN: Chỉ sử dụng phần 'DỮ LIỆU SỰ KIỆN'.");
                    sb.AppendLine("• Chỉ khi người dùng hỏi chung chung ('có lịch gì', 'có gì'): Mới tổng hợp cả Lịch học, Lịch thi và Sự kiện.");
                    sb.AppendLine();

                    if (context.ScheduleClasses != null && context.ScheduleClasses.Any())
                    {
                        var classItems = context.ScheduleClasses.Select(c =>
                        {
                            string thuStr = c.Thu switch { 2 => "Thứ 2", 3 => "Thứ 3", 4 => "Thứ 4", 5 => "Thứ 5", 6 => "Thứ 6", 7 => "Thứ 7", 8 => "Chủ Nhật", _ => "" };
                            var thuDisplay = context.DateScope == "Week" ? $" ({thuStr})" : "";
                            return $"{c.TenMonHoc}{thuDisplay} (Tiết {c.TietBatDau}-{c.TietKetThuc}, Phòng: {c.PhongHoc}, GV: {c.GiangVien})";
                        });
                        sb.AppendLine($"DỮ LIỆU LỊCH HỌC ({dateLabel}): " + string.Join("; ", classItems));
                    }
                    else
                    {
                        sb.AppendLine($"DỮ LIỆU LỊCH HỌC ({dateLabel}): Không có lịch học cố định nào.");
                    }

                    if (context.ScheduleExams != null && context.ScheduleExams.Any())
                    {
                        sb.AppendLine($"DỮ LIỆU LỊCH THI ({dateLabel}): " + string.Join("; ", context.ScheduleExams.Select(e => $"{e.TenMonHoc} ({e.HinhThucThi}, Phòng: {e.PhongThi})")));
                    }
                    else
                    {
                        sb.AppendLine($"DỮ LIỆU LỊCH THI ({dateLabel}): Không có lịch thi nào.");
                    }

                    if (context.ScheduleEvents != null && context.ScheduleEvents.Any())
                    {
                        sb.AppendLine($"DỮ LIỆU SỰ KIỆN ({dateLabel}): " + string.Join("; ", context.ScheduleEvents.Select(ev => $"{ev.TieuDe} tại {ev.DiaDiem}")));
                    }
                    break;

                case AiIntents.TaskQuery:
                    var vnNowQuery = DateTime.UtcNow.AddHours(7);
                    var todayQueryStr = $"{vnNowQuery:dd/MM/yyyy} ({FormatDayOfWeek(vnNowQuery.DayOfWeek)})";

                    sb.AppendLine("NGỮ CẢNH: Tra cứu thông tin danh sách công việc của người dùng.");
                    sb.AppendLine($"HÔM NAY LÀ: {todayQueryStr}.");
                    sb.AppendLine();
                    sb.AppendLine("HƯỚNG DẪN BẮT BUỘC DÀNH CHO TRỢ LÝ:");
                    sb.AppendLine("• Trả lời TRỰC TIẾP và NGẮN GỌN đúng nội dung danh sách người dùng hỏi:");
                    sb.AppendLine("  - Nếu hỏi 'Tôi có task nào quá hạn không?': Chỉ kiểm tra và liệt kê danh sách công việc quá hạn.");
                    sb.AppendLine("  - Nếu hỏi 'Hôm nay tôi có task nào cần làm không?': Tổng hợp cả danh sách Quá hạn và danh sách Deadline hôm nay, thông báo tổng số việc cần xử lý và liệt kê rõ ràng 2 nhóm (🚨 Quá hạn và 🔴 Deadline hôm nay).");
                    sb.AppendLine("• TUYỆT ĐỐI KHÔNG tự động biến câu hỏi tra cứu thành phân tích Ma trận Eisenhower dài dòng.");
                    sb.AppendLine("• Nếu không có công việc nào phù hợp, hãy thông báo rõ ràng rằng người dùng không có công việc nào cần xử lý.");
                    sb.AppendLine();

                    if (context.OverdueTasks != null && context.OverdueTasks.Any())
                    {
                        sb.AppendLine("DANH SÁCH CÔNG VIỆC QUÁ HẠN:");
                        foreach (var t in context.OverdueTasks)
                        {
                            sb.AppendLine($"• {t.TieuDe} (Môn: {t.TenMonHoc ?? "Chung"}, Hạn chót: {t.HanHoanThanh:dd/MM/yyyy}, Mức ưu tiên: {FormatPriority(t.DoUuTien)})");
                        }
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendLine("DANH SÁCH CÔNG VIỆC QUÁ HẠN: Không có công việc nào quá hạn.");
                    }

                    if (context.RelevantTasks != null && context.RelevantTasks.Any())
                    {
                        sb.AppendLine("DANH SÁCH CÔNG VIỆC HÔM NAY:");
                        foreach (var t in context.RelevantTasks)
                        {
                            sb.AppendLine($"• {t.TieuDe} (Môn: {t.TenMonHoc ?? "Chung"}, Hạn chót: {t.HanHoanThanh:dd/MM/yyyy}, Mức ưu tiên: {FormatPriority(t.DoUuTien)})");
                        }
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendLine("DANH SÁCH CÔNG VIỆC HÔM NAY: Không có công việc nào có hạn chót hôm nay.");
                    }
                    break;

                case AiIntents.TaskPrioritization:
                    var vnNowPrior = DateTime.UtcNow.AddHours(7);
                    var todayPriorStr = $"{vnNowPrior:dd/MM/yyyy} ({FormatDayOfWeek(vnNowPrior.DayOfWeek)})";

                    sb.AppendLine("NGỮ CẢNH: Phân tích và đề xuất thứ tự ưu tiên công việc.");
                    sb.AppendLine($"HÔM NAY LÀ: {todayPriorStr}.");
                    sb.AppendLine();
                    sb.AppendLine("QUY TẮC ƯU TIÊN BẮT BUỘC DÀNH CHO TRỢ LÝ:");
                    sb.AppendLine("1. Task quá hạn (nếu có) và Task có deadline HÔM NAY luôn luôn phải được ưu tiên giải quyết hàng đầu.");
                    sb.AppendLine("2. Phải chỉ định rõ ràng Task nào cần làm trước trong ngày hôm nay.");
                    sb.AppendLine("3. Yếu tố khoảng cách Deadline (độ khẩn cấp thời gian) quan trọng hơn DoUuTien ban đầu: Tuyệt đối không được chọn task có deadline ở nhiều ngày tới chỉ vì DoUuTien cao hơn task có deadline hôm nay!");
                    sb.AppendLine("4. DoUuTien chỉ dùng để xếp thứ tự giữa các task có cùng thời hạn deadline (ví dụ cùng deadline hôm nay thì ưu tiên việc có DoUuTien cao hơn).");
                    sb.AppendLine("5. Nếu người dùng chỉ hỏi câu hỏi ngắn, trực diện (ví dụ: 'Task nào cần ưu tiên trước?', 'Task nào ưu tiên trước?'): Hãy trả lời tập trung vào đúng Task đầu tiên cần làm và giải thích lý do ngắn gọn, không tự động in toàn bộ danh sách task tương lai xa.");
                    sb.AppendLine("6. Không được tự bịa đặt task hoặc deadline không có trong dữ liệu dưới đây.");
                    sb.AppendLine();

                    if (context.OverdueTasks != null && context.OverdueTasks.Any())
                    {
                        sb.AppendLine("🚨 CÔNG VIỆC QUÁ HẠN (CẦN LÀM NGAY):");
                        foreach (var t in context.OverdueTasks)
                        {
                            sb.AppendLine($"• {t.TieuDe} (Môn: {t.TenMonHoc ?? "Chung"}, Hạn chót: {t.HanHoanThanh:dd/MM/yyyy}, Mức ưu tiên: {FormatPriority(t.DoUuTien)})");
                        }
                        sb.AppendLine();
                    }

                    if (context.RelevantTasks != null && context.RelevantTasks.Any())
                    {
                        sb.AppendLine("🔴 CÔNG VIỆC CÓ DEADLINE HÔM NAY (ƯU TIÊN BẮT BUỘC LÀM HÔM NAY):");
                        foreach (var t in context.RelevantTasks)
                        {
                            sb.AppendLine($"• {t.TieuDe} (Môn: {t.TenMonHoc ?? "Chung"}, Hạn chót: Hôm nay {t.HanHoanThanh:dd/MM/yyyy}, Mức ưu tiên: {FormatPriority(t.DoUuTien)})");
                        }
                        sb.AppendLine();
                    }

                    if (context.UpcomingDeadlines != null && context.UpcomingDeadlines.Any())
                    {
                        sb.AppendLine("🟡 CÔNG VIỆC CÓ DEADLINE TRONG NHỮNG NGÀY TỚI:");
                        foreach (var t in context.UpcomingDeadlines)
                        {
                            sb.AppendLine($"• {t.TieuDe} (Môn: {t.TenMonHoc ?? "Chung"}, Hạn chót: {t.HanHoanThanh:dd/MM/yyyy}, Mức ưu tiên: {FormatPriority(t.DoUuTien)})");
                        }
                    }
                    break;

                case AiIntents.TaskHelp:
                    sb.AppendLine($"NGỮ CẢNH: Hướng dẫn thực hiện công việc \"{context.FocusedTaskTitle}\".");
                    if (context.FocusedTask != null)
                    {
                        sb.AppendLine($"Thông tin: Môn {context.FocusedTask.TenMonHoc ?? "Chung"}, Hạn: {context.FocusedTask.HanHoanThanh:dd/MM/yyyy}.");
                    }
                    sb.AppendLine("HƯỚNG DẪN TRẢ LỜI: Đưa ra 4 bước hành động cụ thể, chia nhỏ thời gian Pomodoro và mục tiêu rõ ràng.");
                    break;

                case AiIntents.WorkloadAnalysis:
                    sb.AppendLine("NGỮ CẢNH: Đánh giá khối lượng công việc, mức độ quá tải và thống kê nhiệm vụ.");
                    var overdueCount = context.OverdueTasks?.Count ?? 0;
                    var todayCount = context.RelevantTasks?.Count ?? 0;
                    var pendingCount = context.Statistics?.CongViecChuaHoanThanh ?? 0;
                    var totalCount = context.Statistics?.TongSoCongViec ?? 0;

                    sb.AppendLine($"SỐ LIỆU THỰC TẾ: Tổng số việc: {totalCount}, Chưa hoàn thành: {pendingCount} (trong đó Quá hạn: {overdueCount}, Deadline hôm nay: {todayCount}).");
                    if (context.RelevantExamSchedules != null && context.RelevantExamSchedules.Any())
                    {
                        sb.AppendLine($"Lịch thi sắp tới: {context.RelevantExamSchedules.Count} môn.");
                    }

                    sb.AppendLine();
                    sb.AppendLine("HƯỚNG DẪN BẮT BUỘC DÀNH CHO TRỢ LÝ:");
                    sb.AppendLine("1. ĐẶC BIỆT CHÚ Ý: Nếu có công việc quá hạn (Overdue > 0), KHÔNG ĐƯỢC kết luận là 'cân bằng' hay 'tiến hành ổn định'. Phải cảnh báo rõ ràng người dùng có công việc quá hạn cần ưu tiên xử lý dứt điểm.");
                    sb.AppendLine("2. Nếu người dùng hỏi số lượng (ví dụ 'Tôi đang có bao nhiêu công việc cần hoàn thành?'): Hãy trả lời thẳng số lượng chưa hoàn thành trên tổng số, nêu rõ số việc quá hạn và deadline hôm nay, không tự động đưa ma trận Eisenhower dài dòng.");
                    sb.AppendLine("3. Nếu người dùng hỏi 'Tôi có quá nhiều task không?' hoặc 'Tôi có đang quá tải không?': Đưa ra nhận định dựa trên số task quá hạn và cận hạn, giải thích ngắn gọn và đưa ra lời khuyên thiết thực.");
                    break;

                case AiIntents.StudyRecommendation:
                    sb.AppendLine("NGỮ CẢNH: Đề xuất phương pháp học tập hiệu quả và tối ưu hóa thời gian.");
                    sb.AppendLine("HƯỚNG DẪN: Kết hợp nguyên lý Pomodoro, ma trận Eisenhower và kỹ thuật học tập chủ động.");
                    break;
            }

            // ══════════════════════════════════════════════════════════════════════════
            // 4. USER QUERY
            // ══════════════════════════════════════════════════════════════════════════
            sb.AppendLine();
            sb.AppendLine($"CÂU HỎI CỦA NGƯỜI DÙNG: \"{userQuery}\"");
            sb.AppendLine("HÃY TRẢ LỜI NGAY DƯỚI ĐÂY:");

            return sb.ToString();
        }

        private static string FormatPriority(byte priority)
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

        private static string FormatDayOfWeek(DayOfWeek dow)
        {
            return dow switch
            {
                DayOfWeek.Monday => "Thứ Hai",
                DayOfWeek.Tuesday => "Thứ Ba",
                DayOfWeek.Wednesday => "Thứ Tư",
                DayOfWeek.Thursday => "Thứ Năm",
                DayOfWeek.Friday => "Thứ Sáu",
                DayOfWeek.Saturday => "Thứ Bảy",
                DayOfWeek.Sunday => "Chủ Nhật",
                _ => "Thứ Hai"
            };
        }
    }
}
