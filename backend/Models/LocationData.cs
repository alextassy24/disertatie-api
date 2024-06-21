#nullable disable
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class LocationData
    {
        [JsonPropertyName("lat")]
        public double lat { get; set; }

        [JsonPropertyName("lon")]
        public double lon { get; set; }

        [JsonPropertyName("guid")]
        public string guid { get; set; }
    }
}
