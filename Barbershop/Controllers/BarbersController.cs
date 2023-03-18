using Barbershop.Data;
using Barbershop.Models;
using Barbershop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Barbershop.Controllers
{
    public class BarbersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BarbersController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<Barbers> objList = _db.Barbers.Include(b => b.WorkPosition).Include(b => b.Specializations);
            return View(objList);
        }

        //GET - Create
        public IActionResult Create(int? id)
        {
            BarbersVM barberVM = new BarbersVM()
            {
                Barber = new Barbers(),
                WorkPostionsSelectList = _db.WorkPositions.Select(i => new SelectListItem
                {
                    Text = i.PositionName,
                    Value = i.Id.ToString()
                }
                ),
                Specializations = _db.Specializations.ToList()
            };
            return View(barberVM);
        }

        //POST - Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BarbersVM barberVM)
        {
            var files = HttpContext.Request.Form.Files;
            string webRootPath = _webHostEnvironment.WebRootPath;

            //create
            string upload = webRootPath + WC.BarberPath;
            string fileName = Guid.NewGuid().ToString();
            string extension = Path.GetExtension(files[0].FileName);

            using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
            {
                files[0].CopyTo(fileStream);
            }

            barberVM.Barber.BarberImage = fileName + extension;

            barberVM.Barber.Specializations = new List<Specializations>();

            foreach (var specializationId in barberVM.SpecializationIds)
            {
                var specialization = _db.Specializations.Find(specializationId);
                if (specialization != null)
                {
                    barberVM.Barber.Specializations.Add(specialization);
                }
            }

            _db.Barbers.Add(barberVM.Barber);

            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
