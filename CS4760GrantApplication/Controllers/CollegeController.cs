using CS4760GrantApplication.Data;
using CS4760GrantApplication.Models;
using CS4760GrantApplication.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CS4760GrantApplication.Controllers
{
    public class CollegeController : Controller
    {
        private readonly CS4760GrantApplicationContext _context;

        public CollegeController(CS4760GrantApplicationContext context)
        {
            _context = context;
        }

        // GET: College/Add
        public IActionResult Add()
        {
            var users = _context.Users.Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName }).ToList();
            ViewData["Deans"] = new SelectList(users, "Id", "FullName");
            return View();
        }

        // POST: College/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CollegeAddViewModel newCollege)
        {
            College college = new College();

            if (ModelState.IsValid)
            {
                // if a dean was selected, find that user and assign them to the college
                if (newCollege.DeanId.HasValue)
                {
                    college.Dean = await _context.Users.FindAsync(newCollege.DeanId.Value);
                }
                college.Name = newCollege.Name;

                _context.Add(college);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            var users = _context.Users.Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName }).ToList();
            ViewData["Deans"] = new SelectList(users, "Id", "FullName", newCollege.DeanId);
            return View(college);
        }
    }
}
