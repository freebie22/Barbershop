using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Barbershop.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; } = "Гість";
        [ForeignKey("UserId")]
        public virtual BarbershopUser User { get; set; }
        [Required]
        public int BarberId { get; set; } 
        [ForeignKey("BarberId")]
        public virtual Barbers Barber { get; set; }
        [Required, Column(TypeName = "date")]
        public DateTime Date { get; set; }
        [Required]
        public TimeSpan StartTime { get; set; }
        [Required]
        public TimeSpan EndTime { get; set; }
        [Required, Range(1, 10000)]
        public decimal TotalPrice { get; set; }
        [Required]
        public virtual ICollection<Services> Services { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string AppointmentType { get; set; }
        [Required]
        public string AppointmentStatus { get; set;}
        public string? Comment { get; set; }

    }
}
