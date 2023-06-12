using Barbershop.Data;
using Barbershop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Barbershop.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public EmployeesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            EmployeesVM employeesVM = new EmployeesVM()
            {
                Barbers = _db.Barbers.Include(u => u.WorkPosition).Include(u => u.Specializations).Where(u => u.IsActive == true),
                WorkPositions = _db.WorkPositions
            };
            return View(employeesVM);
        }

        public IActionResult Details(int id)
        {
            DetailsVM DetailsVM = new DetailsVM()
            {
                Barbers = _db.Barbers.Include(u => u.WorkPosition).Include(u => u.Specializations).Where(u => u.Id == id).FirstOrDefault()
            };

            if(DetailsVM.Barbers == null)
            {
                TempData[WC.Error] = "Такого барбера не існує";
                return RedirectToAction("Index");
            }

            if (DetailsVM.Barbers.IsActive == false)
            {
                TempData[WC.Info] = "Цей барбер на даний момент не працює.";
                return RedirectToAction("Index");
            }

            else
            {
                return View(DetailsVM);
            }

        }
    }
}
