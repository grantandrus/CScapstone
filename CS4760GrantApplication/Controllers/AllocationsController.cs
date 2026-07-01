using ClosedXML.Excel;
using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using CS4760GrantApplication.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CS4760GrantApplication.Controllers
{
    public class AllocationsController : Controller
    {
        private readonly CS4760GrantApplicationContext _context;
        public IConfiguration Configuration { get; set; }

        public AllocationsController(CS4760GrantApplicationContext context, IConfiguration _configuration)
        {
            _context = context;
            Configuration = _configuration;
        }

        public List<Grant> Grants = new();

        [SessionAuthorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var acceptedGrants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.User)
                .Include(g => g.Reviews)
                .Where(g => !g.IsSaved && g.Reviews.Count() != 0 && g.Status == GrantStatus.ApprovedARCC)
                .ToListAsync();

            var rejectedGrants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.User)
                .Include(g => g.Reviews)
                .Where(g => !g.IsSaved && g.Reviews.Count() != 0 && g.Status == GrantStatus.RejectedARCC)
                .ToListAsync();

            var undecidedGrants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.User)
                .Include(g => g.Reviews)
                .Where(g => !g.IsSaved && g.Reviews.Count() != 0 && (g.Status != GrantStatus.RejectedARCC && g.Status != GrantStatus.ApprovedARCC))
                .ToListAsync();

            var allocationRules = await _context.AllocationRules
                .OrderByDescending(r => r.MinScore)
                .ToListAsync();

            var allocation = await _context.Allocations.FirstOrDefaultAsync()
                     ?? new Allocation();

            var vm = new AllocationsViewModel
            {
                AcceptedGrants = acceptedGrants,
                RejectedGrants = rejectedGrants,
                UndecidedGrants = undecidedGrants,
                Allocation = allocation,
                Rules = allocationRules
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
                vm.AcceptedGrants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.User)
                .Include(g => g.Reviews)
                .Where(g => !g.IsSaved && g.Reviews.Count() != 0 && g.Status == GrantStatus.ApprovedARCC)
                .ToListAsync();

                vm.RejectedGrants = await _context.Grants
                    .Include(g => g.BudgetItems)
                    .Include(g => g.User)
                    .Include(g => g.Reviews)
                    .Where(g => !g.IsSaved && g.Reviews.Count() != 0 && g.Status == GrantStatus.RejectedARCC)
                    .ToListAsync();

                vm.UndecidedGrants = await _context.Grants
                    .Include(g => g.BudgetItems)
                    .Include(g => g.User)
                    .Include(g => g.Reviews)
                    .Where(g => !g.IsSaved && g.Reviews.Count() != 0 && (g.Status != GrantStatus.RejectedARCC && g.Status != GrantStatus.ApprovedARCC))
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

        [SessionAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyCutoff(AllocationsViewModel vm)
        {
            var allocation = await _context.Allocations.FirstOrDefaultAsync();

            if (allocation == null)
            {
                TempData["Message"] = "Please save the allocation settings first.";
                return RedirectToAction(nameof(Index));
            }

            var undecidedGrants = await _context.Grants
                .Include(g => g.Reviews)
                .Where(g =>
                    !g.IsSaved &&
                    g.Reviews.Any() &&
                    g.Status != GrantStatus.ApprovedARCC &&
                    g.Status != GrantStatus.RejectedARCC)
                .ToListAsync();

            foreach (var grant in undecidedGrants)
            {
                decimal averageScore = grant.Reviews.Average(r => r.AverageScore);

                if (averageScore >= allocation.CutoffPercent)
                {
                    grant.Status = GrantStatus.ApprovedARCC;
                }
                else
                {
                    grant.Status = GrantStatus.RejectedARCC;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Cutoff applied successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRule(AllocationsViewModel vm)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            _context.AllocationRules.Add(vm.NewRule);

            await _context.SaveChangesAsync();

            TempData["Message"] = "Allocation rule added.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRule(int id)
        {
            var rule = await _context.AllocationRules.FindAsync(id);

            if (rule != null)
            {
                _context.AllocationRules.Remove(rule);

                await _context.SaveChangesAsync();
            }

            TempData["Message"] = "Allocation rule removed.";

            return RedirectToAction(nameof(Index));
        }

        [SessionAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyRule(int id)
        {
            var rule = await _context.AllocationRules.FindAsync(id);

            if (rule == null)
            {
                TempData["Message"] = "Allocation rule not found.";
                return RedirectToAction(nameof(Index));
            }

            var grants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.Reviews)
                .Where(g => g.Status == GrantStatus.ApprovedARCC)
                .ToListAsync();

            foreach (var grant in grants)
            {
                if (!grant.Reviews.Any())
                    continue;

                decimal averageScore = grant.Reviews.Average(r => r.AverageScore);

                if (averageScore >= rule.MinScore &&
                    averageScore <= rule.MaxScore)
                {
                    decimal requestedAmount = grant.BudgetItems
                        .Where(b => b.FundingSource == "ARCC")
                        .Sum(b => b.Amount);

                    grant.AllocatedFunds =
                        requestedAmount * rule.PercentAllocated / 100m;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Allocation rule applied.";

            return RedirectToAction(nameof(Index));
        }

        [SessionAuthorize]
        [HttpPost]
        public IActionResult Export()
        {
            using XLWorkbook wb = new();
            DataTable dt = GetGrants().Tables[0];

            var ws = wb.Worksheets.Add(dt);

            ws.Columns("A").AdjustToContents();
            ws.Columns("B").AdjustToContents().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Columns("C").AdjustToContents();

            using MemoryStream stream = new();
            wb.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "GrantAllocations.xlsx");
        }

        private DataSet GetGrants()
        {
            DataSet ds = new();
            var constr = Configuration.GetConnectionString("DefaultConnection");
            string sql = "SELECT Grants.Title AS \"Grant Title\", " +
                "Grants.UserId AS \"PI Account Number\", " +
                "Users.FirstName + ' ' + Users.LastName AS \"PI Name\" " +
                "FROM Grants " +
                "JOIN Users ON Grants.UserId = Users.Id " +
                "ORDER BY Grants.Id;";
            using (SqlConnection con = new(constr))
            {
                using SqlDataAdapter sda = new(sql, con);
                sda.Fill(ds);
            }

            return ds;
        }


    }
}