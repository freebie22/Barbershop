using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Barbershop.Models
{
    public class Specializations
    {
        [Key]
        public int Id { get; set; }
        [Required, DisplayName("Назва спеціалазації")]
        public string SpecName { get; set; }
        [Required, DisplayName("Зображення спеціалазації")]
        public string SpecImage { get; set; }
    }
}
