using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Models;
using CS4760GrantApplication.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            return View(await _context.Departments.Include(d => d.Chair)
                .Include(d => d.College)
                .ToListAsync());
        }

        // Get: Departments/Create
        [SessionAuthorize("admin")]
        public IActionResult Create()
        {
            var users = _context.Users.Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName }).ToList();
            ViewData["Chairs"] = new SelectList(users, "Id", "FullName");
            ViewData["Colleges"] = new SelectList(_context.Colleges.ToList(), "Id", "Name");
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize("admin")]
        public async Task<IActionResult> Create(DepartmentCreateViewModel newDepartment)
        {
            Department department = new Department();

            if (ModelState.IsValid)
            {
                department.DepartmentName = newDepartment.DepartmentName;
                department.Description = newDepartment.Description;

                // if a chair was selected, find that user and assign them to the department
                if (newDepartment.ChairId.HasValue)
                {
                    department.Chair = await _context.Users.FindAsync(newDepartment.ChairId.Value);
                }

                if (newDepartment.CollegeId != 0)
                {
                    department.CollegeId = newDepartment.CollegeId;
                }

                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            var users = _context.Users.Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName }).ToList();
            ViewData["Chairs"] = new SelectList(users, "Id", "FullName", newDepartment.ChairId);
            ViewData["Colleges"] = new SelectList(_context.Colleges.ToList(), "Id", "Name", newDepartment.CollegeId);
            return View(newDepartment);
        }

        // GET: Departments/Edit/5
        [HttpGet]
        [SessionAuthorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var department = await _context.Departments.FindAsync(id);

            if (department == null)
                return NotFound();

            var users = _context.Users.Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName }).ToList();
            ViewData["Chairs"] = new SelectList(users, "Id", "FullName", department.ChairId);
            ViewData["Colleges"] = new SelectList(_context.Colleges.ToList(), "Id", "Name", department.CollegeId);

            var viewModel = new DepartmentCreateViewModel
            {
                Id = department.Id,
                DepartmentName = department.DepartmentName,
                Description = department.Description,
                ChairId = department.ChairId,
                CollegeId = department.CollegeId,
            };

            return View(viewModel);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [SessionAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DepartmentCreateViewModel viewModel)
        {
            var department = await _context.Departments.FindAsync(viewModel.Id);

            if (department == null) return NotFound();

            department.DepartmentName = viewModel.DepartmentName;
            department.Description = viewModel.Description;
            department.ChairId = viewModel.ChairId;
            department.CollegeId = viewModel.CollegeId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
