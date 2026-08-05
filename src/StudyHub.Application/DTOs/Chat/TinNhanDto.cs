using System;

namespace StudyHub.Application.DTOs.Chat
{
    public class TinNhanDto
    {
        public int MaTinNhan { get; set; }
        public int MaNhom { get; set; }
        public int MaNguoiGui { get; set; }
        public string TenNguoiGui { get; set; } = string.Empty;
        public string? AvatarNguoiGui { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public byte LoaiTinNhan { get; set; } // 0: Text, 1: Image, 2: File, 3: System
        public bool DaChinhSua { get; set; }
        public DateTime NgayGui { get; set; }
        public bool IsMine { get; set; }
    }
}
