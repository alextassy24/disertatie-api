#nullable disable
using System.ComponentModel.DataAnnotations;

namespace backend.ViewModels{

    public class InfoViewModel{
        [Required]
        public string Id{get;set;}
    }
}