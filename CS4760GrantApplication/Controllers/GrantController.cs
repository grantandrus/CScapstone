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
                    UserId = vm.UserId
                };

                grant.Departments = await _context.Departments.Where(
                    d => vm.SelectedDepartmentIds.Contains(d.Id)
                    ).ToListAsync();


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
                isMultipleDepartments = grant.isMultipleDepartments,
                UserId = grant.UserId,
                InvolvesHumanOrAnimalSubjects = grant.InvolvesHumanOrAnimalSubjects,

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
            CreateGrantViewModel vm)
        {
            if (id != vm.Id)
            {
                return NotFound();
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

            var grant = await _context.Grants
                .Include(g => g.Departments)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grant == null)
            {
                return NotFound();
            }

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

            // Update many-to-many Departments
            grant.Departments = await _context.Departments
                .Where(d => vm.SelectedDepartmentIds.Contains(d.Id))
                .ToListAsync();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GrantExists(id))
                {
                    return NotFound();
                }

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