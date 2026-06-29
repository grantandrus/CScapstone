using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;

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

        // Returns the view that contains the download button
        [SessionAuthorize]
        [HttpGet]
        public IActionResult ExportSpreadsheet()
        {
            return View();
        }

        // Endpoint to download the spreadsheet
        [SessionAuthorize]
        [HttpGet]
        public FileResult DownloadAllocations()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ARCC Allocations");
                worksheet.Cell(1, 1).Value = "PI Name";
                worksheet.Cell(1, 2).Value = "PI Account (email)";
                worksheet.Cell(1, 3).Value = "Grant Title";
                worksheet.Cell(1, 4).Value = "ARCC Allocated";

                var grants = _context.Grants
                    .Include(g => g.User)
                    .Include(g => g.BudgetItems)
                    .ToList();

                for (int i = 0; i < grants.Count; i++)
                {
                    var grant = grants[i];
                    var piName = grant.User != null ? $"{grant.User.FirstName} {grant.User.LastName}" : string.Empty;
                    var piAccount = grant.User?.Email ?? string.Empty;
                    var arccAmount = grant.BudgetItems?.Where(b => b.FundingSource == "ARCC").Sum(b => b.Amount) ?? 0m;

                    worksheet.Cell(i + 2, 1).Value = piName;
                    worksheet.Cell(i + 2, 2).Value = piAccount;
                    worksheet.Cell(i + 2, 3).Value = grant.Title;
                    worksheet.Cell(i + 2, 4).Value = arccAmount;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ARCC_Allocations.xlsx");
                }
            }
        }
    }
}
