using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Barbershop.Data;
using Barbershop.Models;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace Barbershop.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class WorkPositionsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public WorkPositionsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: WorkPositions
        public async Task<IActionResult> Index()
        {
             IEnumerable<WorkPositions> objList = _db.WorkPositions;
            return View(objList);
        }


        // GET: WorkPositions/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkPositions obj)
        {
            if (ModelState.IsValid)
            {
                await Task.Run(() =>
                {
                    _db.WorkPositions.AddAsync(obj);
                    obj.StandardPay = 10;
                    _db.SaveChangesAsync();

                });
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _db.WorkPositions == null)
            {
                return NotFound();
            }

            var obj = await _db.WorkPositions.FindAsync(id);
            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(WorkPositions obj)
        {
            if (ModelState.IsValid)
            {
                await Task.Run(() =>
                {
                    _db.WorkPositions.Update(obj);
                    _db.SaveChangesAsync();

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

            var obj = _db.WorkPositions.Find(id);

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
            var obj = _db.WorkPositions.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.WorkPositions.Remove(obj);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
