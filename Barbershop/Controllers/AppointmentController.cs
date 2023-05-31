using Barbershop.Data;
using Barbershop.Models;
using Barbershop.Models.ViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;
using Syncfusion.EJ2.Inputs;
using System.Drawing;
using System.Globalization;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Barbershop.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        public AppointmentController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
            RemoveSchedule();
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

        public IActionResult Index(int barberId = 0)
        {
            List<Barbers> barbers = _db.Barbers.Include(b => b.WorkPosition).OrderBy(b => b.WorkPosition.PositionName == "Топ-барбер" ? 0 :
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
                    Date = DateTime.MinValue,
                    BarberId = barberId
                },
                BarberDetails = barbers,
                Services = _db.Services.ToList(),

            };
            return View(appointmentVM);
        }

        public JsonResult GetDateAndDayForAppointment(DateTime date)
        {
            DateTime day = date;
            string dayStr = day.ToShortDateString();
            CultureInfo ukrainianCulture = new CultureInfo("uk-UA");
            string dayOfWeek = ukrainianCulture.DateTimeFormat.GetDayName(day.DayOfWeek);

            return Json(new { success = true, dayStr, dayOfWeek });

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
                                                                                            b.WorkPosition.PositionName == "Стажер" ? 2 : 3).ToList();

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

            appointmentVM.Appointment.AppointmentType = WC.ClientOnline;

            appointmentVM.Appointment.AppointmentDateAndTime = DateTime.Now;

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

            var selectedBarber = _db.Barbers.Include(b => b.WorkPosition).FirstOrDefault(b => b.Id == appointmentVM.Appointment.BarberId);

            string subject = $"Запис до барбера {selectedBarber.FullName}";

            var servicesEmail = appointmentVM.Appointment.Services;

            string servicesHtml = string.Empty;

            if (selectedBarber.WorkPosition.PositionName == "Стажер")
            {
                foreach (var service in appointmentVM.Appointment.Services)
                {
                    servicesHtml += $"{service.serviceName} у стажера - {service.traineePrice} грн. <br />";
                }
            }

            if (selectedBarber.WorkPosition.PositionName == "Барбер")
            {
                foreach (var service in appointmentVM.Appointment.Services)
                {
                    servicesHtml += $"{service.serviceName} у барбера - {service.barberPrice} грн. <br />";
                }
            }

            if (selectedBarber.WorkPosition.PositionName == "Топ-барбеер")
            {
                foreach (var service in appointmentVM.Appointment.Services)
                {
                    servicesHtml += $"{service.serviceName} у топ-барбера - {service.seniorPrice} грн. <br />";
                }
            }

            string messageBody = $@"
                <!DOCTYPE html>
                            <html>
                            <head>
                                <title></title>
                                <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
                                <meta name='viewport' content='width=device-width, initial-scale=1'>
                                <meta http-equiv='X-UA-Compatible' content='IE=edge' />
                                <style type='text/css'>
	
                                    @media screen {{
                                        @font-face {{
                                            font-family: 'Lato';
                                            font-style: normal;
                                            font-weight: 400;
                                            src: local('Lato Regular'), local('Lato-Regular'), url(https://fonts.gstatic.com/s/lato/v11/qIIYRU-oROkIk8vfvxw6QvesZW2xOQ-xsNqO47m55DA.woff) format('woff');
                                        }}

                                        @font-face {{
                                            font-family: 'Lato';
                                            font-style: normal;
                                            font-weight: 700;
                                            src: local('Lato Bold'), local('Lato-Bold'), url(https://fonts.gstatic.com/s/lato/v11/qdgUG4U09HnJwhYI-uK18wLUuEpTyoUstqEm5AMlJo4.woff) format('woff');
                                        }}

                                        @font-face {{
                                            font-family: 'Lato';
                                            font-style: italic;
                                            font-weight: 400;
                                            src: local('Lato Italic'), local('Lato-Italic'), url(https://fonts.gstatic.com/s/lato/v11/RYyZNoeFgb0l7W3Vu1aSWOvvDin1pK8aKteLpeZ5c0A.woff) format('woff');
                                        }}

                                        @font-face {{
                                            font-family: 'Lato';
                                            font-style: italic;
                                            font-weight: 700;
                                            src: local('Lato Bold Italic'), local('Lato-BoldItalic'), url(https://fonts.gstatic.com/s/lato/v11/HkF_qI1x_noxlxhrhMQYELO3LdcAZYWl9Si6vvxL-qU.woff) format('woff');
                                        }}
                                    }}

                                    /* CLIENT-SPECIFIC STYLES */
                                    body,
                                    table,
                                    td,
                                    a {{
                                        -webkit-text-size-adjust: 100%;
                                        -ms-text-size-adjust: 100%;
                                    }}

                                    table,
                                    td {{
                                        mso-table-lspace: 0pt;
                                        mso-table-rspace: 0pt;
                                    }}

                                    img {{
                                        -ms-interpolation-mode: bicubic;
                                    }}

                                    /* RESET STYLES */
                                    img {{
                                        border: 0;
                                        height: auto;
                                        line-height: 100%;
                                        outline: none;
                                        text-decoration: none;
                                    }}

                                    table {{
                                        border-collapse: collapse !important;
                                    }}

                                    body {{
                                        height: 100% !important;
                                        margin: 0 !important;
                                        padding: 0 !important;
                                        width: 100% !important;
                                    }}

                                    /* iOS BLUE LINKS */
                                    a[x-apple-data-detectors] {{
                                        color: inherit !important;
                                        text-decoration: none !important;
                                        font-size: inherit !important;
                                        font-family: inherit !important;
                                        font-weight: inherit !important;
                                        line-height: inherit !important;
                                    }}

                                    /* MOBILE STYLES */
                                    @media screen and (max-width:600px) {{
                                        h1 {{
                                            font-size: 32px !important;
                                            line-height: 32px !important;
                                        }}
                                    }}

                                    /* ANDROID CENTER FIX */
                                    div[style*='margin: 16px 0;'] {{
                                        margin: 0 !important;
                                    }}
                                </style>
                            </head>

                            <body style='background-color: #f4f4f4; margin: 0 !important; padding: 0 !important;'>
                            <!-- HIDDEN PREHEADER TEXT -->
                            <table border='0' cellpadding='0' cellspacing='0' width='100%'>
                                <!-- LOGO -->
                                <tr>
                                    <td bgcolor='#000000' align='center'>
                                        <table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px;'>
                                            <tr>
                                                <td align='center' valign='top' style='padding: 40px 10px 40px 10px;'> </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td bgcolor='#000000' align='center' style='padding: 0px 10px 0px 10px;'>
                                        <table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px;'>
                                            <tr>
                                                <td bgcolor='#ffffff' align='center' valign='top' style='padding: 40px 20px 0px 20px; border-radius: 4px 4px 0px 0px; color: #111111; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 48px; font-weight: 400; letter-spacing: 4px; line-height: 48px;'>
                                                    <img src=""https://i.ibb.co/tDWZZnD/Oasis.jpg"" width = '225' height = '120' alt=""""Oasis"""" border=""""0"""" />
                                                     <h1 style='font-size: 24px; font-weight: 400; margin: 2;'>Ви оформили запис до барбера у нашому салоні!</h1> 
                                                </td>
                                            </tr>
                                            
                                            <tr>
                                                <td bgcolor='#ffffff' align='center' style='padding: 0px 30px 20px 30px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'>
                                                    <p style='margin: 0;'>Деталі Вашого запису</p>
                                                    <table width=""95%"" border=""0"" align=""center"" cellpadding=""0"" cellspacing=""0"">

                                                        <tr>
                                                            <td width=""100%"" style=""color:darkslategrey; font-family:Arial, Helvetica, sans-serif; padding:10px;"">
                                                                <div style=""font-size:16px; color:#564319;"">
                                                                    Інформація про клієнта: 
                                                                </div>
                                                                <div style=""font-size:16px;padding-left:15px;"">
                                                                    Ел. пошта &nbsp;: {appointmentVM.Appointment.Email};
                                                                    <br />
                                                                    Номер телефону : {appointmentVM.Appointment.PhoneNumber};
                                                                </div>
                                                                <br />
                                                                <hr>
                        
                                                            </td>
                                                        </tr>
                        
                                                        <tr>
                                                            <td width=""100%"" style=""color:darkslategrey; font-family:Arial, Helvetica, sans-serif; padding:10px;"">
                                                                <div style=""font-size:16px; color:#564319;"">
                                                                    <span class=""text-center"">Інформація про барбера:</span>
                                                                </div>
                                                                <div style=""font-size:16px; color:#525252;padding-left:15px;"">
                                                                    Ім'я та посада: {selectedBarber.FullName} - {selectedBarber.WorkPosition.PositionName};
                                                                    <br />
                                                                    Контактні дані: Тел.:{selectedBarber.PhoneNumber}
                                                                    E-Mail:{selectedBarber.Email};
                                                                </div>
                                                            </td>
                                                        </tr>
                                                <tr>
                                                <td bgcolor='#ffffff' width=""100%"" style=""color:darkslategrey; font-family:Arial, Helvetica, sans-serif; padding:10px;"">
                                                                <br />
                                                                <hr>
                                                                <div style=""font-size:16px; color:#564319;"">
                                                                    Інформація про запис: 
                                                                </div>
                                                                <div style=""font-size:16px;padding-left:15px;"">
                                                                    Дата запису : {appointmentVM.Appointment.Date.ToShortDateString()}
                                                                    <br />
                                                                    Час початку : {appointmentVM.Appointment.StartTime}
                                                                    <br />
                                                                    Орієнтовний час закінчення : {appointmentVM.Appointment.EndTime}
                                                                </div>
                                                                <div style=""font-size:16px; color:#564319;"">
                                                                    Обрані послуги: 
                                                                </div>
                                                                <div style=""font-size:16px;padding-left:15px;"">
                                                                    {servicesHtml}
                                                                    Загальна ціна за послуги: {appointmentVM.Appointment.TotalPrice} грн.
                                                                </div>

                                                                <br />
                                                                <hr>
                        
                                                            </td>
                                            </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td bgcolor='#f4f4f4' align='center' style='padding: 0px 10px 0px 10px;'>
                                        <table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px;'>
                                            <tr>
                                                <td bgcolor='#ffffff' align='left' style='padding: 0px 30px 40px 30px; border-radius: 0px 0px 4px 4px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'>
                                                    <p style='margin: 0;'>Наші соціальні мережі:</p>
                                                    <div>
                                                        <a style='padding-right:10px' href='https://www.instagram.com/boykov_artem'><img src='https://cdn-icons-png.flaticon.com/512/2111/2111463.png' width='25'></a>
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td bgcolor='#f4f4f4' align='center' style='padding: 30px 10px 0px 10px;'>
                                        <table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px;'>
                                            <tr>
                                                <td bgcolor='#000000' align='center' style='padding: 30px 30px 30px 30px; border-radius: 4px 4px 4px 4px; color: #fff; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'>
                                                    <h2 style='font-size: 20px; font-weight: 400; color: #fff; margin: 0;'>Наші контакти</h2>
                                                    <p style='margin: 0;'><a href='tel:+380685034088' target='_blank' style='color: #fff;'>Тел: +380-68-503-40-88</a></p>
                                                        <p style='margin: 0;'><a href='mailto:barbershop.oasis@ukr.net' target='_blank' style='color: #fff;'>Email: barbershop.oasis@ukr.net</a></p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                            </body>
                            </html>";


            _emailSender.SendEmailAsync(appointmentVM.Appointment.Email, subject, messageBody);

            _db.SaveChanges();


            return RedirectToAction("Index", "Home");
        }

        public JsonResult TestDate (DateTime date)
        {
            var i = date;

            return Json(new { success = true, i });
        }


        [HttpPost]
        public JsonResult GetAvaliableDate(int barberId = 0)
        {
            var barberSchedules = new List<BarberSchedule>();

            var barbersIds = new List<int>();
            var barbersDates = new List<DateTime>();
            var barbersNames = new List<string>();

            if(barberId == 0)
            {
                barberSchedules = _db.BarberSchedule.Include(s => s.Barber).OrderBy(s=> s.BarberId).ThenBy(s => s.Date).ToList();
                if (barberSchedules.Count > 0)
                {
                    foreach(var barberSchedule in barberSchedules)
                    {
                        barbersIds.Add(barberSchedule.BarberId);
                        barbersDates.Add(barberSchedule.Date.Date);
                        barbersNames.Add(barberSchedule.Barber.FullName);
                    }
                    return Json(new { success = true, barbersIds, barbersDates, barbersNames });
                }
                else
                {
                    return Json(new { success = false, message = "На жаль, зараз немає вільних дат для запису" });
                }
            }

            if(barberId != 0)
            {
                barberSchedules = _db.BarberSchedule.Include(s => s.Barber).Where(s => s.BarberId == barberId).OrderBy(s => s.Date).ToList();
                if (barberSchedules.Count > 0)
                {
                    foreach (var barberSchedule in barberSchedules)
                    {
                        barbersIds.Add(barberSchedule.BarberId);
                        barbersDates.Add(barberSchedule.Date.Date);
                        barbersNames.Add(barberSchedule.Barber.FullName);
                    }

                    return Json(new { success = true, barbersIds, barbersDates, barbersNames });
                }
                else
                {
                    return Json(new { success = false, message = "На жаль, зараз немає вільних дат для запис до цього барбера" });
                }
            }

            else
            {
                return Json(new { success = false, message = "Помилка на сервері" });
            }

            
        }

        [HttpPost]
        public JsonResult GetAvailableTime(int barberId, DateTime date)
        {
            BarberSchedule schedule = _db.BarberSchedule.FirstOrDefault(s => s.BarberId == barberId && s.Date == date);

            if (schedule == null)
            {
                return Json(new { success = false, message = "Обраний барбер в цей день не працює" });
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
                foreach (var time in availableTimes)
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
            var barber = _db.Barbers.Include(b => b.WorkPosition).FirstOrDefault(b => b.Id == barberId);

            if (barber == null)
            {
                return Json(new { success = false, message = "Такого барбера не існує" });
            }

            string specialization;

            switch (barber.WorkPosition.PositionName)
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
            var service = _db.Services.FirstOrDefault(s => s.Id == serviceId);

            if (service != null)
            {
                double price = 0;
                switch (specialization)
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


                var data = new { success = true, price, duration };


                return Json(data);
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
                switch (specialization)
                {
                    case "Стажер":
                        totalPrice = services.Sum(s => s.traineePrice);
                        break;
                    case "Барбер":
                        totalPrice = services.Sum(s => s.barberPrice);
                        break;
                    case "Топ-барбер":
                        totalPrice = services.Sum(s => s.seniorPrice);
                        break;

                }

                TimeSpan totalDuration = TimeSpan.Zero;

                foreach (var service in services)
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

        public JsonResult SetStartTime(TimeSpan startTime, DateTime day)
        {

            if (!(startTime < day.TimeOfDay.Add(TimeSpan.FromMinutes(15))))
            {
                return Json(new { success = true, startTime });
            }
            else
            {
                return Json(new { success = false, message = "Не обрано час для запису!" });
            }
        }
        public JsonResult SetEndTime(TimeSpan startTime, int barberId, DateTime scheduleDate, List<int> selectedServices)
        {
            BarberSchedule? barber = _db.BarberSchedule.FirstOrDefault(b => b.BarberId == barberId && b.Date == scheduleDate);

            var services = _db.Services.Where(s => selectedServices.Contains(s.Id)).ToList();

            TimeSpan addTime = TimeSpan.Zero;

            if (barber != null)
            {

                foreach (var s in services)
                {
                    addTime += s.Duration;
                }

                List<Appointment> appointments = _db.Appointments
               .Where(a => a.BarberId == barberId && a.Date == scheduleDate)
               .OrderBy(a => a.StartTime)
               .ToList();

                TimeSpan closestStartTime = TimeSpan.Zero;
                TimeSpan timeInterval = TimeSpan.Zero;
                bool isAppointmentFound = false;

                foreach (var appointment in appointments)
                {
                    if (appointment.StartTime >= startTime && appointment.StartTime < barber.EndTime)
                    {
                        closestStartTime = appointment.StartTime;
                        isAppointmentFound = true;
                        break;
                    }
                }
                if(isAppointmentFound)
                {
                    timeInterval = closestStartTime - startTime;
                }

                if(!isAppointmentFound)
                {
                    timeInterval = barber.EndTime - startTime;
                }

                if (timeInterval >= addTime)
                {
                    var endTime = startTime.Add(addTime);
                    return Json(new { success = true, endTime });
                }
                else
                {
                    return Json(new { success = false, message = "Вказаний Вами часовий проміжок вже зайнятий" });
                }
            }
            else
            {
                return Json(new { success = false, message = "Помилка на сервері" });
            }

        }

    }
}