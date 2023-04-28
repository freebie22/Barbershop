using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Barbershop.Models
{
    public class PostOffice
    {
        [Key]
        public int PostOfficeId { get; set; }
        [Required]
        public string OfficeName { get; set; }
        [Required]
        public int CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual City City { get; set; }
    }
}
