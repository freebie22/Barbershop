using Barbershop.Data;
using Barbershop.Models.ViewModels;
using Barbershop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace Barbershop.Controllers
{
    public class NewsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NewsController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

            
        public IActionResult Index()
        {
            if(User.IsInRole(WC.AdminRole))
            {
                var objList = _db.News.Include(o => o.NewsImages).Include(o => o.User).ToList();
                return View(objList);
            }
            else
            {
                return RedirectToAction("NewsList");
            }
        }

        public IActionResult NewsList()
        {
            NewsListVM newsListVM = new NewsListVM()
            {
                News = _db.News,
            };
            return View(newsListVM);
        }

        public IActionResult NewsListDetails(int id)
        {
            var news = _db.News.Include(p => p.User).Include(p => p.NewsImages).Where(p => p.Id == id).FirstOrDefault();
 
            return View(news);
        }

        public IActionResult Create()
        {
            if (User.IsInRole(WC.AdminRole))
            {
                NewsVM newsVM = new NewsVM()
                {
                    News = new News(),
                    NewsImages = _db.NewsImages.ToList()
                };
                return View(newsVM);
            }
            else
            {
                return RedirectToAction("NewsList");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NewsVM newsVM)
        {
            var files1 = HttpContext.Request.Form.Files.GetFile("files1");
            var files2 = HttpContext.Request.Form.Files.GetFiles("files2");
            string webRootPath = _webHostEnvironment.WebRootPath;

            string upload = webRootPath + WC.NewsMainPath;
            string fileName = Guid.NewGuid().ToString();
            string extension = Path.GetExtension(files1.FileName);

            using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
            {
                files1.CopyTo(fileStream);
            }

            newsVM.News.MainPhoto = fileName + extension;

            newsVM.News.NewsImages = new List<NewsImages>();

            newsVM.News.CreatedDate = DateTime.Now;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            newsVM.News.UserId = claim.Value;

            for (int i = 0; i < files2.Count; i++)
            {
                var imgUpload = webRootPath + WC.NewsGalleryPath;
                var imgName = Guid.NewGuid().ToString();
                var imgExtension = Path.GetExtension(files2[i].FileName);

                using (var fileStream = new FileStream(Path.Combine(imgUpload, imgName + imgExtension), FileMode.Create))
                {
                    files2[i].CopyTo(fileStream);
                }
                var newsImage = new NewsImages()
                {
                    Image = imgName + imgExtension
                };
                newsVM.News.NewsImages.Add(newsImage);
            }
            _db.News.Add(newsVM.News);

            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int? id)
        {
            if (User.IsInRole(WC.AdminRole))
            {
                NewsVM newsVM = new NewsVM()
                {
                    News = new News(),
                    NewsImages = _db.NewsImages.ToList()
                };


                newsVM.News = _db.News.Include(o => o.NewsImages).Include(o => o.User).FirstOrDefault(p => p.Id == id);

                newsVM.NewsImages = newsVM.News.NewsImages.ToList();


                if (newsVM.News == null)
                {
                    return RedirectToAction("Index");
                }

                return View(newsVM);
            }

            else
            {
                return RedirectToAction("NewsList");
            }
        }

        //POST - Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(NewsVM newsVM, int id)
        {
            var files1 = HttpContext.Request.Form.Files.GetFile("files1");
            var files2 = HttpContext.Request.Form.Files.GetFiles("files2");

            string webRootPath = _webHostEnvironment.WebRootPath;

            var objFromDb = _db.News.AsNoTracking().FirstOrDefault(u => u.Id == newsVM.News.Id);

            if (files1 != null)
            {
                string upload = webRootPath + WC.NewsMainPath;
                string fileName = Guid.NewGuid().ToString();
                string extension = Path.GetExtension(files1.FileName);

                var oldFile = Path.Combine(upload, objFromDb.MainPhoto);

                if (System.IO.File.Exists(oldFile))
                {
                    System.IO.File.Delete(oldFile);
                }

                using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                {
                    files1.CopyTo(fileStream);
                }

                newsVM.News.MainPhoto = fileName + extension;
            }

            else
            {
                newsVM.News.MainPhoto = objFromDb.MainPhoto;
            }

            if (files2.Count > 0)
            {
                newsVM.News.NewsImages = new List<NewsImages>();

                for (int i = 0; i < files2.Count; i++)
                {
                    var imgUpload = webRootPath + WC.GalleryPath;
                    var imgName = Guid.NewGuid().ToString();
                    var imgExtension = Path.GetExtension(files2[i].FileName);

                    using (var fileStream = new FileStream(Path.Combine(imgUpload, imgName + imgExtension), FileMode.Create))
                    {
                        files2[i].CopyTo(fileStream);
                    }
                    var newsImage = new NewsImages()
                    {
                        Image = imgName + imgExtension
                    };
                    newsVM.News.NewsImages.Add(newsImage);
                }
            }

            else
            {
                newsVM.News.NewsImages = objFromDb.NewsImages;
            }

            newsVM.News.UserId = objFromDb.UserId;
            newsVM.News.CreatedDate = DateTime.Now;


            _db.News.Update(newsVM.News);

            _db.SaveChanges();


            return RedirectToAction("Index");
        }

        public IActionResult Delete(int? id)
        {
            if (User.IsInRole(WC.AdminRole))
            {
                NewsVM newsVM = new NewsVM()
                {
                    News = new News(),
                    NewsImages = _db.NewsImages.ToList()
                };


                newsVM.News = _db.News.Include(o => o.NewsImages).Include(o => o.User).FirstOrDefault(p => p.Id == id);

                newsVM.NewsImages = newsVM.News.NewsImages.ToList();


                if (newsVM.News == null)
                {
                    return RedirectToAction("Index");
                }

                return View(newsVM);
            }

            else
            {
                return RedirectToAction("NewsList");
            }
        }

        //POST - Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.News.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            string upload = _webHostEnvironment.WebRootPath + WC.NewsMainPath;


            var oldFile = Path.Combine(upload, obj.MainPhoto);

            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }

            string uploads = _webHostEnvironment.WebRootPath + WC.NewsGalleryPath;

            var imagesToDelete = _db.NewsImages.Where(pi => pi.News.Any(p => p.Id == obj.Id)).ToList();

            foreach (var imageToDelete in imagesToDelete)
            {
                var imageFile = Path.Combine(uploads, imageToDelete.Image);
                if (System.IO.File.Exists(imageFile))
                {
                    System.IO.File.Delete(imageFile);
                }
               _db.NewsImages.Remove(imageToDelete);
            }

            _db.News.Remove(obj);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
