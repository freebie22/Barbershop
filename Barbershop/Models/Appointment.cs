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
        public string UserId { get; set; }
        [Required]
        public int BarberId { get; set; } 
        [ForeignKey("BarberId")]
        public virtual Barbers Barber { get; set; }
        [Required, Column(TypeName = "date")]
        public DateTime Date { get; set; }
        [Required, Range(typeof(TimeSpan), "07:00", "23:59", ErrorMessage = "Оберіть будь-ласка час для запису")]
        public TimeSpan StartTime { get; set; }
        [Required, Range(typeof(TimeSpan), "07:00", "23:59", ErrorMessage = "Оберіть будь-ласка час для запису")]
        public TimeSpan EndTime { get; set; }
        [Required, Range(1, 10000, ErrorMessage = "Оберіть хоча б одну послугу")]
        public decimal TotalPrice { get; set; }
        public decimal TotalPriceWithDiscount { get; set; }
        public string? UsedPromo { get; set; }
        [Required(ErrorMessage = "Вкажіть свій номер телефону")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Вкажіть свою електронну пошту")]
        public string Email { get; set; }
        [Required]
        public string AppointmentType { get; set; }
        [Required]
        public string AppointmentStatus { get; set;}
        public string? Comment { get; set; }
        public DateTime AppointmentDateAndTime { get; set; }


    }
}
