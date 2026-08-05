namespace StudyHub.Application.DTOs.Chat
{
    public class SendChatMessageRequest
    {
        public int MaNhom { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public byte LoaiTinNhan { get; set; } = 0; // 0: Text
    }
}
