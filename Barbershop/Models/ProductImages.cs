using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Barbershop.Models
{
    public class ProductImages
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Image { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual ICollection<Products> Products { get; set; }
    }
}
