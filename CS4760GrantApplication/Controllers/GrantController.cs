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

            // Return view with grant data
            return View(grant);
        }

        // POST: e.g.: Grant/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,title,description,UserId")] Grant grant)
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
            return View(grant);
        }

        private bool GrantExists(int id)
        {
            return _context.Grants.Any(e => e.Id == id);
        }
    }
}
