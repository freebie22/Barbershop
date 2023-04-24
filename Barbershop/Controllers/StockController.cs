using Barbershop.Data;
using Barbershop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Barbershop.Controllers
{
    public class StockController : Controller
    {
        private readonly ApplicationDbContext _db;

        public StockController(ApplicationDbContext db)
        {
            _db= db;

        }
        
        public IActionResult Index()
        {
            StockVM stock = new StockVM() 
            {
                Products = _db.Products.Include(p => p.ProductCategory),
                Categories = _db.ProductCategory
            };
            return View(stock); 
        }

        public IActionResult Details(int id)
        {
            ProductsDetailsVM productsDetails = new ProductsDetailsVM()
            {
                Product = _db.Products.Include(p => p.ProductCategory).Include(p => p.ProductImages).Where(p => p.Id == id).FirstOrDefault(),
            };
            return View(productsDetails);
        }
    }
}
