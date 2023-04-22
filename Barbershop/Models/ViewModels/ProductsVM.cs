using Microsoft.AspNetCore.Mvc.Rendering;

namespace Barbershop.Models.ViewModels
{
    public class ProductsVM
    {
        public Products Product { get; set; }
        public IEnumerable<SelectListItem>? ProductCategorySelectList { get; set; }
        public List<ProductImages> Images { get; set; }
        public List<int> ImagesIds { get; set; }
    }
}
