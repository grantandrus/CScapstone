using CS4760GrantApplication.Data;
using CS4760GrantApplication.Models;
using CS4760GrantApplication.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        // GET: People
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || user.PasswordHash != password)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            string role = user.IsAdmin ? "admin" : "regular";

            // Store user info in session
            HttpContext.Session.SetInt32("UserID", user.Id);
            HttpContext.Session.SetString("UserRole", role);
            HttpContext.Session.SetString("Name", $"{user.FirstName} {user.LastName}");

            return View("Index", "Home");
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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

                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");

            }

            return View(regUser);
        }
        
    }
}
