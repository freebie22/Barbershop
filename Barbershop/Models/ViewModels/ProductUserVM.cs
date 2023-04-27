namespace Barbershop.Models.ViewModels
{
    public class ProductUserVM
    {
        public ProductUserVM()
        {
            ProductList = new List<Products>();
        }
        public BarbershopUser BarbershopUser { get; set; }
        public List<Products> ProductList { get; set; }
        public decimal OrderTotal { get; set; }
    }
}
