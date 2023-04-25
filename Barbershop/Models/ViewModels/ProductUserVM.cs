namespace Barbershop.Models.ViewModels
{
    public class ProductUserVM
    {
        public ProductUserVM()
        {
            ProductList = new List<Products>();
        }
        public BarbershopUser BarbershopUser { get; set; }
        public IList<Products> ProductList { get; set; }
    }
}
