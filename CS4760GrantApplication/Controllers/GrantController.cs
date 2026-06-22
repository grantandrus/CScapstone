using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Models;
using CS4760GrantApplication.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;

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
            var vm = new CreateGrantViewModel();

            vm.Departments = _context.Departments
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.DepartmentName
                })
                .ToList();

            vm.Users = _context.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.FirstName + " " + u.LastName,
                    Selected = u.Id == HttpContext.Session.GetInt32("UserID")
                })
                .ToList();

            vm.Colleges = _context.Colleges.
                Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }
                ).ToList();

            return View(vm);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public async Task<IActionResult> Create(
            CreateGrantViewModel vm,
            List<IFormFile>? attachments,
            IFormFile? approvalFile)
        {
            if (attachments != null && attachments.Count > 3)
            {
                ModelState.AddModelError("attachments", "You can upload up to 3 files.");
            }

            if (vm.InvolvesHumanOrAnimalSubjects && approvalFile == null)
            {
                ModelState.AddModelError("approvalFile", "An approval file is required when human or animal subjects are involved.");
            }
            if (!vm.isMultipleDepartments &&
                vm.SelectedDepartmentIds.Count > 1)
            {
                ModelState.AddModelError(
                    nameof(vm.SelectedDepartmentIds),
                    "Only one department may be selected unless Multiple Departments is checked."
                );
            }

            if (!vm.IsSaved)
            {
                if (!vm.Acknowledgement1 || !vm.Acknowledgement2 || !vm.Acknowledgement3 || !vm.Acknowledgement4)
                {
                    ModelState.AddModelError("", "All acknowledgements must be accepted to submit.");
                }

                if (string.IsNullOrWhiteSpace(vm.Signature))
                {
                    ModelState.AddModelError("", "Signature is required to submit.");
                }

                if (vm.SignatureDate.Date != DateTime.Today)
                {
                    ModelState.AddModelError("", "Signature Date must match today's date to submit.");
                }
            }

            // signature should be inputted right before submission, not when saved
            if(vm.IsSaved)
            {
                ModelState.Remove("Signature");
            }

            if (!ModelState.IsValid)
            {
                vm.Departments = _context.Departments
                     .Select(d => new SelectListItem
                     {
                         Value = d.Id.ToString(),
                         Text = d.DepartmentName
                     })
                     .ToList();

                vm.Users = _context.Users
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = u.FirstName + " " + u.LastName
                    })
                    .ToList();

                vm.Colleges = _context.Colleges.
                    Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }
                    ).ToList();

                return View(vm);
            }

            if (ModelState.IsValid)
            {
                var grant = new Grant
                {
                    Title = vm.Title,
                    Description = vm.Description,
                    ProjectSummary = vm.ProjectSummary,
                    Justification = vm.Justification,
                    ProjectImpact = vm.ProjectImpact,
                    ProjectTimeline = vm.ProjectTimeline,
                    SuccessEvaluation = vm.SuccessEvaluation,
                    isMultipleDepartments = vm.isMultipleDepartments,
                    InvolvesHumanOrAnimalSubjects = vm.InvolvesHumanOrAnimalSubjects,
                    HasAbroadSupport = vm.HasAbroadSupport,
                    Dissemination = vm.Dissemination,
                    UserId = vm.UserId,
                    CollegeId = vm.CollegeId,
                    IsSaved = vm.IsSaved,
                    Acknowledgement1 = vm.Acknowledgement1,
                    Acknowledgement2 = vm.Acknowledgement2,
                    Acknowledgement3 = vm.Acknowledgement3,
                    Acknowledgement4 = vm.Acknowledgement4,
                    Signature = vm.Signature == null ? "" : vm.Signature,
                    SignatureDate = vm.SignatureDate
                };

                grant.Departments = await _context.Departments.Where(
                    d => vm.SelectedDepartmentIds.Contains(d.Id)
                    ).ToListAsync();


                _context.Add(grant);
                await _context.SaveChangesAsync();

                foreach (var item in vm.BudgetItems)
                {
                    if (!string.IsNullOrWhiteSpace(item.Description) ||
                        !string.IsNullOrWhiteSpace(item.ItemType) ||
                        !string.IsNullOrWhiteSpace(item.FundingSource) ||
                        item.Amount > 0)
                    {
                        item.GrantId = grant.Id;
                        _context.BudgetItems.Add(item);
                    }
                }

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

            return View(vm);
        }

        // GET: e.g.: Grant/Edit/5
        [HttpGet]
        [SessionAuthorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grant = await _context.Grants
                .Include(g => g.Departments)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grant == null)
            {
                return NotFound();
            }

            // Retrieve attached files associated with this grant
            var attachments = await _context.GrantAttachments
                .Where(ga => ga.GrantId == grant.Id)
                .ToListAsync();

            var budgetItems = await _context.BudgetItems
            .Where(b => b.GrantId == grant.Id)
            .ToListAsync();

            var vm = new CreateGrantViewModel
            {
                Id = grant.Id,
                Title = grant.Title,
                Description = grant.Description,
                ProjectSummary = grant.ProjectSummary,
                Justification = grant.Justification,
                ProjectImpact = grant.ProjectImpact,
                ProjectTimeline = grant.ProjectTimeline,
                SuccessEvaluation = grant.SuccessEvaluation,
                BudgetItems = budgetItems,
                isMultipleDepartments = grant.isMultipleDepartments,
                UserId = grant.UserId,
                InvolvesHumanOrAnimalSubjects = grant.InvolvesHumanOrAnimalSubjects,
                HasAbroadSupport = grant.HasAbroadSupport,
                Dissemination = grant.Dissemination,
                IsSaved = grant.IsSaved,
                Attachments = attachments, // Include attachments in the view model

                SelectedDepartmentIds = grant.Departments
                    .Select(d => d.Id)
                    .ToList()
            };

            vm.Departments = _context.Departments
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.DepartmentName
                })
                .ToList();

            vm.Users = _context.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.FirstName + " " + u.LastName
                })
                .ToList();

            return View(vm);
        }

        // POST: e.g.: Grant/Edit/5
        [HttpPost]
        [SessionAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CreateGrantViewModel vm,
            List<IFormFile>? attachments,
            IFormFile? approvalFile)
        {
            if (id != vm.Id)
            {
                return NotFound();
            }

            var existingAttachments = await _context.GrantAttachments
                .Where(ga => ga.GrantId == id)
                .ToListAsync();

            var existingRegularCount = existingAttachments.Count(ga => !ga.IsApprovalFile);
            var removedRegularCount = existingAttachments.Count(ga => vm.AttachmentsToRemove.Contains(ga.Id) && !ga.IsApprovalFile);
            var incomingRegularCount = attachments?.Count(a => a.Length > 0) ?? 0;

            var resultingFileCount = (existingRegularCount - removedRegularCount) + incomingRegularCount;

            // Enforce limit of 3
            if (resultingFileCount > 3)
            {
                ModelState.AddModelError("attachments", "You can only have up to 3 attached files.");
            }

            bool removingApproval = existingAttachments.Any(ga => vm.AttachmentsToRemove.Contains(ga.Id) && ga.IsApprovalFile);
            bool hasExistingApproval = existingAttachments.Any(ga => ga.IsApprovalFile) && !removingApproval;

            if (vm.InvolvesHumanOrAnimalSubjects && !hasExistingApproval && (approvalFile == null || approvalFile.Length == 0))
            {
                ModelState.AddModelError("approvalFile", "An approval file is required when human or animal subjects are involved.");
            }

            if (!vm.IsSaved)
            {
                if (!vm.Acknowledgement1 || !vm.Acknowledgement2 || !vm.Acknowledgement3 || !vm.Acknowledgement4)
                {
                    ModelState.AddModelError("", "All acknowledgements must be accepted to submit.");
                }

                if (string.IsNullOrWhiteSpace(vm.Signature))
                {
                    ModelState.AddModelError("", "Signature is required to submit.");
                }

                if (vm.SignatureDate.Date != DateTime.Today)
                {
                    ModelState.AddModelError("", "Signature Date must match today's date to submit.");
                }
            }

            // signature should be inputted right before submission, not when saved
            if (vm.IsSaved)
            {
                ModelState.Remove("Signature");
            }

            if (!ModelState.IsValid)
            {
                vm.Departments = _context.Departments
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.DepartmentName
                    })
                    .ToList();

                vm.Users = _context.Users
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = u.FirstName + " " + u.LastName
                    })
                    .ToList();

                vm.Attachments = existingAttachments;
                return View(vm);
            }

            var grant = await _context.Grants
                .Include(g => g.Departments)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grant == null) return NotFound();

            // Update scalar properties
            grant.Title = vm.Title;
            grant.Description = vm.Description;
            grant.ProjectSummary = vm.ProjectSummary;
            grant.Justification = vm.Justification;
            grant.ProjectImpact = vm.ProjectImpact;
            grant.ProjectTimeline = vm.ProjectTimeline;
            grant.SuccessEvaluation = vm.SuccessEvaluation;
            grant.isMultipleDepartments = vm.isMultipleDepartments;
            grant.UserId = vm.UserId;
            grant.InvolvesHumanOrAnimalSubjects = vm.InvolvesHumanOrAnimalSubjects;
            grant.HasAbroadSupport = vm.HasAbroadSupport;
            grant.Dissemination = vm.Dissemination;
            grant.IsSaved = vm.IsSaved;
            grant.Acknowledgement1 = vm.Acknowledgement1;
            grant.Acknowledgement2 = vm.Acknowledgement2;
            grant.Acknowledgement3 = vm.Acknowledgement3;
            grant.Acknowledgement4 = vm.Acknowledgement4;
            grant.Signature = vm.Signature == null ? "" : vm.Signature;
            grant.SignatureDate = vm.SignatureDate;

            grant.Departments = await _context.Departments
                .Where(d => vm.SelectedDepartmentIds.Contains(d.Id))
                .ToListAsync();

            var existingBudgetItems = await _context.BudgetItems
                .Where(b => b.GrantId == grant.Id)
                .ToListAsync();

            _context.BudgetItems.RemoveRange(existingBudgetItems);

            foreach (var item in vm.BudgetItems)
            {
                if (!string.IsNullOrWhiteSpace(item.Description) ||
                    !string.IsNullOrWhiteSpace(item.ItemType) ||
                    !string.IsNullOrWhiteSpace(item.FundingSource) ||
                    item.Amount > 0)
                {
                    item.Id = 0;
                    item.GrantId = grant.Id;
                    _context.BudgetItems.Add(item);
                }
            }

            try
            {
                string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                // 1. Process Removals
                if (vm.AttachmentsToRemove != null && vm.AttachmentsToRemove.Any())
                {
                    var filesToDelete = existingAttachments.Where(ga => vm.AttachmentsToRemove.Contains(ga.Id)).ToList();

                    foreach (var file in filesToDelete)
                    {
                        // Remove from filesystem
                        string physicalPath = Path.Combine(_environment.WebRootPath, file.FilePath.TrimStart('/'));
                        if (System.IO.File.Exists(physicalPath))
                        {
                            System.IO.File.Delete(physicalPath);
                        }

                        // Remove from DB
                        _context.GrantAttachments.Remove(file);
                    }
                }

                // 2. Process New Incoming Files
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

                            _context.GrantAttachments.Add(new GrantAttachment
                            {
                                GrantId = grant.Id,
                                FileName = file.FileName,
                                FilePath = "/uploads/" + uniqueFileName,
                                IsApprovalFile = false
                            });
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

                    _context.GrantAttachments.Add(new GrantAttachment
                    {
                        GrantId = grant.Id,
                        FileName = approvalFile.FileName,
                        FilePath = "/uploads/" + uniqueFileName,
                        IsApprovalFile = true
                    });
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GrantExists(id)) return NotFound();
                throw;
            }

            return RedirectToAction("Index", "Dashboard");
        }

        public async Task<IActionResult> DeptReview(int id)
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
        [SessionAuthorize]
        [DeptChair]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var grant = await _context.Grants.FindAsync(id);
            if (grant == null) return NotFound();

            grant.DeptReviewStatus = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [SessionAuthorize]
        [DeptChair]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var grant = await _context.Grants.FindAsync(id);
            if (grant == null) return NotFound();

            grant.DeptReviewStatus = false;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Dashboard");
        }

        private bool GrantExists(int id)
        {
            return _context.Grants.Any(e => e.Id == id);
        }
    }
}