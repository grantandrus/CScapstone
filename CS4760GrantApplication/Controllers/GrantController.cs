using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CS4760GrantApplication.ViewModels;

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
                    UserId = vm.UserId,
                    IsSaved = vm.IsSaved
                };

                grant.Departments = await _context.Departments.Where(
                    d => vm.SelectedDepartmentIds.Contains(d.Id)
                    ).ToListAsync();


                _context.Add(grant);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(vm.BudgetDescription) ||
                    !string.IsNullOrWhiteSpace(vm.BudgetItemType) ||
                    !string.IsNullOrWhiteSpace(vm.BudgetFundingSource) ||
                    vm.BudgetAmount.HasValue)
                                {
                    var budgetItem = new BudgetItem
                    {
                        GrantId = grant.Id,
                        Description = vm.BudgetDescription,
                        ItemType = vm.BudgetItemType,
                        FundingSource = vm.BudgetFundingSource,
                        Amount = vm.BudgetAmount ?? 0
                    };

                    _context.BudgetItems.Add(budgetItem);
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

            var budgetItem = await _context.BudgetItems
            .FirstOrDefaultAsync(b => b.GrantId == grant.Id);

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
                BudgetDescription = budgetItem?.Description ?? string.Empty,
                BudgetItemType = budgetItem?.ItemType ?? string.Empty,
                BudgetFundingSource = budgetItem?.FundingSource ?? string.Empty,
                BudgetAmount = budgetItem?.Amount,
                isMultipleDepartments = grant.isMultipleDepartments,
                UserId = grant.UserId,
                InvolvesHumanOrAnimalSubjects = grant.InvolvesHumanOrAnimalSubjects,
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
            grant.IsSaved = vm.IsSaved;

            grant.Departments = await _context.Departments
                .Where(d => vm.SelectedDepartmentIds.Contains(d.Id))
                .ToListAsync();

            var budgetItem = await _context.BudgetItems
    .FirstOrDefaultAsync(b => b.GrantId == grant.Id);

            if (!string.IsNullOrWhiteSpace(vm.BudgetDescription)
                || !string.IsNullOrWhiteSpace(vm.BudgetItemType)
                || !string.IsNullOrWhiteSpace(vm.BudgetFundingSource)
                || vm.BudgetAmount.HasValue)
            {
                if (budgetItem == null)
                {
                    budgetItem = new BudgetItem
                    {
                        GrantId = grant.Id
                    };

                    _context.BudgetItems.Add(budgetItem);
                }

                budgetItem.Description = vm.BudgetDescription;
                budgetItem.ItemType = vm.BudgetItemType;
                budgetItem.FundingSource = vm.BudgetFundingSource;
                budgetItem.Amount = vm.BudgetAmount ?? 0;
            }
            else if (budgetItem != null)
            {
                _context.BudgetItems.Remove(budgetItem);
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

        private bool GrantExists(int id)
        {
            return _context.Grants.Any(e => e.Id == id);
        }
    }
}