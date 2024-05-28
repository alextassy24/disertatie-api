#nullable disable
namespace backend.Settings
{
    public class MqttConfiguration
    {
        public string Client { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
