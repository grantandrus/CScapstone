using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CS4760GrantApplication.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly CS4760GrantApplicationContext _context;

        public DepartmentsController(CS4760GrantApplicationContext context)
        {
            _context = context;
        }

        // GET: Departments
        [SessionAuthorize("admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Departments.ToListAsync());
        }

        // Get: Departments/Create
        [SessionAuthorize("admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize("admin")]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(department);
        }
    }
}
