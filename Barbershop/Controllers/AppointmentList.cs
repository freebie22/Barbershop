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

        public IActionResult Index(DateTime? searchDate, DateTime? searchAppointmentDate, string searchEmail = null, string searchPhone = null, string Status = null, string Type = null)
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

        [HttpPost]
        public IActionResult AppointmentDone(int id)
        {
            Appointment appointment = _db.Appointments.FirstOrDefault(u => u.Id == id);
            appointment.AppointmentStatus = WC.AppointmentDone;
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
    }
}
