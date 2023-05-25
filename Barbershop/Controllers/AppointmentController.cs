using Barbershop.Data;
using Barbershop.Models;
using Barbershop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

        public IActionResult Index(int? barberId)
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
        [ActionName("Index")]
        public IActionResult IndexPost(AppointmentVM appointmentVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            appointmentVM.Appointment.UserId = claim.Value;

            appointmentVM.Appointment.AppointmentStatus = WC.AppointmentReceived;

            appointmentVM.Appointment.AppointmentType = "Онлайн";

            appointmentVM.Appointment.PhoneNumber = "+3806850340888";
            appointmentVM.Appointment.Email = "bojkov755@gmail.com";

            if (appointmentVM.ServicesIds == null)
            {
                _db.Entry(appointmentVM.Appointment).Collection(b => b.Services).Load();
            }

            if (appointmentVM.ServicesIds != null)
            {
                var selectedServices = _db.Services.Where(s => appointmentVM.ServicesIds.Contains(s.Id));

                appointmentVM.Appointment.Services = new List<Services>();

                foreach (var services in selectedServices)
                {
                    appointmentVM.Appointment.Services.Add(services);
                }
            }

            _db.Appointments.Add(appointmentVM.Appointment);
            _db.SaveChanges();


            return RedirectToAction("Index", "Home");
        }

        //public IActionResult AppointmentConfirmation(AppointmentVM appointmentVM)
        //{

        //    if(appointmentVM == null)
        //    {
        //        return RedirectToAction("Index");

        //    }

        //    return View(appointmentVM); 
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[ActionName("AppointmentConfirmation")]
        //public IActionResult AppointmentConfirmationPost(AppointmentVM appointmentVM)
        //{
        //    var storedAppointmentVM = appointmentVM;

        //    if(storedAppointmentVM == null)
        //    {
        //        return RedirectToAction("Index");
        //    }

        //    storedAppointmentVM.Appointment.PhoneNumber = appointmentVM.Appointment.PhoneNumber;
        //    storedAppointmentVM.Appointment.Email = appointmentVM.Appointment.Email;
        //    storedAppointmentVM.Appointment.AppointmentStatus = WC.AppointmentReceived;
        //    storedAppointmentVM.Appointment.AppointmentType = "Онлайн";
        //    storedAppointmentVM.Appointment.Comment = appointmentVM.Appointment.Comment;

        //    _db.Appointments.Add(storedAppointmentVM.Appointment);
        //    _db.SaveChanges();

        //    return RedirectToAction("Index");

        //}

        public JsonResult GetAvailableTime(int barberId, DateTime date)
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

        public JsonResult GetWorkPostiion(int barberId)
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

        public JsonResult SetServicePrice(int serviceId)
        {
            var service = _db.Services.FirstOrDefault(s=>s.Id == serviceId);

            if(service != null)
            {
                var traineePrice = service.traineePrice;
                var barberPrice = service.barberPrice;
                var seniorPrice = service.seniorPrice;

                return Json(new { success = true, traineePrice, barberPrice, seniorPrice });
            }
            else
            {
                return Json(new { success = false, message = "Помилка на стороні сервера" });
            }

        }

        [HttpPost]
        public JsonResult CalculateTotalPrice(List<int> selectedServices)
        {
            var services = _db.Services.Where(s => selectedServices.Contains(s.Id)).ToList();
            if (services.Any())
            {
                double totalTraineePrice = services.Sum(s => s.traineePrice);
                double totalBarberPrice = services.Sum(s => s.barberPrice);
                double totalSeniorPrice = services.Sum(s => s.seniorPrice);

                return Json(new { success = true, totalTraineePrice, totalBarberPrice, totalSeniorPrice });
            }
            else
            {
                return Json(new { success = false, message = "Помилка на стороні сервера" });
            }
        }

        public JsonResult SetEndTime(TimeSpan startTime, int addTime, int barberId, DateTime scheduleDate)
        {
            BarberSchedule barber = _db.BarberSchedule.FirstOrDefault(b => b.BarberId == barberId && b.Date == scheduleDate);

            if(barber != null)
            {
                TimeSpan lastTime = barber.EndTime;

                TimeSpan endTime = startTime.Add(TimeSpan.FromMinutes(addTime));

                if (endTime <= lastTime)
                {
                    return Json(new { success = true, endTime });
                }

                else
                {
                    return Json(new { success = false, message = "Барбер в цей час не працює" });
                }
            }

            else
            {
                return Json(new { success = false, message = "Помилка на сервері" });
            }

                  
        }

    }
}
