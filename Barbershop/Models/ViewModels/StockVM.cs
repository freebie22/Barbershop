namespace Barbershop.Models.ViewModels
{
    public class StockVM
    {
        public IEnumerable<Products> Products { get; set; } 
        public IEnumerable<ProductCategory> Categories { get; set; }
    }
}
