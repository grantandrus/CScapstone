using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Models;
using CS4760GrantApplication.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CS4760GrantApplication.Controllers
{
    public class UsersController : Controller
    {
        private readonly CS4760GrantApplicationContext _context;

        public UsersController(CS4760GrantApplicationContext context)
        {
            _context = context;
        }

        [SessionAuthorize]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
                return RedirectToAction("Login", "Users");

            var user = await _context.Users
                .Include(u => u.College)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpGet]
        [GuestOnly]
        public IActionResult Login() => View();

        [HttpPost]
        [GuestOnly]
        public async Task<IActionResult> Login(string email, string password)
        {
            var hasher = new PasswordHasher<User>();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            string role = user.IsAdmin ? "admin" : "regular";

            // Store user info in session
            HttpContext.Session.SetInt32("UserID", user.Id);
            HttpContext.Session.SetString("UserRole", role);
            HttpContext.Session.SetString("Name", $"{user.FirstName} {user.LastName}");

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: Users/Create
        [GuestOnly]
        public IActionResult Create()
        {
            var colleges = _context.Colleges.ToList();
            var departments = _context.Departments.ToList();

            colleges.Insert(0, new College() { Id = 0, Name = "None" });
            departments.Insert(0, new Department() { Id = 0, DepartmentName = "None" });

            ViewBag.CollegeList = new SelectList(colleges, "Id", "Name");
            ViewBag.DepartmentList = new SelectList(departments, "Id", "DepartmentName");

            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [GuestOnly]
        public async Task<IActionResult> Create(RegisterViewModel regUser)
        {

            User user = new User();
            var hasher = new PasswordHasher<User>();

            if (ModelState.IsValid)
            {
                var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == regUser.Email);
                if (existing != null)
                {
                    ViewBag.Error = "An account with that email already exists.";
                    return View(regUser);
                }

                user.FirstName = regUser.FirstName;
                user.LastName = regUser.LastName;
                user.Email = regUser.Email;
                user.PasswordHash = hasher.HashPassword(user, regUser.Password);
                user.IsAdmin = false;
                
                if (regUser.CollegeId != null && regUser.CollegeId != 0)
                {
                    user.CollegeId = regUser.CollegeId;
                }

                if (regUser.DepartmentId != null && regUser.DepartmentId != 0)
                {
                    user.DepartmentId = regUser.DepartmentId;
                }

                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");

            }

            return View(regUser);
        }

        [SessionAuthorize]
        public async Task<IActionResult> Edit()
        {
            var userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
                return RedirectToAction("Login", "Users");

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return RedirectToAction("Login", "Users");

            var viewModel = new EditProfileViewModel
            {
                UserID = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CollegeId = user.CollegeId,
                DepartmentId = user.DepartmentId
            };

            var colleges = _context.Colleges.ToList();
            var departments = _context.Departments.ToList();

            colleges.Insert(0, new College() { Id = 0, Name = "None" });
            departments.Insert(0, new Department() { Id = 0, DepartmentName = "None" });

            ViewBag.CollegeList = new SelectList(colleges, "Id", "Name", user.CollegeId);
            ViewBag.DepartmentList = new SelectList(departments, "Id", "DepartmentName", user.DepartmentId);


            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public async Task<IActionResult> Edit(EditProfileViewModel user)
        {
            var id = HttpContext.Session.GetInt32("UserID");
            var emailCheck = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (id != user.UserID)
                return RedirectToAction("Login", "Users");

            var existing = await _context.Users.FindAsync(id);

            if (existing == null)
                return RedirectToAction("Login", "Users");

            existing.FirstName = user.FirstName;
            existing.LastName = user.LastName;

            if (emailCheck == null) // If not in use
            {
                existing.Email = user.Email;
            }
            else if (existing.Email != user.Email && emailCheck != null) // If not user's current email but is in use by another
            {
                var colleges = _context.Colleges.ToList();
                var departments = _context.Departments.ToList();
                colleges.Insert(0, new College() { Id = 0, Name = "None" });
                departments.Insert(0, new Department() { Id = 0, DepartmentName = "None" });
                ViewBag.CollegeList = new SelectList(colleges, "Id", "Name", user.CollegeId);
                ViewBag.DepartmentList = new SelectList(departments, "Id", "DepartmentName", user.DepartmentId);

                ViewBag.Error = "An account with that email already exists.";
                return View();
            }

            if (user.CollegeId != null && user.CollegeId != 0)
            {
                existing.CollegeId = user.CollegeId;
            }

            if (user.DepartmentId != null && user.DepartmentId != 0)
            {
                existing.DepartmentId = user.DepartmentId;
            }

            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("Name", $"{user.FirstName} {user.LastName}");

            return RedirectToAction(nameof(Index));
        }

        [SessionAuthorize]
        public async Task<IActionResult> EditPassword()
        {
            var userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
                return RedirectToAction("Login", "Users");

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return RedirectToAction("Login", "Users");

            var viewModel = new EditPasswordViewModel
            {
                UserID = user.Id,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public async Task<IActionResult> EditPassword(EditPasswordViewModel user)
        {
            var id = HttpContext.Session.GetInt32("UserID");
            var hasher = new PasswordHasher<User>();

            if (id != user.UserID)
                return RedirectToAction("Login", "Users");

            var existing = await _context.Users.FindAsync(id);

            if (existing == null)
                return RedirectToAction("Login", "Users");

            existing.PasswordHash = hasher.HashPassword(existing, user.Password);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
