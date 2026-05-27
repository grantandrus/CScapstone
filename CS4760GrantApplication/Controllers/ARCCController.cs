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
                OtherUsers = _context.Users.Where(u => !u.IsCommitteeMember).ToList()
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
    }
}
