#nullable disable
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class LocationData
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }

        [JsonPropertyName("guid")]
        public string Guid { get; set; }
    }
}
