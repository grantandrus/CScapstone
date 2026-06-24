using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Models;
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
            Grants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.User)
                .Include(g => g.Reviews)
                .Where(g => !g.IsSaved)
                .ToListAsync();

            return View(Grants);

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
        public async Task<IActionResult> Create(Allocation allocation)
        {
            if (!ModelState.IsValid)
            {
                return View(allocation);
            }

            var existingAllocation = await _context.Allocations.FirstOrDefaultAsync();

            if (existingAllocation == null)
            {
                _context.Allocations.Add(allocation);
            }
            else
            {
                existingAllocation.AvailableAmount = allocation.AvailableAmount;
                existingAllocation.RolloverAmount = allocation.RolloverAmount;
                existingAllocation.CutoffPercent = allocation.CutoffPercent;
            }

            await _context.SaveChangesAsync();

            ViewBag.Message = "Allocations saved successfully.";
            return View(await _context.Allocations.FirstOrDefaultAsync());
        }
    }
}