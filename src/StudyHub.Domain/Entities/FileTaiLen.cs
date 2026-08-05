using System;
using System.Collections.Generic;

namespace StudyHub.Domain.Entities
{
    public class FileTaiLen
    {
        public int MaFile { get; set; }
        public int MaNguoiDung { get; set; }
        public virtual NguoiDung NguoiDung { get; set; }

        public string TenGoc { get; set; }
        public string TenLuu { get; set; }
        public string DuongDan { get; set; }
        public string LoaiFile { get; set; }
        public long DungLuong { get; set; }
        public string Extension { get; set; }
        public DateTime NgayTaiLen { get; set; } = DateTime.Now;
        public bool DaXoa { get; set; } = false;

        // Navigation properties
        public virtual ICollection<TepDinhKemTinNhan> TepDinhKemTinNhan { get; set; } = new List<TepDinhKemTinNhan>();
        public virtual ICollection<TaiLieu> TaiLieu { get; set; } = new List<TaiLieu>();
        public virtual ICollection<LichSuTomTat> LichSuTomTat { get; set; } = new List<LichSuTomTat>();
    }
}
