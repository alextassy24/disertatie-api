#nullable disable
using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.ViewModels
{

    public class RecoverPasswordViewModel
    {
        [Required]
        [AtLeastOneLowerCase]
        [AtLeastOneUpperCase]
        [AtLeastOneNumber]
        [AtLeastOneSpecialCharacter]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Token { get; set; }
    }
}