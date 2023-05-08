using Microsoft.AspNetCore.Mvc.Rendering;

namespace Barbershop.Models.ViewModels
{
    public class BarberScheduleVM
    {
        public BarberSchedule BarberSchedule { get; set; }
        public IEnumerable<SelectListItem> BarbershopUserSelectList { get; set; }   
    }
}
