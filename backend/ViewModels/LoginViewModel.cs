#nullable disable
using System.ComponentModel.DataAnnotations;

namespace backend.ViewModels{
    public class LoginViewModel{
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email {get;set;}
        [Required]
        [DataType(DataType.Password)]
        public string Password {get;set;}
    }
}