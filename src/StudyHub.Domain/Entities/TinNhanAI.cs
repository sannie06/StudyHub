using System;

namespace StudyHub.Domain.Entities
{
    public class TinNhanAI
    {
        public int MaTinNhanAI { get; set; }
        
        public int MaHoiThoai { get; set; }
        public virtual HoiThoaiAI HoiThoaiAI { get; set; }

        public string VaiTro { get; set; } // user / assistant
        public string NoiDung { get; set; }
        public int? TokenSuDung { get; set; }
        public DateTime NgayGui { get; set; } = DateTime.Now;
    }
}
