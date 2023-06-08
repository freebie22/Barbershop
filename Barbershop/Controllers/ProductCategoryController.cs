using Barbershop.Data;
using Barbershop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Barbershop.Controllers
{
    [Authorize]
    public class ProductCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProductCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(string? searchName)
        {
            if(User.IsInRole(WC.AdminRole))
            {
                IEnumerable<ProductCategory> objList = _db.ProductCategory;
                if(!string.IsNullOrEmpty(searchName))
                {
                    objList = _db.ProductCategory.Where(o => o.Name.ToLower().Contains(searchName));
                }
                return View(objList);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        
        public IActionResult Create() 
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCategory obj)
        {
            if(ModelState.IsValid)
            {
                await Task.Run(() =>
                {
                    _db.ProductCategory.AddAsync(obj);
                    _db.SaveChangesAsync();
                });
                return RedirectToAction("Index");   
            }
            return View(obj);
        }

        public IActionResult Edit(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }

            var obj = _db.ProductCategory.Find(id);

            if(obj == null) {
                return NotFound();
            }

            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductCategory obj)
        {
            if(ModelState.IsValid)
            {
                await Task.Run(() =>
                {
                    _db.ProductCategory.Update(obj);
                    _db.SaveChangesAsync();
                });
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Delete(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }

            var obj = _db.ProductCategory.Find(id);

            if(obj == null)
            {
                return NotFound();
            }

            return View(obj);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int? id)
        {
            var obj = _db.ProductCategory.Find(id);

            if(obj == null)
            {
                return NotFound();
            }
            if(ModelState.IsValid)
            {
                await Task.Run(() =>
                {
                    _db.ProductCategory.Remove(obj);
                    _db.SaveChangesAsync();
                });
                return RedirectToAction("Index");
            }
            return View(obj);
        }


    }
}
