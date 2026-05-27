using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = new SelectList(
                await _context.Departments.ToListAsync(),
                "Id",
                "DepartmentName"
            );
            ViewBag.Users = new SelectList(
                await _context.Users.ToListAsync(),
                "Id",
                "Email",
                HttpContext.Session.GetInt32("UserID")
            );
            return View();
        }

        // POST
        [HttpPost]
        public async Task<IActionResult> Create(
            Grant grant,
            List<IFormFile>? attachments,
            IFormFile? approvalFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = new SelectList(
                    await _context.Departments.ToListAsync(),
                    "Id",
                    "DepartmentName"
                );
                ViewBag.Users = new SelectList(
                    await _context.Users.ToListAsync(),
                    "Id",
                    "Email",
                    HttpContext.Session.GetInt32("UserID")
                );

                return View(grant);
            }
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

        // GET: e.g.: Grant/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            // Validate id is passed
            if (id == null)
            {
                return NotFound();
            }

            // Find grant in database by id
            var grant = await _context.Grants.FindAsync(id);
            if (grant == null)
            {
                return NotFound();
            }

            ViewBag.Departments = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Departments, "Id", "DepartmentName", grant.DepartmentId);
            ViewBag.Users = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Users, "Id", "Email", grant.UserId);

            // Return view with grant data
            return View(grant);
        }

        // POST: e.g.: Grant/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,ProjectSummary,Justification,ProjectImpact,isMultipleDepartments,DepartmentId,UserId,InvolvesHumanOrAnimalSubjects")] Grant grant, IFormFile? approvalFile, List<IFormFile> attachments)
        {
            // Validate id in url matches grant id
            if (id != grant.Id)
            {
                return NotFound();
            }

            // Validate model state
            if (ModelState.IsValid)
            {
                try
                {
                    // Update grant in database
                    _context.Update(grant);
                    await _context.SaveChangesAsync();
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
                {
                    // If grant does not exist, return not found
                    if (!GrantExists(grant.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Dashboard");
            }
            ViewBag.Departments = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Departments, "Id", "DepartmentName", grant.DepartmentId);
            ViewBag.Users = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Users, "Id", "Email", grant.UserId);
            return View(grant);
        }

        private bool GrantExists(int id)
        {
            return _context.Grants.Any(e => e.Id == id);
        }
    }
}