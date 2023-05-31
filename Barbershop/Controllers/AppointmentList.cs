using Barbershop.Data;
using Barbershop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Barbershop.Controllers
{
    public class AppointmentList : Controller
    {
        private readonly ApplicationDbContext _db;

        public AppointmentList(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(DateTime? searchDate, DateTime? searchAppointmentDate, string searchEmail = null, string searchPhone = null, string Status = null, string Type = null)
        {
            AppointmentListVM appointmentListVM = new AppointmentListVM()
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
    }
}
