#nullable disable
using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.ViewModels
{

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }
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
    }
}