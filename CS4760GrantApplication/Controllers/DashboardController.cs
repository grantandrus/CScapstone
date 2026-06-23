using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Models;
using CS4760GrantApplication.ViewModels;
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
            var isDeptChair = HttpContext.Session.GetString("IsDeptChair") == "True";

            var viewModel = new DashboardViewModel
            {
                IsDeptChair = isDeptChair,
                MyGrants = await _context.Grants
                .Include(g => g.Departments)
                .Include(g => g.College)
                .Where(g => g.UserId == HttpContext.Session.GetInt32("UserID"))
                .ToListAsync()
            };

            if (isDeptChair)
            {
                var user = await _context.Users.FindAsync(HttpContext.Session.GetInt32("UserID"));

                if (user?.DepartmentId != null)
                {
                    viewModel.DepartmentGrants = await _context.Grants
                        .Include(g => g.User)
                        .Include(g => g.College)
                        .Where(g => !g.IsSaved && g.User != null && g.User.DepartmentId == user.DepartmentId)
                        .ToListAsync();
                }
            }
            return View(viewModel);
        }
    }
}
