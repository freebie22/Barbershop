namespace Barbershop.Models.ViewModels
{
    public class ProductsDetailsVM
    {
        public ProductsDetailsVM()
        {
            Product = new Products();
        }

        public Products Product { get; set; }

        public bool ExistsInCart { get; set; }
    }
}
