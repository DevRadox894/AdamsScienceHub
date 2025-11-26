using AdamsScienceHub.Data;          // ✅ Needed for ApplicationDbContext
using AdamsScienceHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;     // ✅ Needed for Session
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;

namespace AdamsScienceHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;   // ✅ Database context

        // Inject DbContext into the controller
        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> UserDashboard()
        {
            // Get email from claims (set during SignInAsync)
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login", "Account");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewBag.Subjects = await _db.Subjects.ToListAsync();

            return View(user);
        }

        public IActionResult AdminDashboard()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
