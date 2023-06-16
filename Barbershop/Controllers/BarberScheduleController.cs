using Barbershop.Data;
using Barbershop.Models;
using Barbershop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Barbershop.Controllers
{
    [Authorize]
    public class BarberScheduleController : Controller
    {
        private readonly ApplicationDbContext _db;

        public BarberScheduleController(ApplicationDbContext db)
        {
            _db = db;
            RemoveSchedule();
        }

        public IActionResult Index(string? searchName)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            List<BarberSchedule> schedules = new List<BarberSchedule>();

            if (User.IsInRole(WC.AdminRole))
            {
                schedules = _db.BarberSchedule.Include(b => b.Barber).OrderBy(b => b.BarberId).ThenBy(b => b.Date).ToList();
                if(!string.IsNullOrEmpty(searchName))
                {
                    schedules = _db.BarberSchedule.Include(b => b.Barber).Where(b => b.Barber.FullName.ToLower().Contains(searchName)).OrderBy(b => b.BarberId).ThenBy(b => b.Date).ToList();
                }
            }

            if (User.IsInRole(WC.BarberRole))
            {
                schedules = _db.BarberSchedule.Include(b => b.Barber).Where(b => b.Barber.BarbershopUserId == claim.Value).OrderBy(b => b.BarberId).ThenBy(b => b.Date).ToList();
            }

            if (User.IsInRole(WC.ClientRole))
            {
                return RedirectToAction("Index", "Home");
            }
            return View(schedules);
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
            if(User.IsInRole(WC.AdminRole))
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

            else
            {
                return RedirectToAction("Index", "Home");
            }

            
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
            if (User.IsInRole(WC.AdminRole))
            {
                if(id == 0 || id == null)
                {
                    TempData[WC.Error] = "Такого розкладу немає";
                    return RedirectToAction("Index");
                }
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

                if(scheduleVM.BarberSchedule == null)
                {
                    TempData[WC.Error] = "Такого розкладу немає";
                    return RedirectToAction("Index");
                }

                return View(scheduleVM);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
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
            if (User.IsInRole(WC.AdminRole))
            {
                if (id == 0 || id == null)
                {
                    TempData[WC.Error] = "Такого розкладу немає";
                    return RedirectToAction("Index");
                }
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

                if (scheduleVM.BarberSchedule == null)
                {
                    TempData[WC.Error] = "Такого розкладу немає";
                    return RedirectToAction("Index");
                }

                return View(scheduleVM);
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
