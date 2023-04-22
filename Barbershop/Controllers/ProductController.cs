using Barbershop.Data;
using Barbershop.Models;
using Barbershop.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Barbershop.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            this.db = db;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<Products> objList = db.Products.Include(p => p.ProductCategory).Include(p => p.ProductImages);
            return View(objList);
        }

        public IActionResult Details()
        {
            return View();
        }

        public IActionResult Create()
        {
            ProductsVM productsVM = new ProductsVM()
            {
                Product = new Products(),
                ProductCategorySelectList = db.ProductCategory.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }
                ),
                Images = db.ProductImages.ToList()
            };
            return View(productsVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductsVM productsVM)
        {
            var files = HttpContext.Request.Form.Files;
            string webRootPath = webHostEnvironment.WebRootPath;

            
            string upload = webRootPath + WC.ProductPath;
            string fileName = Guid.NewGuid().ToString();
            string extension = Path.GetExtension(files[0].FileName);

            using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
            {
                files[0].CopyTo(fileStream);
            }

            productsVM.Product.ProductImage = fileName + extension;

            productsVM.Product.ProductImages = new List<ProductImages>();

           for(int i = 1; i < files.Count; i++)
           {
                var imgUpload = webRootPath + WC.GalleryPath;
                var imgName = Guid.NewGuid().ToString();
                var imgExtension = Path.GetExtension(files[i].FileName);

                using (var fileStream = new FileStream(Path.Combine(imgUpload, imgName + imgExtension), FileMode.Create))
                 {
                    files[i].CopyTo(fileStream);
                 }
                 var productImage = new ProductImages()
                 {
                     Image = imgName
                 };
                 productsVM.Product.ProductImages.Add(productImage);
            }                     
            db.Products.Add(productsVM.Product);

            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
