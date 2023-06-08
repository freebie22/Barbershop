using Barbershop.Data;
using Barbershop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Data;

namespace Barbershop.Controllers
{
    [Authorize]
    public class SpecializationsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SpecializationsController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(string? searchName)
        {
            if(User.IsInRole(WC.AdminRole))
            {
                IEnumerable<Specializations> objList = _db.Specializations;
                if(!string.IsNullOrEmpty(searchName))
                {
                    objList = _db.Specializations.Where(o => o.SpecName.ToLower().Contains(searchName));
                }
                return View(objList);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
        }
        //GET - Create
        public IActionResult Create()
        {
            return View();
        }

        //POST - Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Specializations obj)
        {
            var files = HttpContext.Request.Form.Files;
            string webRootPath = _webHostEnvironment.WebRootPath;

            //create
            string upload = webRootPath + WC.SpecPath;
            string fileName = Guid.NewGuid().ToString();
            string extension = Path.GetExtension(files[0].FileName);

            using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
            {
                files[0].CopyTo(fileStream);
            }

            obj.SpecImage = fileName + extension;

            _db.Specializations.Add(obj);

            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        //GET - Edit
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var obj = _db.Specializations.Find(id);

            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }

        //POST - Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Specializations obj, int id)
        {
            var files = HttpContext.Request.Form.Files;
            string webRootPath = _webHostEnvironment.WebRootPath;

            var objFromDb = _db.Specializations.AsNoTracking().FirstOrDefault(u => u.Id == obj.Id);

            if (files.Count > 0)
            {
                string upload = webRootPath + WC.SpecPath;
                string fileName = Guid.NewGuid().ToString();
                string extension = Path.GetExtension(files[0].FileName);

                var oldFile = Path.Combine(upload, objFromDb.SpecImage);

                if (System.IO.File.Exists(oldFile))
                {
                    System.IO.File.Delete(oldFile);
                }

                using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }

                obj.SpecImage = fileName + extension;
            }

            else
            {
                obj.SpecImage = objFromDb.SpecImage;
            }

            _db.Specializations.Update(obj);

            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        //GET - Delete
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var obj = _db.Specializations.Find(id);

            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }

        //POST - Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.Specializations.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            string upload = _webHostEnvironment.WebRootPath + WC.SpecPath;

            var oldFile = Path.Combine(upload, obj.SpecImage);

            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }

            _db.Specializations.Remove(obj);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
