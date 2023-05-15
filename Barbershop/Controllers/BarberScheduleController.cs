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
            RemoveSchedule();
        }

        public IActionResult Index()
        {
            var objList = _db.BarberSchedule.Include(b => b.Barber).ToList();
            return View(objList);
        }

        private void RemoveSchedule()
        {
            var objList = _db.BarberSchedule.Include(b => b.Barber).ToList();
            foreach (var obj in objList)
            {
                if (obj.Date < DateTime.Now.Date)
                {
                    _db.BarberSchedule.Remove(obj);
                }
            }
            _db.SaveChanges();
        }

        public IActionResult Create()
        {

            var barbers = _db.Barbers.OrderBy(b => b.FullName).ToList();

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
            var barbers = _db.Barbers.OrderBy(b => b.FullName).ToList();

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

            var existingSchedule = _db.BarberSchedule.FirstOrDefault(b => b.Id == scheduleVM.BarberSchedule.Id);
            if (existingSchedule != null)
            {
                existingSchedule.Date = scheduleVM.BarberSchedule.Date;
                existingSchedule.StartTime = scheduleVM.BarberSchedule.StartTime;
                existingSchedule.EndTime = scheduleVM.BarberSchedule.EndTime;

                _db.BarberSchedule.Update(existingSchedule);
                _db.SaveChanges();
            }

            return RedirectToAction("Index");

        }

        public IActionResult Delete(int? id)
        {
            var barbers = _db.Barbers.OrderBy(b => b.FullName).ToList();

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
