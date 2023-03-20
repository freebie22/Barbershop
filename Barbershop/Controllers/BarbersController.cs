using Barbershop.Data;
using Barbershop.Models;
using Barbershop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Barbershop.Controllers
{
    public class BarbersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BarbersController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<Barbers> objList = _db.Barbers.Include(b => b.WorkPosition).Include(b => b.Specializations);
            return View(objList);
        }

        //GET - Create
        public IActionResult Create(int? id)
        {
            BarbersVM barberVM = new BarbersVM()
            {
                Barber = new Barbers(),
                WorkPostionsSelectList = _db.WorkPositions.Select(i => new SelectListItem
                {
                    Text = i.PositionName,
                    Value = i.Id.ToString()
                }
                ),
                Specializations = _db.Specializations.ToList()
            };
            return View(barberVM);
        }

        //POST - Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BarbersVM barberVM)
        {
            var files = HttpContext.Request.Form.Files;
            string webRootPath = _webHostEnvironment.WebRootPath;

            //create
            string upload = webRootPath + WC.BarberPath;
            string fileName = Guid.NewGuid().ToString();
            string extension = Path.GetExtension(files[0].FileName);

            using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
            {
                files[0].CopyTo(fileStream);
            }

            barberVM.Barber.BarberImage = fileName + extension;

            barberVM.Barber.Specializations = new List<Specializations>();

            foreach (var specializationId in barberVM.SpecializationIds)
            {
                var specialization = _db.Specializations.Find(specializationId);
                if (specialization != null)
                {
                    barberVM.Barber.Specializations.Add(specialization);
                }
            }

            _db.Barbers.Add(barberVM.Barber);

            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        //GET - Edit
        public IActionResult Edit(int? id)
        {
            BarbersVM barberVM = new BarbersVM()
            {
                Barber = new Barbers(),
                WorkPostionsSelectList = _db.WorkPositions.Select(i => new SelectListItem
                {
                    Text = i.PositionName,
                    Value = i.Id.ToString()
                }
                ),
                Specializations = _db.Specializations.ToList()
            };

            barberVM.Barber = _db.Barbers.Find(id);
            if (barberVM.Barber == null)
            {
                return NotFound();
            }

            return View(barberVM);
        }

        //POST - Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BarbersVM barberVM, int id)
        {
            var files = HttpContext.Request.Form.Files;
            string webRootPath = _webHostEnvironment.WebRootPath;

            var objFromDb = _db.Barbers.AsNoTracking().FirstOrDefault(u => u.Id == barberVM.Barber.Id);

            if (files.Count > 0)
            {
                string upload = webRootPath + WC.BarberPath;
                string fileName = Guid.NewGuid().ToString();
                string extension = Path.GetExtension(files[0].FileName);

                var oldFile = Path.Combine(upload, objFromDb.BarberImage);

                if (System.IO.File.Exists(oldFile))
                {
                    System.IO.File.Delete(oldFile);
                }

                using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }

                barberVM.Barber.BarberImage = fileName + extension;
            }

            else
            {
                barberVM.Barber.BarberImage = objFromDb.BarberImage;
            }


            if (barberVM.SpecializationIds != null)
            {
                var selectedSpecializations = _db.Specializations.Where(s => barberVM.SpecializationIds.Contains(s.Id));

                if (barberVM.Barber.Specializations != null)
                {
                    barberVM.Barber.Specializations.Clear();
                }
                else
                {
                    barberVM.Barber.Specializations = new List<Specializations>();
                }

                foreach (var specialization in selectedSpecializations)
                {
                    barberVM.Barber.Specializations.Add(specialization);
                }
            }

            _db.Barbers.Update(barberVM.Barber);

            _db.SaveChanges();


            return RedirectToAction("Index");
        }

        //GET - Delete
        public IActionResult Delete(int? id)
        {
            BarbersVM barberVM = new BarbersVM()
            {
                Barber = new Barbers(),
                WorkPostionsSelectList = _db.WorkPositions.Select(i => new SelectListItem
                {
                    Text = i.PositionName,
                    Value = i.Id.ToString()
                }
                ),
                Specializations = _db.Specializations.ToList()
            };

            barberVM.Barber = _db.Barbers.Find(id);
            if (barberVM.Barber == null)
            {
                return NotFound();
            }

            return View(barberVM);
        }

        //POST - Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.Barbers.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            string upload = _webHostEnvironment.WebRootPath + WC.BarberPath;

            var oldFile = Path.Combine(upload, obj.BarberImage);

            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }

            _db.Barbers.Remove(obj);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}
