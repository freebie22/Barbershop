using Barbershop.Data;
using Barbershop.Models;
using Barbershop.Models.ViewModels;
using Barbershop.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            ProductsDetailsVM productsDetails = new ProductsDetailsVM()
            {
                Product = _db.Products.Include(p => p.ProductCategory).Include(p => p.ProductImages).Where(p => p.Id == id).FirstOrDefault(),
            };

            foreach (var item in shoppingCartList)
            {
                if (item.ProductId == id)
                {
                    productsDetails.ExistsInCart = true;
                }
            }



            return View(productsDetails);
        }

        [HttpPost, ActionName("Details")]
        public IActionResult DetailsPost(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            var product = _db.Products.FirstOrDefault(p => p.Id == id);

            shoppingCartList.Add(new ShoppingCart { ProductId = id });

            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveFromCart(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            var itemToRemove = shoppingCartList.SingleOrDefault(r => r.ProductId == id);

            if (itemToRemove != null)
            {
                shoppingCartList.Remove(itemToRemove);
            }


            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
