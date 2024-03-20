#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SerialNumber { get; set; }

        [Required]
        public Guid DeviceID { get; set; }

        [Required]
        public Wearer Wearer { get; set; }

        [Required]
        public int WearerID { get; set; }

        [JsonIgnore]
        [Required]
        public User User { get; set; }

        [Required]
        public string UserID { get; set; }

        public List<Location> Locations { get; set; }
    }
}
