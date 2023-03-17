using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Barbershop.Models
{
    public class ProductCategory
    {
        [Key]
        public int Id { get; set; }
        [Required, DisplayName("Назва категорії")]
        public string Name { get; set; }
    }
}
