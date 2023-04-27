using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Barbershop.Models
{
    public class Products
    {
        [Key]
        public int Id { get; set; }
        [Required, DisplayName("Назва товару")]
        public string ProductName { get; set; }
        [Required, Range(1, 15000), DisplayName("Ціна, грн")]
        public decimal Price { get; set; }
        [Required, DisplayName("Виробник")]
        public string Producer { get; set; }
        [Required, DisplayName("Об'єм")]
        public string Volume { get; set; }
        [Required, DisplayName("Склад")]
        public string Consist { get; set; }
        [Required, DisplayName("Опис")]
        public string Description { get; set; }
        [Required, DisplayName("Як застосовувати")]
        public string Usage { get; set; }
        [Required]
        public string ProductImage { get; set; }
        [Display(Name = "Категорія товару")]
        public int ProductCategoryId { get; set; }
        [ForeignKey("ProductCategoryId")]
        public virtual ProductCategory ProductCategory { get; set; }
        [Required, Display(Name = "Галерея")]
        public virtual ICollection<ProductImages> ProductImages { get; set; }

        [NotMapped, Range(1,20)]
        public int TempCount { get; set; }
    }
}
