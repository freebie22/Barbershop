using System.ComponentModel.DataAnnotations;

namespace Barbershop.Models
{
    public class NewsImages
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Image { get; set; }
        [Required]
        public virtual ICollection<News> News { get; set; }
    }
}
