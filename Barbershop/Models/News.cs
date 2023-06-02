using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Barbershop.Models
{
    public class News
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual BarbershopUser User { get; set; }
        [Required]
        public string MainPhoto { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string ShortDescription { get; set; }
        [Required]
        public string Category { get; set;}
        [Display(Name = "Галерея")]
        public virtual ICollection<NewsImages?> NewsImages { get; set; }
        public string? VideoUrl { get; set; }

    }
}
