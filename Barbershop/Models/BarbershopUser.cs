using Microsoft.AspNetCore.Identity;

namespace Barbershop.Models
{
    public class BarbershopUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
