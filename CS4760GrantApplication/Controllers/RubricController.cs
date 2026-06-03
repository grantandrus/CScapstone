using CS4760GrantApplication.Data;
using CS4760GrantApplication.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CS4760GrantApplication.Controllers
{
    public class RubricController : Controller
    {
        private readonly CS4760GrantApplicationContext _context;

        public RubricController(CS4760GrantApplicationContext context)
        {
            _context = context;
        }

        // GET: Rubric/Create
        public IActionResult Create()
        {
            var model = new ViewModels.RubricCreateViewModel
            {
                RubricCriteria = _context.RubricCriteria.ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditCriterion(int id, string Name, string Description, int MaxScore, string RatingSuggestion)
        {
            var criterion = await _context.RubricCriteria.FindAsync(id);
            if (criterion == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                criterion.Name = Name;
                criterion.Description = Description;
                criterion.MaxScore = MaxScore;
                criterion.RatingSuggestion = RatingSuggestion;

                _context.Update(criterion);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Create");
        }

        [HttpPost]
        public async Task<IActionResult> AddCriterion(string Name, string Description, int MaxScore, string RatingSuggestion)
        {
            if (ModelState.IsValid)
            {
                var criterion = new Models.RubricCriterion
                {
                    Name = Name,
                    Description = Description,
                    MaxScore = MaxScore,
                    RatingSuggestion = RatingSuggestion
                };

                _context.RubricCriteria.Add(criterion);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Create");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCriterion(int id)
        {
            var criterion = await _context.RubricCriteria.FindAsync(id);
            if (criterion != null)
            {
                _context.RubricCriteria.Remove(criterion);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Create");
        }
    }
}
