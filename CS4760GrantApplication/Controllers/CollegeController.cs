using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Models;
using CS4760GrantApplication.ViewModels;
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

        // GET: Colleges
        [SessionAuthorize("admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Colleges.Include(c => c.Dean).ToListAsync());
        }

        // GET: College/Add
        [SessionAuthorize("admin")]
        public IActionResult Add()
        {
            return View();
        }

        // POST: College/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize("admin")]
        public async Task<IActionResult> Add(CollegeAddViewModel newCollege)
        {
            College college = new College();

            if (ModelState.IsValid)
            {
                college.Name = newCollege.Name;

                _context.Add(college);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: College/Edit/5
        [HttpGet]
        [SessionAuthorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var college = await _context.Colleges.FindAsync(id);

            if (college == null)
                return NotFound();

            var users = _context.Users.Where(u => u.CollegeId == id).Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName }).ToList();
            ViewData["Deans"] = new SelectList(users, "Id", "FullName", college.DeanId);

            var viewModel = new CollegeAddViewModel
            {
                Id = college.Id,
                Name = college.Name,
                DeanId = college.DeanId,
            };

            return View(viewModel);
        }

        // POST: College/Edit/5
        [HttpPost]
        [SessionAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CollegeAddViewModel viewModel)
        {
            var college = await _context.Colleges.FindAsync(viewModel.Id);

            if (college == null) return NotFound();

            college.Name = viewModel.Name;
            college.DeanId = viewModel.DeanId;

            var currentDean = await _context.Users
                .FirstOrDefaultAsync(u => u.IsCollegeDean && u.CollegeId == college.Id);
            if (currentDean != null && currentDean.Id != viewModel.DeanId)
            {
                currentDean.IsCollegeDean = false;
            }

            if (viewModel.DeanId.HasValue)
            {
                var newDean = await _context.Users.FindAsync(viewModel.DeanId.Value);
                if (newDean != null)
                {
                    newDean.IsCollegeDean = true;
                    // Redundant but makes sure that user's college is correct
                    newDean.CollegeId = college.Id;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [SessionAuthorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var college = await _context.Colleges.FirstOrDefaultAsync(m => m.Id == id);
            if (college == null) return NotFound();
            return View(college);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var college = await _context.Colleges.FindAsync(id);
            if (college != null)
            {
                var collegeDean = await _context.Users
                    .FirstOrDefaultAsync(u => u.IsCollegeDean && u.CollegeId == id);

                // Make sure College Dean is not the Deam of a deleted College
                if (collegeDean != null)
                {
                    collegeDean.IsCollegeDean = false;
                }

                _context.Colleges.Remove(college);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
