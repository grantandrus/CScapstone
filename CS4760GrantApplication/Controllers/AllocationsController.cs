using ClosedXML.Excel;
using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Models;
using DocumentFormat.OpenXml.Spreadsheet;
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