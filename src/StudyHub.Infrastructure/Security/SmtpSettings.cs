namespace StudyHub.Infrastructure.Security
{
    public class SmtpSettings
    {
        public const string SectionName = "SmtpSettings";

        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
