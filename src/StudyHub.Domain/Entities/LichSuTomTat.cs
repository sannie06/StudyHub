using System;

namespace StudyHub.Domain.Entities
{
    public class LichSuTomTat
    {
        public int MaTomTat { get; set; }
        
        public int MaNguoiDung { get; set; }
        public virtual NguoiDung NguoiDung { get; set; }
        
        public int MaFile { get; set; }
        public virtual FileTaiLen FileTaiLen { get; set; }
        
        public string NoiDungTomTat { get; set; }
        public DateTime NgayTomTat { get; set; } = DateTime.Now;
    }
}
