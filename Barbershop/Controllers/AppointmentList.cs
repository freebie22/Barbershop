using Barbershop.Data;
using Barbershop.Models;
using Barbershop.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Barbershop.Controllers
{
    public class AppointmentList : Controller
    {
        private readonly ApplicationDbContext _db;

        public AppointmentList(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public AppointmentDetailsVM AppointmentVM { get; set; }

        public IActionResult Index(int? searchId, DateTime? searchDate, DateTime? searchAppointmentDate, string searchEmail = null, string searchPhone = null, string Status = null, string Type = null)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            AppointmentListVM appointmentListVM = new AppointmentListVM();
            if (User.IsInRole(WC.AdminRole))
            {
                appointmentListVM = new AppointmentListVM()
                {
                    AppointmentList = _db.Appointments.Include(a => a.Barber),
                    StatusList = WC.listAppointmentStatus.ToList().Select(i => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = i,
                        Value = i
                    }),
                    AppointmentType = WC.listAppointmentType.ToList().Select(i => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = i,
                        Value = i
                    })
                };
                if (searchId > 0)
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(a => a.Id == searchId);
                }
                if (searchDate.HasValue)
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(a => a.Date == searchDate);
                }
                if (searchAppointmentDate.HasValue)
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(a => a.AppointmentDateAndTime == searchAppointmentDate);
                }
                if (!string.IsNullOrEmpty(searchEmail))
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(a => a.Email.ToLower().Contains(searchEmail.ToLower()));
                }
                if (!string.IsNullOrEmpty(searchPhone))
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(a => a.PhoneNumber.ToLower().Contains(searchPhone.ToLower()));
                }
                if (!string.IsNullOrEmpty(Status) && Status != "--Статус запису--")
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(u => u.AppointmentStatus.ToLower().Contains(Status.ToLower()));
                }
                if (!string.IsNullOrEmpty(Type) && Type != "--Тип запису--")
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(u => u.AppointmentType.ToLower().Contains(Type.ToLower()));
                }

                return View(appointmentListVM);
            }
            
            if(User.IsInRole(WC.ClientRole))
            {
                appointmentListVM = new AppointmentListVM()
                {
                    AppointmentList = _db.Appointments.Include(a => a.Barber).Where(a => a.UserId == claim.Value),
                    StatusList = WC.listAppointmentStatus.ToList().Select(i => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = i,
                        Value = i
                    }),
                    AppointmentType = WC.listAppointmentType.ToList().Select(i => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = i,
                        Value = i
                    })
                };
                if (searchId > 0)
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(a => a.Id == searchId);
                }
                if (searchDate.HasValue)
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(a => a.Date == searchDate);
                }
                if (searchAppointmentDate.HasValue)
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(a => a.AppointmentDateAndTime == searchAppointmentDate);
                }
                if (!string.IsNullOrEmpty(searchEmail))
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(a => a.Email.ToLower().Contains(searchEmail.ToLower()));
                }
                if (!string.IsNullOrEmpty(searchPhone))
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(a => a.PhoneNumber.ToLower().Contains(searchPhone.ToLower()));
                }
                if (!string.IsNullOrEmpty(Status) && Status != "--Статус запису--")
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(u => u.AppointmentStatus.ToLower().Contains(Status.ToLower()));
                }
                if (!string.IsNullOrEmpty(Type) && Type != "--Тип запису--")
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(u => u.AppointmentType.ToLower().Contains(Type.ToLower()));
                }

                return View(appointmentListVM);
            }

            if(User.IsInRole(WC.BarberRole))
            {
                appointmentListVM = new AppointmentListVM()
                {
                    AppointmentList = _db.Appointments.Include(a => a.Barber).Where(a => a.Barber.BarbershopUserId == claim.Value),
                    StatusList = WC.listAppointmentStatus.ToList().Select(i => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = i,
                        Value = i
                    }),
                    AppointmentType = WC.listAppointmentType.ToList().Select(i => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = i,
                        Value = i
                    })
                };
                if (searchId > 0)
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(a => a.Id == searchId);
                }
                if (searchDate.HasValue)
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(a => a.Date == searchDate);
                }
                if (searchAppointmentDate.HasValue)
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(a => a.AppointmentDateAndTime == searchAppointmentDate);
                }
                if (!string.IsNullOrEmpty(searchEmail))
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(a => a.Email.ToLower().Contains(searchEmail.ToLower()));
                }
                if (!string.IsNullOrEmpty(searchPhone))
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(a => a.PhoneNumber.ToLower().Contains(searchPhone.ToLower()));
                }
                if (!string.IsNullOrEmpty(Status) && Status != "--Статус запису--")
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(u => u.AppointmentStatus.ToLower().Contains(Status.ToLower()));
                }
                if (!string.IsNullOrEmpty(Type) && Type != "--Тип запису--")
                {
                    appointmentListVM.AppointmentList = appointmentListVM.AppointmentList.Where(u => u.AppointmentType.ToLower().Contains(Type.ToLower()));
                }

                return View(appointmentListVM);
            }

            else
            {
                return RedirectToAction("Index", "Home");
            }

            
        }

        public IActionResult Details(int id)
        {
            AppointmentVM = new AppointmentDetailsVM()
            {
                Appointment = _db.Appointments.Include(a => a.Barber).Include(a => a.User).Include(a => a.Barber.WorkPosition).Include(a => a.Services).FirstOrDefault(a => a.Id == id),
            };
            if(AppointmentVM.Appointment == null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(AppointmentVM.Appointment);
            }
        }

        private void SetUserAppCount(string userId)
        {
            var user = _db.Users.FirstOrDefault(u => u.Id == userId);
            var barbershopUser = (BarbershopUser)user;

            barbershopUser.AppointmentSuccessCount++;
            barbershopUser.AppointmentPoints++;
            _db.Users.Update(barbershopUser);
            _db.SaveChanges();
        }

        [HttpPost]
        public IActionResult AppointmentDone(int id)
        {
            Appointment appointment = _db.Appointments.FirstOrDefault(u => u.Id == id);
            appointment.AppointmentStatus = WC.AppointmentDone;
            SetUserAppCount(appointment.UserId);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult AppointmentCancelled(int id)
        {
            Appointment appointment = _db.Appointments.FirstOrDefault(u => u.Id == id);
            appointment.AppointmentStatus = WC.AppointmentCancelled;
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public JsonResult LeaveReview(int barberId, int Rate, string? comment)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var review = new Review();

            review.LeftByUserId = claim.Value;
            review.BarberId = barberId;
            review.Rate = Rate;
            review.Review_Comment = comment;
            review.CreatedAt = DateTime.Now;

            _db.Reviews.Add(review);
            _db.SaveChanges();

            return Json(new { success = true });

        }

        [HttpPost]
        public IActionResult ConvertToPromo()
        {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                var user = _db.Users.FirstOrDefault(u => u.Id == claim.Value);
                var barbershopUser = (BarbershopUser)user;

                if(barbershopUser.AppointmentPoints < 3)
                {
                    return RedirectToAction("Index");
                }

                if (barbershopUser.LastPromoCodeGeneratedDate != DateTime.MinValue &&
                    (DateTime.Now - barbershopUser.LastPromoCodeGeneratedDate).TotalDays < 5)
                {
                    TempData[WC.Warning] = "Увага! З минулої генерації промокоду ще не минуло 5 днів!";
                    return RedirectToAction("Index");
                }


                var PromoCode = new PromoCodes();

                PromoCode.IsActive = true;
                PromoCode.Code = Guid.NewGuid().ToString();
                PromoCode.UserId = claim.Value;
                PromoCode.Discount = 15;
                PromoCode.Type = WC.PromoAppointment;


                //barbershopUser.AppointmentPoints -= 3;
                barbershopUser.LastPromoCodeGeneratedDate = DateTime.Now;

                _db.PromoCodes.Add(PromoCode);
                _db.Users.Update(barbershopUser);
                _db.SaveChanges();

            TempData[WC.Success] = "Вітаємо! Переглянути свої активні промокоди ви можете натиснувши на 'Мої промокоди'.";
            TempData[WC.Info] = "Зверніть увагу, що цей промокод є дійсним лише для Вашого облікового запису.";

            return RedirectToAction("Index");                   
        }

        public JsonResult GetMyPromos()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var promos = _db.PromoCodes.Where(p => p.UserId == claim.Value).Select(p => p.Code);

            if(promos.Count() > 0)
            {
                return Json(new {success = true, promos});
            }
            else
            {
                return Json(new { success = false, message = "На жаль, у Вас ще немає промокодів." });
            }
        }
    }
}
