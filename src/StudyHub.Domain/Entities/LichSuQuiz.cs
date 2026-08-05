using System;

namespace StudyHub.Domain.Entities
{
    public class LichSuQuiz
    {
        public int MaQuiz { get; set; }
        
        public int MaNguoiDung { get; set; }
        public virtual NguoiDung NguoiDung { get; set; }
        
        public string ChuDe { get; set; }
        public int SoCauHoi { get; set; }
        public decimal? DiemSo { get; set; }
        public string NoiDung { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}
