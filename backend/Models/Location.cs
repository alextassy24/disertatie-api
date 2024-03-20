#nullable disable

using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Location
    {
        [Key]
        public int ID { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public Product Product { get; set; }
        public int ProductID { get; set; }
    }
}
