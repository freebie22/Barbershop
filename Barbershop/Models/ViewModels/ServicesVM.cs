using Microsoft.AspNetCore.Mvc.Rendering;

namespace Barbershop.Models.ViewModels
{
    public class ServicesVM
    {
        public Services Service { get; set; }
        public IEnumerable<SelectListItem> Specializaions { get; set; }
    }
}
