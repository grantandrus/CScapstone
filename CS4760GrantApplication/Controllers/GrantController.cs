using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace CS4760GrantApplication.Controllers
{
    public class GrantController : Controller
    {
        private readonly CS4760GrantApplicationContext _context;
        private readonly IWebHostEnvironment _environment;

        public GrantController(CS4760GrantApplicationContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET
        [SessionAuthorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST
        [HttpPost]
        public async Task<IActionResult> Create(
            Grant grant,
            List<IFormFile>? attachments,
            IFormFile? approvalFile)
        {
            if (attachments != null && attachments.Count > 3)
            {
                ModelState.AddModelError("attachments", "You can upload up to 3 files.");
            }

            if (grant.InvolvesHumanOrAnimalSubjects && approvalFile == null)
            {
                ModelState.AddModelError("approvalFile", "An approval file is required when human or animal subjects are involved.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(grant);
                await _context.SaveChangesAsync();

                string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                if (attachments != null)
                {
                    foreach (var file in attachments)
                    {
                        if (file.Length > 0)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                            string filePath = Path.Combine(uploadFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            var grantAttachment = new GrantAttachment
                            {
                                GrantId = grant.Id,
                                FileName = file.FileName,
                                FilePath = "/uploads/" + uniqueFileName,
                                IsApprovalFile = false
                            };

                            _context.GrantAttachments.Add(grantAttachment);
                        }
                    }
                }

                if (approvalFile != null && approvalFile.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(approvalFile.FileName);
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await approvalFile.CopyToAsync(fileStream);
                    }

                    var approvalAttachment = new GrantAttachment
                    {
                        GrantId = grant.Id,
                        FileName = approvalFile.FileName,
                        FilePath = "/uploads/" + uniqueFileName,
                        IsApprovalFile = true
                    };

                    _context.GrantAttachments.Add(approvalAttachment);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Dashboard");
            }

            return View(grant);
        }
    }
}