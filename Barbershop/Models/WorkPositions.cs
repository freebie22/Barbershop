using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Barbershop.Models
{
    public class WorkPositions
    {
        [Key]
        public int Id { get; set; }
        [Required,DisplayName("Назва посади")]
        public string PositionName { get; set; }
        [Required,DisplayName("Стандартний оклад")]
        public int StandardPay { get; set; }
    }
}
