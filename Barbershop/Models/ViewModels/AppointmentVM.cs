using Microsoft.AspNetCore.Mvc.Rendering;

namespace Barbershop.Models.ViewModels
{
    public class AppointmentVM
    {
        public Appointment Appointment { get; set; }
        public BarbershopUser User { get; set; }
        public IEnumerable<SelectListItem> Barbers { get; set; }
        public List<Barbers> BarberDetails { get; set; } 
        public List<Services> Services { get; set; }
        public List<int>  servicesIds { get; set; }
    }
}
