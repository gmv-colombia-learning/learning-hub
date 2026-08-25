namespace VirtualBuddy.Infraestructure.Util
{
    public class ResendSettings
    {
        public const string SectionName = "Resend";
        public string ApiKey { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = "VirtualBuddy";
    }
}
