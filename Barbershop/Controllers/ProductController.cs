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
            var files1 = HttpContext.Request.Form.Files.GetFile("files1");
            var files2 = HttpContext.Request.Form.Files.GetFiles("files2");
            string webRootPath = webHostEnvironment.WebRootPath;

            
            string upload = webRootPath + WC.ProductPath;
            string fileName = Guid.NewGuid().ToString();
            string extension = Path.GetExtension(files1.FileName);

            using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
            {
                files1.CopyTo(fileStream);
            }

            productsVM.Product.ProductImage = fileName + extension;

            productsVM.Product.ProductImages = new List<ProductImages>();

           for(int i = 0; i < files2.Count; i++)
           {
                var imgUpload = webRootPath + WC.GalleryPath;
                var imgName = Guid.NewGuid().ToString();
                var imgExtension = Path.GetExtension(files2[i].FileName);

                using (var fileStream = new FileStream(Path.Combine(imgUpload, imgName + imgExtension), FileMode.Create))
                 {
                    files2[i].CopyTo(fileStream);
                 }
                 var productImage = new ProductImages()
                 {
                     Image = imgName + imgExtension
                 };
                 productsVM.Product.ProductImages.Add(productImage);
            }                     
            db.Products.Add(productsVM.Product);

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //GET - Edit
        public IActionResult Edit(int? id)
        {
            ProductsVM productVM = new ProductsVM()
            {
                Product = new Products(),
                ProductCategorySelectList = db.ProductCategory.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }
                )
            };


            productVM.Product = db.Products.Include(p => p.ProductImages).FirstOrDefault(p => p.Id == id);

            productVM.Images = productVM.Product.ProductImages.ToList();


            if (productVM.Product == null)
            {
                return NotFound();
            }

            return View(productVM);
        }

        //POST - Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductsVM productVM, int id)
        {
            var files1 = HttpContext.Request.Form.Files.GetFiles("files1");
            var files2 = HttpContext.Request.Form.Files.GetFiles("files2");

            string webRootPath = webHostEnvironment.WebRootPath;

            var objFromDb = db.Products.AsNoTracking().FirstOrDefault(u => u.Id == productVM.Product.Id);

            if (files1.Count > 0)
            {
                string upload = webRootPath + WC.BarberPath;
                string fileName = Guid.NewGuid().ToString();
                string extension = Path.GetExtension(files1[0].FileName);

                var oldFile = Path.Combine(upload, objFromDb.ProductImage);

                if (System.IO.File.Exists(oldFile))
                {
                    System.IO.File.Delete(oldFile);
                }

                using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                {
                    files1[0].CopyTo(fileStream);
                }

                productVM.Product.ProductImage = fileName + extension;
            }

            else
            {
                productVM.Product.ProductImage = objFromDb.ProductImage;
            }

            if (files2.Count > 0)
            {
                productVM.Product.ProductImages = new List<ProductImages>();

                for (int i = 0; i < files2.Count; i++)
                {
                    var imgUpload = webRootPath + WC.GalleryPath;
                    var imgName = Guid.NewGuid().ToString();
                    var imgExtension = Path.GetExtension(files2[i].FileName);

                    using (var fileStream = new FileStream(Path.Combine(imgUpload, imgName + imgExtension), FileMode.Create))
                    {
                        files2[i].CopyTo(fileStream);
                    }
                    var productImage = new ProductImages()
                    {
                        Image = imgName + imgExtension
                    };
                    productVM.Product.ProductImages.Add(productImage);
                }
            }

            else
            {
                productVM.Product.ProductImages = objFromDb.ProductImages;
            }




            db.Products.Update(productVM.Product);

            db.SaveChanges();


            return RedirectToAction("Index");
        }

        public IActionResult Delete(int? id)
        {
            ProductsVM productVM = new ProductsVM()
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

            productVM.Product = db.Products.Find(id);
            if (productVM.Product == null)
            {
                return NotFound();
            }

            return View(productVM);
        }

        //POST - Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            var obj = db.Products.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            string upload = webHostEnvironment.WebRootPath + WC.ProductPath;


            var oldFile = Path.Combine(upload, obj.ProductImage);

            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }

            string uploads = webHostEnvironment.WebRootPath + WC.GalleryPath;

            var imagesToDelete = db.ProductImages.Where(pi => pi.Products.Any(p => p.Id == obj.Id)).ToList();

            foreach (var imageToDelete in imagesToDelete)
            {
                var imageFile = Path.Combine(uploads, imageToDelete.Image);
                if (System.IO.File.Exists(imageFile))
                {
                    System.IO.File.Delete(imageFile);
                }
                db.ProductImages.Remove(imageToDelete);
            }

            db.Products.Remove(obj);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
