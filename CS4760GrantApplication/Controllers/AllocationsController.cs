using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Models;
using CS4760GrantApplication.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CS4760GrantApplication.Controllers
{
    public class AllocationsController : Controller
    {
        private readonly CS4760GrantApplicationContext _context;

        public AllocationsController(CS4760GrantApplicationContext context)
        {
            _context = context;
        }

        public List<Grant> Grants = new();

        [SessionAuthorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var grants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.User)
                .Include(g => g.Reviews)
                .Where(g => !g.IsSaved && g.Reviews.Count() != 0)
                .ToListAsync();

            var allocation = await _context.Allocations.FirstOrDefaultAsync()
                     ?? new Allocation();

            var vm = new AllocationsViewModel
            {
                Grants = grants,
                Allocation = allocation
            };

            return View(vm);

        }

        [SessionAuthorize]
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var grant = await _context.Grants
                .Include(g => g.User)
                .Include(g => g.College)
                .Include(g => g.Departments)
                .Include(g => g.Attachments)
                .Include(g => g.BudgetItems)
                .Include(g => g.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grant == null)
                return NotFound();

            return View(grant);
        }

        [SessionAuthorize]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var allocation = await _context.Allocations.FirstOrDefaultAsync();

            if (allocation == null)
            {
                allocation = new Allocation();
            }

            return View(allocation);
        }

        [SessionAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AllocationsViewModel vm)
        {
            var allocation = vm.Allocation;

            if (!ModelState.IsValid)
            {
                vm.Grants = await _context.Grants
                    .Include(g => g.BudgetItems)
                    .Include(g => g.User)
                    .Include(g => g.Reviews)
                    .Where(g => !g.IsSaved)
                    .ToListAsync();

                return View("Index", vm);
            }

            var existing = await _context.Allocations.FirstOrDefaultAsync();

            if (existing == null)
            {
                _context.Allocations.Add(allocation);
            }
            else
            {
                existing.AvailableAmount = allocation.AvailableAmount;
                existing.RolloverAmount = allocation.RolloverAmount;
                existing.CutoffPercent = allocation.CutoffPercent;
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Allocations saved successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}