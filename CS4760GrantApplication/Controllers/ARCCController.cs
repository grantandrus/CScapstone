using CS4760GrantApplication.Data;
using CS4760GrantApplication.Models;
using CS4760GrantApplication.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CS4760GrantApplication.Controllers
{
    public class ARCCController : Controller
    {
        private readonly CS4760GrantApplicationContext _context;
        public ARCCController(CS4760GrantApplicationContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var model = new ARCCViewModel
            {
                Members = _context.Users.Where(u => u.IsCommitteeMember).ToList(),
                OtherUsers = _context.Users.Where(u => !u.IsCommitteeMember).ToList(),
                CurrentChairId = _context.Users.Where(u => u.IsCommitteeChair).Select(u => (int?)u.Id).FirstOrDefault(),
                SubmittedGrants = _context.Grants // Fetch submitted grants
                    .Include(g => g.Title)
                    .Include(g => g.User)
                    .Include(g => g.BudgetItems)
                    .Where(g => g.IsSaved)
                    .ToList()
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Remove(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                user.IsCommitteeMember = false;
                if(user.IsCommitteeChair)
                {
                    user.IsCommitteeChair = false;
                }
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Add(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                user.IsCommitteeMember = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateChair(int selectedUserId)
        {
            var currentChair = await _context.Users.FirstOrDefaultAsync(u => u.IsCommitteeChair);
            // if a current chair is found then they must be removed as chair since there can be only one
            if(currentChair != null)
            {
                currentChair.IsCommitteeChair = false;
                // still have the old chair as a committee member by default though (should already be set but just in case)
                currentChair.IsCommitteeMember = true;
            }

            var newChair = await _context.Users.FindAsync(selectedUserId);
            if(newChair != null)
            {
                newChair.IsCommitteeChair = true;
                // make sure they are a committee member too
                newChair.IsCommitteeMember = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
