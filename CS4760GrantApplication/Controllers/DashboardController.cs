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
            var userId = HttpContext.Session.GetInt32("UserID");
            var isDeptChair = HttpContext.Session.GetString("IsDeptChair") == "True";
            var isCollegeDean = HttpContext.Session.GetString("IsCollegeDean") == "True";

            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            await GenerateGrantReportNotifications(userId.Value);

            var viewModel = new DashboardViewModel
            {
                IsDeptChair = isDeptChair,
                IsCollegeDean = isCollegeDean,

                MyGrants = await _context.Grants
                    .Include(g => g.Departments)
                    .Include(g => g.College)
                    .Include(g => g.BudgetItems)
                    .Where(g => g.UserId == userId)
                    .ToListAsync(),

                Notifications = await _context.Notifications
                    .Include(n => n.Grant)
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .OrderByDescending(n => n.NotificationDate)
                    .ToListAsync()
            };

            if (isDeptChair)
            {
                var user = await _context.Users.FindAsync(userId);

                if (user?.DepartmentId != null)
                {
                    viewModel.DepartmentGrants = await _context.Grants
                        .Include(g => g.User)
                        .Include(g => g.College)
                        .Where(g => !g.IsSaved && g.User != null && g.User.DepartmentId == user.DepartmentId)
                        .ToListAsync();
                }
            }

            if (isCollegeDean)
            {
                var user = await _context.Users.FindAsync(userId);

                if (user?.CollegeId != null)
                {
                    viewModel.CollegeGrants = await _context.Grants
                        .Include(g => g.User)
                        .Include(g => g.College)
                        .Include(g => g.BudgetItems)
                        .Where(g => !g.IsSaved && g.User != null && g.CollegeId == user.CollegeId && g.BudgetItems.Any(b => b.FundingSource == "College"))
                        .ToListAsync();
                }
            }

            return View(viewModel);
        }

        private async Task GenerateGrantReportNotifications(int userId)
        {
            var today = DateTime.Today;

            var acceptedGrants = await _context.Grants
                .Where(g => g.UserId == userId && g.Statuses.Contains(GrantStatus.ApprovedARCC))
                .ToListAsync();
    
            foreach (var grant in acceptedGrants)
            {
                if (grant.ReportDueDate == null)
                {
                    grant.ReportDueDate = new DateTime(2026, 6, 30);
                }

                var dueDate = grant.ReportDueDate.Value.Date;

                if (today >= dueDate.AddDays(-7))
                {
                    await CreateNotificationIfNeeded(
                        userId,
                        grant.Id,
                        grant.Title,
                        dueDate,
                        dueDate.AddDays(-7),
                        "1 week");
                }
                else if (today >= dueDate.AddMonths(-1))
                {
                    await CreateNotificationIfNeeded(
                        userId,
                        grant.Id,
                        grant.Title,
                        dueDate,
                        dueDate.AddMonths(-1),
                        "1 month");
                }
                else if (today >= dueDate.AddMonths(-2))
                {
                    await CreateNotificationIfNeeded(
                        userId,
                        grant.Id,
                        grant.Title,
                        dueDate,
                        dueDate.AddMonths(-2),
                        "2 months");
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task CreateNotificationIfNeeded(
            int userId,
            int grantId,
            string grantTitle,
            DateTime dueDate,
            DateTime notificationDate,
            string timeBeforeDue)
        {
            if (DateTime.Today < notificationDate.Date)
            {
                return;
            }

            var message = $"Grant report for \"{grantTitle}\" is due on {dueDate:MM/dd/yyyy}. This is the {timeBeforeDue} reminder.";

            var alreadyExists = await _context.Notifications.AnyAsync(n =>
                n.UserId == userId &&
                n.GrantId == grantId &&
                n.Message == message);

            if (!alreadyExists)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = userId,
                    GrantId = grantId,
                    Message = message,
                    NotificationDate = DateTime.Now,
                    IsRead = false
                });
            }
        }
    }
}