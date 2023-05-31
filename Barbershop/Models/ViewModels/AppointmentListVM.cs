using Microsoft.AspNetCore.Mvc.Rendering;

namespace Barbershop.Models.ViewModels
{
    public class AppointmentListVM
    {
        public IEnumerable<Appointment> AppointmentList { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }
        public IEnumerable<SelectListItem> AppointmentType { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
    }
}
