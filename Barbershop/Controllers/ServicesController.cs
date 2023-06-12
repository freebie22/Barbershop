using Barbershop.Data;
using Barbershop.Models;
using Barbershop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Barbershop.Controllers
{
    [Authorize]
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ServicesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(string? searchName)
        {

                IEnumerable<Services> objList = _db.Services;
                if (!string.IsNullOrEmpty(searchName))
                {
                    objList = _db.Services.Where(o => o.serviceName.ToLower().Contains(searchName));
                }

                return View(objList);          
        }

        //GET - Create
        public IActionResult Create()
        {
            ServicesVM serviceVM = new ServicesVM() {
                Service = new Services(),
                Specializaions = _db.Specializations.Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                {
                    Text = s.SpecName,
                    Value = s.Id.ToString()
                })
            };
            return View(serviceVM);
        }

        //POST - Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServicesVM serviceVM)
        {
            var durationInMinutes = serviceVM.Service.Duration.Hours * 60 + serviceVM.Service.Duration.Minutes;
            if (ModelState.IsValid)
            {
                await Task.Run(() =>
                {
                    serviceVM.Service.Duration = TimeSpan.FromMinutes(durationInMinutes);
                    _db.Services.AddAsync(serviceVM.Service);
                    _db.SaveChangesAsync();

                });
                return RedirectToAction("Index");
            }
            return View(serviceVM);
        }

        //GET - Edit
        public IActionResult Edit(int? id)
        {
            ServicesVM serviceVM = new ServicesVM()
            {
                Service = _db.Services.FirstOrDefault(s => s.Id == id),
                Specializaions = _db.Specializations.Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                {
                    Text = s.SpecName,
                    Value = s.Id.ToString()
                })
            };

            if(serviceVM.Service == null)
            {
                return RedirectToAction("Index");
            }

            else
            {
                return View(serviceVM);
            }


        }

        //POST - Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ServicesVM serviceVM)
        {
            var durationInMinutes = serviceVM.Service.Duration.Hours * 60 + serviceVM.Service.Duration.Minutes;

            serviceVM.Service.Duration = TimeSpan.FromMinutes(durationInMinutes);
            _db.Services.Update(serviceVM.Service);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        //GET - Delete
        public IActionResult Delete(int? id)
        {
            ServicesVM serviceVM = new ServicesVM()
            {
                Service = _db.Services.FirstOrDefault(s => s.Id == id),
                Specializaions = _db.Specializations.Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                {
                    Text = s.SpecName,
                    Value = s.Id.ToString()
                })
            };

            if(serviceVM.Service == null)
            {
                return RedirectToAction("Index");
            }

            else
            {
                return View(serviceVM);
            }
        }

        //POST - Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.Services.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Services.Remove(obj);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
