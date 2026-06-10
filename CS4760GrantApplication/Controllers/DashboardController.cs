using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CS4760GrantApplication.Controllers
{
    public class DashboardController : Controller
    {
        private readonly CS4760GrantApplicationContext _context;

        public DashboardController(CS4760GrantApplicationContext context)
        {
            _context = context;
        }

        [SessionAuthorize]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Grants
                .Include(g => g.Departments)
                .Include(g => g.College)
                .Where(g => g.UserId == HttpContext.Session.GetInt32("UserID"))
                .ToListAsync());
        }
    }
}
