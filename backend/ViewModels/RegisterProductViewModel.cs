#nullable disable
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace backend.ViewModels
{
    public class RegisterProductViewModel
    {
        [Required]
        public string DeviceSerialNumber { get; set; }
        [Required]
        public Guid DeviceGUID { get; set; }
        public int WearerID { get; set; }
    }
}