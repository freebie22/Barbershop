using Barbershop.Data;
using Barbershop.Models;
using Barbershop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Barbershop.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class BarberMainController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BarberMainController(ApplicationDbContext db, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Barbers> objList = _db.Barbers.ToList();

            return View(objList);
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


            if (barberVM.SpecializationIds == null)
            {
                _db.Entry(barberVM.Barber).Collection(b => b.Specializations).Load();
            }

            if (barberVM.SpecializationIds != null)
            {
                var selectedSpecializations = _db.Specializations.Where(s => barberVM.SpecializationIds.Contains(s.Id));

                barberVM.Barber.Specializations = new List<Specializations>();

                foreach (var specialization in selectedSpecializations)
                {
                    barberVM.Barber.Specializations.Add(specialization);
                }
            }

            _db.Barbers.Update(barberVM.Barber);

            

            _db.SaveChanges();


            return RedirectToAction("Index");
        }

    }
}
