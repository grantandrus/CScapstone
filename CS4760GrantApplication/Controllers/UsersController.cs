using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using CS4760GrantApplication.Migrations;
using CS4760GrantApplication.Models;
using CS4760GrantApplication.ViewModels;
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

        public async Task<IActionResult> Index()
        {
            return View();
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

            colleges.Insert(0, new Models.College() { Id = 0, Name = "None" });
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
        
    }
}
