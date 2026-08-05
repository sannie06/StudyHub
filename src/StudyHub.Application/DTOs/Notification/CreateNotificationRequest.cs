namespace StudyHub.Application.DTOs.Notification
{
    public class CreateNotificationRequest
    {
        public int MaNguoiDung { get; set; }
        public int MaLoaiThongBao { get; set; } = 1;
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string? DuongDan { get; set; }
        public byte MucDo { get; set; } = 1;
    }
}
