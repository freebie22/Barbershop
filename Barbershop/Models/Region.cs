using System.ComponentModel.DataAnnotations;

namespace Barbershop.Models
{
    public class Region
    {
        [Key]
        public int RegionId { get; set; }
        [Required]
        public string RegionName { get; set; }
        
        public virtual ICollection<City> Cities { get; set; }
    }
}
