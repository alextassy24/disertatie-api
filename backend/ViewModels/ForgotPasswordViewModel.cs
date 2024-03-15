#nullable disable
using System.ComponentModel.DataAnnotations;

namespace backend.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}