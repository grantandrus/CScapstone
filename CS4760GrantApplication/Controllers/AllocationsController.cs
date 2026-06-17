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

        [SessionAuthorize]
        [HttpGet]
        public async Task<IActionResult> Index()
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
        public async Task<IActionResult> Index(Allocation allocation)
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