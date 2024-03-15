#nullable disable
using System.ComponentModel.DataAnnotations;
using Org.BouncyCastle.Asn1.X509;

namespace backend.Models
{
    public class Token
    {
        [Key]
        public int Id { get; set; }
        public string UserID {get;set;}
        public User User {get;set;}
        public string TokenValue { get; set; }
        public bool Status { get; set; }
        public Usage Usage {get;set;}
        public DateTime CreatedAt { get; set; }
        public DateTime ExpirationDate { get; set; }
    }

    public enum Usage
    {
        EmailConfirmation, PasswordReset
    }
}