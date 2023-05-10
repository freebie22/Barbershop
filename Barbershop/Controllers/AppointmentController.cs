using Barbershop.Data;
using Barbershop.Models;
using Barbershop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
            AppointmentVM appointmentVM = new AppointmentVM()
            {
                Barbers = _db.Barbers.Select(i => new SelectListItem
                {
                    Text = i.FullName,
                    Value = i.Id.ToString(),
                    Selected = i.Id == barberId,
                }),
                Appointment = new Appointment()
                {
                    Date = DateTime.Now.Date,
                }
               
            };
            return View(appointmentVM);
        }

        public IActionResult GetAvailableTime(int barberId, DateTime date)
        {
            BarberSchedule schedule = _db.BarberSchedule.FirstOrDefault(s => s.BarberId == barberId && s.Date == date);

            if (schedule == null)
            {
                return Json(new { success = false, message = "Розклад не знайдено" }) ;
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

                if (isAvailable && !(day.Date == DateTime.Today && currentTime < DateTime.Now.TimeOfDay))
                {
                    availableTimes.Add(currentTime);
                }

                currentTime += TimeSpan.FromMinutes(15);
            }
           
            return Json(new { success = true, availableTimes });
        }

    }
}
