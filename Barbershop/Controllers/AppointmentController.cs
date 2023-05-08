using Barbershop.Data;
using Barbershop.Models;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult GetAvailableTime(string barberId, DateTime date)
        {
            // Отримати розклад барбера за вказану дату
            BarberSchedule schedule = _db.BarberSchedule
                .FirstOrDefault(s => s.BarbershopUserId == barberId && s.Date == date);

            if (schedule == null)
            {
                // Розклад не знайдений, повернути порожній список
                return Json(new { success = false, message = "Розклад не знайдено" }) ;
            }

            // Отримати всі записи на цей день для даного барбера
            List<Appointment> appointments = _db.Appointments
                .Where(a => a.BarberId == barberId && a.Date == date)
                .ToList();

            // Отримати початковий та кінцевий час робочого дня барбера
            TimeSpan startTime = schedule.StartTime;
            TimeSpan endTime = schedule.EndTime;

            // Створити список доступних часів
            List<TimeSpan> availableTimes = new List<TimeSpan>();

            // Пройтись по інтервалам часу протягом робочого дня
            TimeSpan currentTime = startTime;
            while (currentTime < endTime)
            {
                // Перевірити, чи цей інтервал часу вільний
                bool isAvailable = true;
                foreach (var appointment in appointments)
                {
                    if (currentTime >= appointment.StartTime && currentTime < appointment.EndTime)
                    {
                        // Час зайнятий, перейти до наступного інтервалу
                        isAvailable = false;
                        break;
                    }
                }

                // Якщо інтервал часу вільний, додати його до списку доступних часів
                if (isAvailable)
                {
                    availableTimes.Add(currentTime);
                }

                // Перейти до наступного інтервалу часу
                currentTime = currentTime.Add(TimeSpan.FromMinutes(30));
            }

            return Json(new { success = true, availableTimes });
        }

    }
}
