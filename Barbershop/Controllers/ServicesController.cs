using Barbershop.Data;
using Barbershop.Models;
using Microsoft.AspNetCore.Mvc;

namespace Barbershop.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ServicesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<Services> objList = _db.Services;
            return View(objList);
        }

        //GET - Create
        public IActionResult Create()
        {
            return View();
        }

        //POST - Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Services obj)
        {
            if (ModelState.IsValid)
            {
                await Task.Run(() =>
                {
                    _db.Services.AddAsync(obj);
                    _db.SaveChangesAsync();

                });
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //GET - Edit
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var obj = _db.Services.Find(id);

            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }

        //POST - Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Services obj)
        {
            if (ModelState.IsValid)
            {
                await Task.Run(() =>
                {
                    _db.Services.Update(obj);
                    _db.SaveChanges();

                });
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //GET - Delete
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var obj = _db.Services.Find(id);

            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }

        //POST - Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
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
