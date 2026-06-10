using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Models;
using CS4760GrantApplication.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CS4760GrantApplication.Controllers
{
    public class ReviewController : Controller
    {
        private readonly CS4760GrantApplicationContext _context;

        public ReviewController(CS4760GrantApplicationContext context)
        {
            _context = context;
        }

        public List<Grant> Grants = new();

        [SessionAuthorize]
        public async Task<IActionResult> Index()
        {

            Grants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.User)
                .Where(g => !g.IsSaved)
                .ToListAsync();

            return View(Grants);
        }
    }
}
