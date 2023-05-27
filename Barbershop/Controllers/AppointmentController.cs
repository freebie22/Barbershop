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
                }),
                Appointment = new Appointment()
                {
                    Date = DateTime.Now.Date,
                    BarberId = barberId
                },
                BarberDetails = barbers,
                Services = _db.Services.ToList(),
                              
            };
            return View(appointmentVM);
        }

        public JsonResult SetSelectedBarber(int barberId)
        {
           if (barberId != 0)
           {
               var barber = _db.Barbers.FirstOrDefault(b => b.Id == barberId);
               var barberName = barber.FullName;

               return Json(new { success = true, barberName });
           }

           else
           {
               return Json(new { success = false, message = "Оберіть барбера" });
           }
          
        }

        public JsonResult GetBarberInfo()
        {
            var barbers = _db.Barbers.Include(b => b.WorkPosition).OrderBy(b => b.WorkPosition.PositionName == "Топ-барбер" ? 0 :
                                                                                            b.WorkPosition.PositionName == "Барбер" ? 1 :
                                                                                            b.WorkPosition.PositionName == "Стажер" ? 2 : 3).AsEnumerable();

            List<int> barbersIds = new List<int>();
            List<string> barbersNames = new List<string>();
            List<string> barbersWorkPositions = new List<string>();
            List<string> barbersPhotos = new List<string>();

            if (barbers.Count() > 0)
            {
                foreach (var barber in barbers)
                {
                    var Id = barber.Id;
                    var Name = barber.FullName;
                    var WorkPosition = barber.WorkPosition.PositionName;
                    var Photos = barber.BarberImage;

                    barbersIds.Add(Id);
                    barbersNames.Add(Name);
                    barbersWorkPositions.Add(WorkPosition);
                    barbersPhotos.Add(Photos);
                }

                var barberPath = WC.BarberPath;

                return Json(new { success = true, barbersIds, barbersNames, barbersWorkPositions, barbersPhotos, barberPath });
            }



            else
            {
                return Json(new { success = false, message = "Помилка на стороні сервера" });
            }

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

            var email = appointmentVM.Appointment.Email;

            appointmentVM.Appointment.AppointmentType = "Онлайн";

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




        [HttpPost]
        public JsonResult GetAvailableTime(int barberId, DateTime date)
        {
            BarberSchedule schedule = _db.BarberSchedule.FirstOrDefault(s => s.BarberId == barberId && s.Date == date);

            if (schedule == null)
            {
                return Json(new { success = false, message = "Барбер в цей день не працює" }) ;
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
                    if (currentTime >= appointment.StartTime.Subtract(TimeSpan.FromMinutes(15)) && currentTime < appointment.EndTime)
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

            List<TimeSpan> morningTimes = new List<TimeSpan>();
            List<TimeSpan> dayTimes = new List<TimeSpan>();
            List<TimeSpan> eveningTimes = new List<TimeSpan>();

            if (availableTimes.Count > 0)
            {
                foreach(var time in availableTimes)
                {
                    var hour = time.Hours;

                    if (hour >= 6 && hour < 12)
                    {
                        morningTimes.Add(time);
                    }
                    if (hour >= 12 && hour < 18)
                    {
                        dayTimes.Add(time);
                    }
                    if (hour >= 18 && hour < 24)
                    {
                        eveningTimes.Add(time);
                    }
                }
                
                
                return Json(new { success = true, morningTimes, dayTimes, eveningTimes });
            }           
            else
            {
                return Json(new { success = false, message = "На цей день немає вільних годин для запису" });
            }
            
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

        public JsonResult SetServicePrice(int serviceId, string specialization)
        {
            var service = _db.Services.FirstOrDefault(s=>s.Id == serviceId);

            if(service != null)
            {
                double price = 0;
                switch(specialization)
                {
                    case "Стажер":
                        price = service.traineePrice; 
                        break;
                    case "Барбер":
                        price = service.barberPrice;
                        break;
                    case "Топ-барбер":
                        price = service.seniorPrice;
                        break;

                }

                TimeSpan duration = service.Duration;
                

                return Json(new { success = true, price, duration });
            }
            else
            {
                return Json(new { success = false, message = "Помилка на стороні сервера" });
            }

        }

        [HttpPost]
        public JsonResult CalculateTotalPrice(List<int> selectedServices, string specialization)
        {
            var services = _db.Services.Where(s => selectedServices.Contains(s.Id)).ToList();
            if (services.Any())
            {
                double totalPrice = 0;
                switch(specialization)
                {
                    case "Стажер":
                        totalPrice = services.Sum(s => s.traineePrice);
                        break;
                    case "Барбер": 
                      totalPrice  = services.Sum(s => s.barberPrice);
                        break;
                    case "Топ-барбер":
                        totalPrice = services.Sum(s => s.seniorPrice);
                        break;

                }

                TimeSpan totalDuration = TimeSpan.Zero;

                foreach(var service in services)
                {
                    totalDuration += service.Duration;
                }

                return Json(new { success = true, totalPrice, totalDuration });
            }
            else
            {
                return Json(new { success = false, message = "Помилка на стороні сервера" });
            }
        }

        public JsonResult SetStartTime(TimeSpan startTime)
        {
            if(!(startTime < DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(15))))
            {
                return Json(new {success = true, startTime});
            }
            else
            {
                return Json(new { success = false, message = "Будь-ласка, оберіть час для запису з наявних" });
            }
        }
        public JsonResult SetEndTime(TimeSpan startTime, TimeSpan addTime, int barberId, DateTime scheduleDate)
        {
            BarberSchedule? barber = _db.BarberSchedule.FirstOrDefault(b => b.BarberId == barberId && b.Date == scheduleDate);

            if(barber != null)
            {
                TimeSpan lastTime = barber.EndTime;

                TimeSpan endTime = startTime.Add(addTime);

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
