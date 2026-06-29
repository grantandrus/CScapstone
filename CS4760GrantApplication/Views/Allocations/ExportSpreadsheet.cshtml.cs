using ClosedXML.Excel;
using CS4760GrantApplication.Data;
using CS4760GrantApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;

namespace CS4760GrantApplication.Views.Allocations;
public partial class ExportSpreadsheet
{
}

public class ExcelModel : PageModel
{
    private readonly CS4760GrantApplicationContext _context;
    public ExcelModel(CS4760GrantApplicationContext context)
    {
        _context = context;
    }

    public FileResult OnGet()
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
                // There is no explicit "account number" on the User model; use Email as a substitute.
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
