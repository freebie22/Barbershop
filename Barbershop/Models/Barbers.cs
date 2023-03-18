using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Barbershop.Models
{
    public class Barbers
    {
        [Key]
        public int Id { get; set; }
        [Required, DisplayName("Ім'я та прізвище")]
        public string FullName { get; set; }
        [Required, DisplayName("Номер телефону")]
        public string PhoneNumber { get; set; }
        [Required, DisplayName("Про барбера")]
        public string Description { get; set; }
        [Required, DisplayName("Стаж роботи")]
        public int Exprerience { get; set; }
        [Required]
        public string BarberImage { get; set; }
        [Required, Display(Name = "Посада")]
        public int WorkPositionId { get; set; }
        [ForeignKey("WorkPositionId")]
        public virtual WorkPositions WorkPosition { get; set; }
        [Required, DisplayName("Спеціалізація")]
        public ICollection<Specializations> Specializations { get; set; }
    }
}
