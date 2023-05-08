using Barbershop.Data;
using Barbershop.Models;
using Barbershop.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Barbershop.Controllers
{
    public class BarberScheduleController : Controller
    {
        private readonly ApplicationDbContext _db;

        public BarberScheduleController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<BarberSchedule> objList = _db.BarberSchedule.Include(b => b.BarbershopUser).ToList();
            return View(objList);
        }

        public IActionResult Create()
        {
            var barberRole = _db.Roles.FirstOrDefault(r => r.Name == "Barber");
            var barberRoleId = barberRole?.Id;

            var barbers = _db.BarbershopUser
                .Where(u => _db.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == barberRoleId))
                .OrderBy(u => u.FullName)
                .ToList();

            BarberScheduleVM scheduleVM = new BarberScheduleVM()
            {
                BarberSchedule = new BarberSchedule()
                {
                    Date = DateTime.Now,
                },
                BarbershopUserSelectList = barbers.Select(i => new SelectListItem
                {
                    Text = i.FullName,
                    Value = i.Id.ToString(),
                })
            };

            return View(scheduleVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BarberScheduleVM scheduleVM)
        {
            _db.BarberSchedule.Add(scheduleVM.BarberSchedule);        
            _db.SaveChanges();

            return RedirectToAction("Index");

        }

        public IActionResult Edit(int? id)
        {
            var barberRole = _db.Roles.FirstOrDefault(r => r.Name == "Barber");
            var barberRoleId = barberRole?.Id;

            var barbers = _db.BarbershopUser
                .Where(u => _db.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == barberRoleId))
                .OrderBy(u => u.FullName)
                .ToList();

            BarberScheduleVM scheduleVM = new BarberScheduleVM()
            {
                BarberSchedule = new BarberSchedule()
                {
                    Date = DateTime.Now,
                },
                BarbershopUserSelectList = barbers.Select(i => new SelectListItem
                {
                    Text = i.FullName,
                    Value = i.Id.ToString(),
                })
            };

            scheduleVM.BarberSchedule = _db.BarberSchedule.FirstOrDefault(b => b.Id == id);

            return View(scheduleVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BarberScheduleVM scheduleVM)
        {

            _db.BarberSchedule.Update(scheduleVM.BarberSchedule);
            _db.SaveChanges();

            return RedirectToAction("Index");

        }

        public IActionResult Delete(int? id)
        {
            var barberRole = _db.Roles.FirstOrDefault(r => r.Name == "Barber");
            var barberRoleId = barberRole?.Id;

            var barbers = _db.BarbershopUser
                .Where(u => _db.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == barberRoleId))
                .OrderBy(u => u.FullName)
                .ToList();

            BarberScheduleVM scheduleVM = new BarberScheduleVM()
            {
                BarberSchedule = new BarberSchedule()
                {
                    Date = DateTime.Now,
                },
                BarbershopUserSelectList = barbers.Select(i => new SelectListItem
                {
                    Text = i.FullName,
                    Value = i.Id.ToString(),
                })
            };

            scheduleVM.BarberSchedule = _db.BarberSchedule.FirstOrDefault(b => b.Id == id);

            return View(scheduleVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.BarberSchedule.Find(id);

            if(obj == null)
            {
                return NotFound();
            }

            _db.BarberSchedule.Remove(obj);
            _db.SaveChanges();

            return RedirectToAction("Index");

        }
    }
}
