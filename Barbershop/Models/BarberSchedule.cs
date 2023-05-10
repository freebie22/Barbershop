using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;

namespace Barbershop.Models
{
    public class BarberSchedule
    {
        [Key]
        public int Id { get; set; }
        [Column(TypeName = "date")]
        [Required, DataType(DataType.Date), DisplayName("Дата")]
        public DateTime Date { get; set; }
        [Required]
        public int BarberId { get; set; }
        [ForeignKey("BarberId")]
        public virtual Barbers Barber { get; set; }
        [Required, DisplayName("Початок робочого дня")]
        public TimeSpan StartTime { get; set; }
        [Required, DisplayName("Кінець робочого дня")]
        public TimeSpan EndTime { get; set; }
    }
}
