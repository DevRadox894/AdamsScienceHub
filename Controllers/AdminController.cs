using Microsoft.AspNetCore.Mvc;
using AdamsScienceHub.Data;
using AdamsScienceHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AdamsScienceHub.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Admin Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        //Manage Users Page
        public async Task<IActionResult> Users()
        {
            // Exclude admin users from the list
            var users = await _db.Users
                .Where(u => u.Role != "Admin")
                .ToListAsync();


            ViewBag.TotalUsers = users.Count;
            return View("ManageUsers",users); // Returns only non-admin users

        }




        // Optional: Delete User
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user != null)
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Users");
        }


      



    }
}
