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
                RubricCriteria = _context.RubricCriteria.Include(r => r.RatingSuggestions).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditCriterion(int id, string Name, string Description, int MaxScore, List<int> SuggestionScores, List<string> SuggestionDescriptions)
        {
            var criterion = await _context.RubricCriteria.Include(r => r.RatingSuggestions).FirstOrDefaultAsync(r => r.Id == id);
            if (criterion == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                criterion.Name = Name;
                criterion.Description = Description;
                criterion.MaxScore = MaxScore;

                // Handle Rating Suggestions Replace
                _context.RatingSuggestions.RemoveRange(criterion.RatingSuggestions);

                var newSuggestions = new List<Models.RatingSuggestion>();
                if (SuggestionScores != null && SuggestionDescriptions != null && SuggestionScores.Count == SuggestionDescriptions.Count)
                {
                    for (int i = 0; i < SuggestionScores.Count; i++)
                    {
                        if(!string.IsNullOrWhiteSpace(SuggestionDescriptions[i]))
                        {
                            newSuggestions.Add(new Models.RatingSuggestion
                            {
                                Score = SuggestionScores[i],
                                Description = SuggestionDescriptions[i]
                            });
                        }
                    }
                }
                criterion.RatingSuggestions = newSuggestions;

                _context.Update(criterion);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Create");
        }

        [HttpPost]
        public async Task<IActionResult> AddCriterion(string Name, string Description, int MaxScore, List<int> SuggestionScores, List<string> SuggestionDescriptions)
        {
            if (ModelState.IsValid)
            {
                var criterion = new Models.RubricCriterion
                {
                    Name = Name,
                    Description = Description,
                    MaxScore = MaxScore
                };

                var newSuggestions = new List<Models.RatingSuggestion>();
                if (SuggestionScores != null && SuggestionDescriptions != null && SuggestionScores.Count == SuggestionDescriptions.Count)
                {
                    for (int i = 0; i < SuggestionScores.Count; i++)
                    {
                         if(!string.IsNullOrWhiteSpace(SuggestionDescriptions[i]))
                         {
                            newSuggestions.Add(new Models.RatingSuggestion
                            {
                                Score = SuggestionScores[i],
                                Description = SuggestionDescriptions[i]
                            });
                         }
                    }
                }
                criterion.RatingSuggestions = newSuggestions;

                _context.RubricCriteria.Add(criterion);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Create");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCriterion(int id)
        {
            var criterion = await _context.RubricCriteria
                .Include(r => r.RatingSuggestions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (criterion != null)
            {
                _context.RatingSuggestions.RemoveRange(criterion.RatingSuggestions);
                _context.RubricCriteria.Remove(criterion);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Create");
        }
    }
}
