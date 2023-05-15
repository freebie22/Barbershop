using Barbershop.Data;
using Barbershop.Models;
using Barbershop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Barbershop.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AppointmentController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(int barberId = 0)
        {
            List<Barbers> barbers = _db.Barbers.Include(b => b.WorkPosition).OrderBy(b=> b.WorkPosition.PositionName == "Топ-барбер" ? 0 : 
                                                                                            b.WorkPosition.PositionName == "Барбер" ? 1 :
                                                                                            b.WorkPosition.PositionName == "Стажер" ? 2 : 3).ToList();
            AppointmentVM appointmentVM = new AppointmentVM()
            {
                Barbers = barbers.Select(i => new SelectListItem
                {
                    Text = i.FullName,
                    Value = i.Id.ToString(),
                    Selected = i.Id == barberId,
                }),
                Appointment = new Appointment()
                {
                    Date = DateTime.Now.Date,
                },
                BarberDetails = barbers,
                Services = _db.Services.ToList(),
                              
            };
            return View(appointmentVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("IndexPost")]
        public IActionResult Index(AppointmentVM appointmentVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            Appointment appointment = new Appointment()
            {
                UserId = claim.Value,
                BarberId = appointmentVM.Appointment.BarberId,
                Date = appointmentVM.Appointment.Date,
                StartTime = appointmentVM.Appointment.StartTime,
                EndTime = appointmentVM.Appointment.StartTime.Add(TimeSpan.FromMinutes(10)),
                TotalPrice= appointmentVM.Appointment.TotalPrice,

            };

            return View(appointmentVM); 
        }

        public IActionResult GetAvailableTime(int barberId, DateTime date)
        {
            BarberSchedule schedule = _db.BarberSchedule.FirstOrDefault(s => s.BarberId == barberId && s.Date == date);

            if (schedule == null)
            {
                return Json(new { success = false, message = "На дану дату відсутні години для запису" }) ;
            }


            List<Appointment> appointments = _db.Appointments
                .Where(a => a.BarberId == barberId && a.Date == date)
                .ToList();


            TimeSpan startTime = schedule.StartTime;
            TimeSpan endTime = schedule.EndTime;
            DateTime day = schedule.Date;


            List<TimeSpan> availableTimes = new List<TimeSpan>();


            TimeSpan currentTime = startTime;
            while (currentTime <= endTime.Subtract(TimeSpan.FromMinutes(30)))
            {

                bool isAvailable = true;
                foreach (var appointment in appointments)
                {
                    if (currentTime >= appointment.StartTime && currentTime < appointment.EndTime)
                    {
                        isAvailable = false;
                        break;
                    }
                }

                if (isAvailable && !(day.Date == DateTime.Today && currentTime < DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(30))))
                {
                    availableTimes.Add(currentTime);
                }

                currentTime += TimeSpan.FromMinutes(15);
            }
           
            return Json(new { success = true, availableTimes });
        }

        public IActionResult GetWorkPostiion(int barberId)
        {
            var barber = _db.Barbers.Include(b=> b.WorkPosition).FirstOrDefault(b => b.Id== barberId);

            if(barber == null)
            {
                return Json(new { success = false, message = "Такого барбера не існує" });
            }

            string specialization;

            switch(barber.WorkPosition.PositionName)
            {
                case "Стажер":
                    specialization = "Стажер";
                    break;
                case "Барбер":
                    specialization = "Барбер";
                    break;
                case "Топ-барбер":
                    specialization = "Топ-барбер";
                    break;
                default:
                    specialization = "Не знайдено";
                    break;
            }
            

            return Json(new { success = true, specialization });

        }

    }
}
