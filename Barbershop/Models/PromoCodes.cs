using System.ComponentModel.DataAnnotations;

namespace Barbershop.Models
{
    public class PromoCodes
    {
        [Key] 
        public int Id { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public double Discount { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        public string UserId { get; set; }
    }
}
