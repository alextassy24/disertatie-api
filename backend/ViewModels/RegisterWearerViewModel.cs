#nullable disable
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace backend.ViewModels
{
    public class RegisterWearerViewModel
    {
        [OnlyLetters]
        public string WearerName { get; set; }
        [Required]
        public int WearerAge { get; set; }
    }
}