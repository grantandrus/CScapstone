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

        public List<Grant> AllocatedGrants = new();

        [SessionAuthorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var acceptedGrants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.User)
                .Include(g => g.Reviews)
                .Where(g => !g.IsSaved && g.Reviews.Count() != 0 && g.Statuses.Contains(GrantStatus.ApprovedARCC) && !g.IsAllocationCompleted)
                .ToListAsync();

            var rejectedGrants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.User)
                .Include(g => g.Reviews)
                .Where(g => !g.IsSaved && g.Reviews.Count() != 0 && g.Statuses.Contains(GrantStatus.RejectedARCC) && !g.IsAllocationCompleted)
                .ToListAsync();

            var undecidedGrants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.User)
                .Include(g => g.Reviews)
                .Where(g => !g.IsSaved && g.Reviews.Count() != 0 && (!g.Statuses.Contains(GrantStatus.RejectedARCC) && !g.Statuses.Contains(GrantStatus.ApprovedARCC) && !g.IsAllocationCompleted))
                .ToListAsync();

            var allocatedGrants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.User)
                .Include(g => g.Reviews)
                .Where(g => !g.IsSaved && g.Reviews.Count() != 0 && g.IsAllocationCompleted)
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
                AllocatedGrants = allocatedGrants,
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
                .Where(g => !g.IsSaved && g.Reviews.Count() != 0 && g.Statuses.Contains(GrantStatus.ApprovedARCC) && !g.IsAllocationCompleted)
                .ToListAsync();

                vm.RejectedGrants = await _context.Grants
                    .Include(g => g.BudgetItems)
                    .Include(g => g.User)
                    .Include(g => g.Reviews)
                    .Where(g => !g.IsSaved && g.Reviews.Count() != 0 && g.Statuses.Contains(GrantStatus.RejectedARCC) && !g.IsAllocationCompleted)
                    .ToListAsync();

                vm.UndecidedGrants = await _context.Grants
                    .Include(g => g.BudgetItems)
                    .Include(g => g.User)
                    .Include(g => g.Reviews)
                    .Where(g => !g.IsSaved && g.Reviews.Count() != 0 && (!g.Statuses.Contains(GrantStatus.RejectedARCC) && !g.Statuses.Contains(GrantStatus.ApprovedARCC) && !g.IsAllocationCompleted))
                    .ToListAsync();

                vm.AllocatedGrants = await _context.Grants
                    .Include(g => g.BudgetItems)
                    .Include(g => g.User)
                    .Include(g => g.Reviews)
                    .Where(g => (!g.IsSaved) && (g.Reviews.Count() != 0) && (g.IsAllocationCompleted))
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
        [HttpGet]
        public async Task<IActionResult> Notes(int id)
        {
            var grant = await _context.Grants
                .Include(g => g.Reviews)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grant == null)
            {
                return NotFound();
            }

            return View(grant);
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
                    (!g.Statuses.Contains(GrantStatus.RejectedARCC) && !g.Statuses.Contains(GrantStatus.ApprovedARCC) && !g.IsAllocationCompleted))
                .ToListAsync();

            foreach (var grant in undecidedGrants)
            {
                decimal averageScore = grant.Reviews.Average(r => r.AverageScore);

                if (averageScore >= allocation.CutoffPercent)
                {
                    grant.Statuses.Add(GrantStatus.ApprovedARCC);
                }
                else
                {
                    grant.Statuses.Add(GrantStatus.RejectedARCC);
                }
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Cutoff applied successfully.";

            return RedirectToAction(nameof(Index));
        }

        [SessionAuthorize]
        [HttpGet]
        public async Task<IActionResult> PreviewCutoffData()
        {
            var allocation = await _context.Allocations.FirstOrDefaultAsync();
            if (allocation == null)
            {
                return Json(new { error = "Allocation settings not found." });
            }

            var undecidedGrants = await _context.Grants
                .Include(g => g.Reviews)
                .Include(g => g.User)
                .Where(g =>
                    !g.IsSaved &&
                    g.Reviews.Any() &&
                    (!g.Statuses.Contains(GrantStatus.RejectedARCC) && !g.Statuses.Contains(GrantStatus.ApprovedARCC)))
                .ToListAsync();

            var resultGrants = new List<object>();
            int acceptedCount = 0;
            int rejectedCount = 0;

            foreach (var grant in undecidedGrants)
            {
                decimal averageScore = grant.Reviews.Average(r => r.AverageScore);
                bool isAccepted = averageScore >= allocation.CutoffPercent;

                if (isAccepted) acceptedCount++;
                else rejectedCount++;

                resultGrants.Add(new
                {
                    title = grant.Title,
                    principalInvestigator = grant.User.FirstName + " " + grant.User.LastName,
                    averageScore = averageScore,
                    accept = isAccepted
                });
            }

            return Json(new
            {
                cutoff = allocation.CutoffPercent,
                acceptedCount = acceptedCount,
                rejectedCount = rejectedCount,
                affectedCount = resultGrants.Count,
                grants = resultGrants
            });
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
        public async Task<IActionResult> ApplyRules(bool ignoreOverage = false)
        {
            var rules = await _context.AllocationRules
                .OrderByDescending(r => r.MinScore)
                .ToListAsync();

            if (!rules.Any())
            {
                TempData["Message"] = "No allocation rules defined.";
                return RedirectToAction(nameof(Index));
            }

            var grants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.Reviews)
                .Where(g => g.Statuses.Contains(GrantStatus.ApprovedARCC) && g.IsAllocationCompleted == false)
                .ToListAsync();

            var totalAvailable = (await _context.Allocations.FirstOrDefaultAsync())?.AvailableAmount ?? 0;

            if (totalAvailable <= 0)
            {
                TempData["Error"] = "No available funds to allocate.";
                return RedirectToAction(nameof(Index));
            }

            decimal total = 0;

            foreach (var grant in grants)
            {
                if (!grant.Reviews.Any())
                    continue;

                decimal averageScore = grant.Reviews.Average(r => r.AverageScore);
                var rule = rules.FirstOrDefault(r => averageScore >= r.MinScore && averageScore <= r.MaxScore);

                if (rule != null)
                {
                    decimal requestedAmount = grant.BudgetItems
                        .Where(b => b.FundingSource == "ARCC")
                        .Sum(b => b.Amount);

                    grant.AllocatedFunds = requestedAmount * rule.PercentAllocated / 100m;
                    grant.AwardDate = DateTime.UtcNow;
                    total += (decimal)grant.AllocatedFunds;
                }
                else
                {
                    grant.AllocatedFunds = 0;
                }
            }

            if (total > totalAvailable)
            {
                TempData["Error"] = $"Allocation rules exceed available funds. Total allocated: ${total.ToString()}, Available: ${totalAvailable.ToString()}. Please adjust the rules.";
                return RedirectToAction(nameof(Index));
            }

            if (!ignoreOverage && (totalAvailable - total) > 5000)
            {
                TempData["ConfirmOverage"] = true;
                TempData["OverageAmount"] = (totalAvailable - total).ToString("C");
                return RedirectToAction(nameof(Index));
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Allocation rules applied successfully.";

            return RedirectToAction(nameof(Index));
        }

        [SessionAuthorize]
        [HttpGet]
        public async Task<IActionResult> PreviewAllocationData()
        {
            var rules = await _context.AllocationRules
                .OrderByDescending(r => r.MinScore)
                .ToListAsync();

            var grants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.Reviews)
                .Include(g => g.User)
                .Where(g => g.Statuses.Contains(GrantStatus.ApprovedARCC))
                .ToListAsync();

            var allocation = await _context.Allocations.FirstOrDefaultAsync();
            decimal totalAvailable = allocation?.AvailableAmount ?? 0;
            decimal totalRequested = 0;
            decimal totalAllocated = 0;

            var resultGrants = new List<object>();

            foreach (var grant in grants)
            {
                if (!grant.Reviews.Any())
                    continue;

                decimal averageScore = grant.Reviews.Average(r => r.AverageScore);
                var rule = rules.FirstOrDefault(r => averageScore >= r.MinScore && averageScore <= r.MaxScore);

                decimal requestedAmount = grant.BudgetItems
                    .Where(b => b.FundingSource == "ARCC")
                    .Sum(b => b.Amount);

                totalRequested += requestedAmount;

                decimal allocatedAmount = 0;
                string ruleRange = "No Rule";
                int percentAllocated = 0;

                if (rule != null)
                {
                    allocatedAmount = requestedAmount * rule.PercentAllocated / 100m;
                    ruleRange = $"{rule.MinScore}-{rule.MaxScore}";
                    percentAllocated = (int)rule.PercentAllocated;
                }

                totalAllocated += allocatedAmount;

                resultGrants.Add(new
                {
                    title = grant.Title,
                    principalInvestigator = grant.User.FirstName + " " + grant.User.LastName,
                    averageScore = averageScore,
                    requestedAmountFormatted = requestedAmount.ToString("C"),
                    ruleRange = ruleRange,
                    percentAllocated = percentAllocated,
                    allocatedAmountFormatted = allocatedAmount.ToString("C")
                });
            }

            decimal remainingFunds = totalAvailable - totalAllocated;

            return Json(new
            {
                totalRequestedFormatted = totalRequested.ToString("C"),
                totalAllocatedFormatted = totalAllocated.ToString("C"),
                availableFundsFormatted = totalAvailable.ToString("C"),
                remainingFundsFormatted = remainingFunds.ToString("C"),
                isOverAllocated = remainingFunds < 0,
                isLargeRemainder = remainingFunds > 5000,
                grants = resultGrants
            });
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
            ws.Columns("D").AdjustToContents();

            using MemoryStream stream = new();
            wb.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "GrantAllocations.xlsx");
        }

        private DataSet GetGrants()
        {
            DataSet ds = new();
            var constr = Configuration.GetConnectionString("DefaultConnection");
            string sql =
                "SELECT Grants.Title AS \"Grant Title\", " +
                "RIGHT('000000' + CAST(UserId AS VARCHAR(6)), 6) AS \"PI Account Number\", " +
                "Users.FirstName + ' ' + Users.LastName AS \"PI Name\", " +
                "'$' + CAST(AllocatedFunds AS VARCHAR(15)) AS \"Allocated Funds\" " +
                "FROM Grants " +
                "JOIN Users ON Grants.UserId = Users.Id " +
                "WHERE AllocatedFunds > 0 AND IsAllocationCompleted = 1 " +
                "ORDER BY Grants.Id;";
            using (SqlConnection con = new(constr))
            {
                using SqlDataAdapter sda = new(sql, con);
                sda.Fill(ds);
            }

            return ds;
        }
        [HttpPost]
        [SessionAuthorize]
        public async Task<IActionResult> CompleteAllocations()
        {
            var grants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.User)
                .Include(g => g.Reviews)
                .Where(g => !g.IsSaved && g.Reviews.Count() != 0 && g.Statuses.Contains(GrantStatus.ApprovedARCC) && !g.IsAllocationCompleted)
                .ToListAsync();


            foreach (var grant in grants)
            {
                if (grant.AllocatedFunds > 0)
                {
                    grant.IsAllocationCompleted = true; 
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}