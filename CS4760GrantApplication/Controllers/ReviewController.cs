using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Models;
using CS4760GrantApplication.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;


namespace CS4760GrantApplication.Controllers
{
    public class ReviewController : Controller
    {
        private readonly CS4760GrantApplicationContext _context;
        private readonly IWebHostEnvironment _environment;

        public ReviewController(
            CS4760GrantApplicationContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public List<Grant> Grants = new();

        [SessionAuthorize]
        public async Task<IActionResult> Index()
        {

            Grants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Include(g => g.User)
                .Include(g => g.Review)
                .Where(g => !g.IsSaved)
                .ToListAsync();

            return View(Grants);
        }

        public async Task<IActionResult> ReviewGrant(int id)
        {
            var grant = await _context.Grants
                .Include(g => g.User)
                .Include(g => g.College)
                .Include(g => g.Departments)
                .Include(g => g.Attachments)
                .Include(g => g.BudgetItems)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grant == null)
                return NotFound();

            return View(grant);
        }

        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var attachment = await _context.GrantAttachments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attachment == null)
                return NotFound();

            var relativePath = attachment.FilePath.TrimStart('/', '\\');

            var fullPath = Path.Combine(
                _environment.WebRootPath,
                relativePath);

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(
                    fullPath,
                    out string? contentType))
            {
                contentType = "application/octet-stream";
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(
                fullPath); 

            return File(bytes, contentType, fullPath);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview(Review review)
        {
            _context.Reveiws.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
