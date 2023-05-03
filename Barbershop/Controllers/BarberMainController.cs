using Barbershop.Data;
using Barbershop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barbershop.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class BarberMainController : Controller
    {
        private readonly ApplicationDbContext _db;

        public BarberMainController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var barbersWithRole = (from userRole in _db.UserRoles
                                   join user in _db.Users on userRole.UserId equals user.Id
                                   join role in _db.Roles on userRole.RoleId equals role.Id
                                   where role.Name == "Barber"
                                   select new BarbershopUser
                                   {
                                       Id = user.Id,
                                       UserName = user.UserName,
                                       PhoneNumber = user.PhoneNumber,
                                       Email = user.Email,
                                       EmailConfirmed = user.EmailConfirmed,
                                   }).ToList();

            return View(barbersWithRole);
        }
    }
}
