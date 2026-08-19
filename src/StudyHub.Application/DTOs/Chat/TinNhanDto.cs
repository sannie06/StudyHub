using System;

namespace StudyHub.Application.DTOs.Chat
{
    public class TepDinhKemChatDto
    {
        public int MaFile { get; set; }
        public string TenFile { get; set; } = string.Empty;
        public string DuongDan { get; set; } = string.Empty;
        public long DungLuong { get; set; }
        public string DinhDang { get; set; } = string.Empty;
    }

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
        public TepDinhKemChatDto? Attachment { get; set; }
    }
}
