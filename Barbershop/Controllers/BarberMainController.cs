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

        public IActionResult Index(string? searchName)
        {
            if(User.IsInRole(WC.AdminRole))
            {
                List<Barbers> objList = _db.Barbers.Include(o => o.Specializations).ToList();

                if(!string.IsNullOrEmpty(searchName))
                {
                    objList = _db.Barbers.Include(o => o.Specializations).Where(o => o.FullName.Contains(searchName)).ToList();
                }

                return View(objList);
            }

            else
            {
                return RedirectToAction("Index", "Home");
            }
        }



        //GET - Edit
        public IActionResult Edit(int? id)
        {
            if (User.IsInRole(WC.AdminRole))
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

                barberVM.Barber = _db.Barbers.Include(b => b.Specializations).FirstOrDefault(b => b.Id == id);

                if (barberVM.Barber == null)
                {
                    TempData[WC.Error] = "Барбера з таким ідентифікатором не існує";
                    return RedirectToAction("Index");

                }

                return View(barberVM);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        //POST - Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BarbersVM barberVM, int id)
        {
            var files = HttpContext.Request.Form.Files;
            string webRootPath = _webHostEnvironment.WebRootPath;

            var objFromDb = _db.Barbers.AsNoTracking().Include(u => u.Specializations).FirstOrDefault(u => u.Id == barberVM.Barber.Id);
 

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


            var selectedSpecializations = _db.Specializations.Where(s => barberVM.SpecializationIds.Contains(s.Id)).ToList();

            var newSpecializations = new List<Specializations>();


            foreach (var specialization in selectedSpecializations)
            {
                bool exists = false;

                foreach (var existingSpecialization in objFromDb.Specializations)
                {
                    if (existingSpecialization.Id == specialization.Id)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    newSpecializations.Add(specialization);
                }
            }

            if(newSpecializations.Count > 0)
            {
                barberVM.Barber.Specializations = newSpecializations;
            }
          
            _db.Barbers.Update(barberVM.Barber);


            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int? id)
        {
            if (User.IsInRole(WC.AdminRole))
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
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

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
