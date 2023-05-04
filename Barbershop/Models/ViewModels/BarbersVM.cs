using Microsoft.AspNetCore.Mvc.Rendering;

namespace Barbershop.Models.ViewModels
{
    public class BarbersVM
    {
        public Barbers Barber { get; set; } 
        public IEnumerable<SelectListItem>? WorkPostionsSelectList { get; set; }
        public List<Specializations> Specializations { get; set; }
        public List<int> SpecializationIds { get; set; }

    }
}
