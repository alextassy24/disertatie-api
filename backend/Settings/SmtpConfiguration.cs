#nullable disable
namespace backend.Settings
{
    public class SmtpConfiguration
    {
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AppPassword { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string AppBaseUrl { get; set; }
    }
}