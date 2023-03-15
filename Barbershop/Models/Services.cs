using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Barbershop.Models
{
    public class Services
    {
        [Key]
        public int Id { get; set; }
        [Required,DisplayName("Послуга")]
        public string serviceName { get; set; }
        [Required,Range(1, 10000),DisplayName("Стажер")]
        public double traineePrice { get; set; }
        [Required, Range(1, 10000),DisplayName("Барбер")]
        public double barberPrice { get; set; }
        [Required, Range(1, 10000),DisplayName("Топ-барбер")]
        public double seniorPrice { get; set; }
    }
}
