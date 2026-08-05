namespace StudyHub.Application.DTOs.Chat
{
    public class TypingNotificationDto
    {
        public int MaNhom { get; set; }
        public int MaNguoiDung { get; set; }
        public string TenNguoiDung { get; set; } = string.Empty;
        public bool IsTyping { get; set; }
    }
}
