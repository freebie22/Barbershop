using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Barbershop.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }
        [Required]
        public string LeftByUserId { get; set; }
        [ForeignKey("LeftByUserId")]
        public virtual BarbershopUser User { get; set; }
        [Required]
        public int BarberId { get; set; }
        [ForeignKey("BarberId")]
        public virtual Barbers Barber { get; set; }
        [Required, Range(1,5)]
        public int Rate { get; set; }
        public string? Review_Comment { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }

    }
}
