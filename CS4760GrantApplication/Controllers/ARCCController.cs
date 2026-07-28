using CS4760GrantApplication.Data;
using CS4760GrantApplication.Models;
using CS4760GrantApplication.ViewModels;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CS4760GrantApplication.Controllers
{
    public class ARCCController : Controller
    {
        private readonly CS4760GrantApplicationContext _context;
        public ARCCController(CS4760GrantApplicationContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var model = new ARCCViewModel
            {
                Members = _context.Users.Where(u => u.IsCommitteeMember).ToList(),
                OtherUsers = _context.Users.Where(u => !u.IsCommitteeMember).ToList(),
                CurrentChairId = _context.Users.Where(u => u.IsCommitteeChair).Select(u => (int?)u.Id).FirstOrDefault(),
                SubmittedGrants = _context.Grants // Fetch submitted grants
                                                  //.Include(g => g.Title)
                    .Include(g => g.User)
                    .Include(g => g.BudgetItems)
                    .Where(g => !g.IsSaved) // Note: !IsSaved is submitted, IsSaved is a draft
                    .ToList()
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Remove(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                user.IsCommitteeMember = false;
                if (user.IsCommitteeChair)
                {
                    user.IsCommitteeChair = false;
                }
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Add(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                user.IsCommitteeMember = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateChair(int selectedUserId)
        {
            var currentChair = await _context.Users.FirstOrDefaultAsync(u => u.IsCommitteeChair);
            // if a current chair is found then they must be removed as chair since there can be only one
            if (currentChair != null)
            {
                currentChair.IsCommitteeChair = false;
                // still have the old chair as a committee member by default though (should already be set but just in case)
                currentChair.IsCommitteeMember = true;
            }

            var newChair = await _context.Users.FindAsync(selectedUserId);
            if (newChair != null)
            {
                newChair.IsCommitteeChair = true;
                // make sure they are a committee member too
                newChair.IsCommitteeMember = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ARCCReport()
        {

            var collegeModel = _context.Grants.Where(g => g.AllocatedFunds > 0).GroupBy(g => g.CollegeId).Select(g => new
            {
                CollegeId = g.Key,
                AllocatedFunds = g.Sum(f => f.AllocatedFunds!)!
            }).ToList().Select(item => new SimpleReportViewModel
            {
                DimensionOne = _context.Colleges
                .Where(c => c.Id == item.CollegeId)
                .Select(c => c.Name)
                .FirstOrDefault() ?? "Unknown",
                Quantity = (int)(item.AllocatedFunds ?? 0)
            })
            .ToList();

            var departmentModel = _context.Grants.Include(g => g.User).Where(g => g.AllocatedFunds > 0 && g.User!.Department != null).GroupBy(g => g.User!.DepartmentId).Select(g => new
            {
                DepartmentId = g.Key,
                AllocatedFunds = g.Sum(f => f.AllocatedFunds!)!
            }).ToList().Select(item => new SimpleReportViewModel
            {
                DimensionOne = _context.Departments
                .Where(d => d.Id == item.DepartmentId)
                .Select(d => d.DepartmentName)
                .FirstOrDefault() ?? "Unknown",
                Quantity = (int)(item.AllocatedFunds ?? 0)
            })
            .ToList();

            int acceptedGrants = _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.User)
                .Include(g => g.Reviews)
                .Where(g => !g.IsSaved && g.Reviews.Count() != 0 && g.Statuses.Contains(GrantStatus.ApprovedARCC))
                .Count();

            int rejectedGrants = _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.User)
                .Include(g => g.Reviews)
                .Where(g => !g.IsSaved && g.Reviews.Count() != 0 && g.Statuses.Contains(GrantStatus.RejectedARCC))
                .Count();

            var rejectModel = new List<SimpleReportViewModel>
            {
                new SimpleReportViewModel { DimensionOne = "Accepted", Quantity = acceptedGrants },
                new SimpleReportViewModel { DimensionOne = "Rejected", Quantity = rejectedGrants }
            };

            var topModel = new List<SimpleReportViewModel>();

            var topGrants = await _context.Grants
               .Include(g => g.BudgetItems)
               .Where(g => g.IsAllocationCompleted == true && g.BudgetItems.Any(b => b.FundingSource == "ARCC"))
               .OrderByDescending(g => g.AllocatedFunds)
               .Take(5)
               .ToListAsync();

            foreach (var grant in topGrants)
            {
                topModel.Add(new SimpleReportViewModel
                {
                    DimensionOne = grant.Title + " (ID: " + grant.Id + ")",
                    Quantity = (int)grant.AllocatedFunds!
                });
            }

            decimal totalNum = (decimal)_context.Grants.Where(g => g.AllocatedFunds > 0).Sum(g => g.AllocatedFunds)!;

            string totalString = totalNum.ToString("#,#.00");

            var vm = new ARCCReportViewModel
            {
                CollegeData = collegeModel,
                DepartmentData = departmentModel,
                RejectData = rejectModel,
                TopData = topModel,
                TotalAllocated = totalNum,
                AllocatedString = totalString,
            };

            return View(vm);
        }

    }
}
