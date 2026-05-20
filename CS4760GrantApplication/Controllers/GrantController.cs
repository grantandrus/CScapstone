using CS4760GrantApplication.Data;
using CS4760GrantApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace CS4760GrantApplication.Controllers
{
    public class GrantController : Controller
    {
        private readonly CS4760GrantApplicationContext _context;

        public GrantController(CS4760GrantApplicationContext context)
        {
            _context = context;
        }

        // GET
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST
        [HttpPost]
        public async Task<IActionResult> Create(Grant grant)
        {
            if (ModelState.IsValid)
            {
                // TODO: once database conneciton is up, add user id as foriegn key
                _context.Add(grant);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Dashboard");
            }

            return View(grant);
        }
    }
}
