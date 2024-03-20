#nullable disable
using System.ComponentModel.DataAnnotations;

namespace backend.ViewModels
{
    public class RegisterLocationViewModel
    {
        [Required]
        public Guid DeviceGUID { get; set; }

        [Required]
        public string Latitude { get; set; }

        [Required]
        public string Longitude { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public TimeOnly Time { get; set; }
    }
}
