using AdamsScienceHub.Data;
using AdamsScienceHub.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdamsScienceHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: Register
        [HttpPost]
        public IActionResult Register(string FullName, string Email, string Password, string ConfirmPassword)
        {
            if (!Regex.IsMatch(FullName, @"^[a-zA-Z\s]+$"))
            {
                ModelState.AddModelError(string.Empty, "Full Name can only contain letters and spaces.");
                return View();
            }

            var allowedDomains = new[] { "gmail.com", "yahoo.com", "hotmail.com" };
            var emailDomain = Email.Split('@').Last().ToLower();
            if (!allowedDomains.Contains(emailDomain))
            {
                ModelState.AddModelError(string.Empty, "Email must be Gmail, Yahoo, or Hotmail.");
                return View();
            }

            if (Password != ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View();
            }

            if (_db.Users.Any(u => u.Email == Email))
            {
                ModelState.AddModelError(string.Empty, "Email already registered.");
                return View();
            }

            var passwordHash = HashPassword(Password);

            var user = new User
            {
                FullName = FullName,
                Email = Email,
                PasswordHash = passwordHash,
                Role = "User"
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            TempData["Success"] = "Registration successful! You can now login.";
            return RedirectToAction("Login");
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string Email, string Password, bool RememberMe = false)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError(string.Empty, "Email and Password are required.");
                return View();
            }

            var user = _db.Users.FirstOrDefault(u => u.Email == Email);
            if (user == null || !VerifyPassword(Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View();
            }

            // --- Create claims for authentication ---
            var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Name, user.FullName),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, user.Role)
};

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = RememberMe };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );


            // Redirect based on role
            return RedirectToAction(user.Role == "Admin" ? "AdminDashboard" : "UserDashboard", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // --- Password hashing ---
        private string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);

            var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return $"{Convert.ToBase64String(salt)}:{hash}";
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            var hashToCheck = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8);

            return hashToCheck.SequenceEqual(hash);
        }

        // --- Get user info for profile modal ---
        public IActionResult GetUserInfo()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
                return Json(new { success = false });

            var user = _db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                fullName = user.FullName,
                email = user.Email
            });
        }

        //updateprofie
        [HttpPost]
        public IActionResult UpdateProfile(string fullName)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null)
                return Json(new { success = false });

            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return Json(new { success = false });

            user.FullName = fullName;
            _db.SaveChanges();

            return Json(new { success = true });
        }


        //PROGRESS

        public async Task<IActionResult> Progress()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login", "Account");

            var user = await _db.Users
                .Include(u => u.QuizResults)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var results = user.QuizResults;

            var vm = new UserProgressViewModel
            {
                TotalQuizzesTaken = results.Count,
                TotalQuestionsAnswered = results.Sum(x => x.TotalQuestions),
                AverageScore = results.Any() ? results.Average(x => x.Score) : 0,
                BestScore = results.Any() ? results.Max(x => x.Score) : 0,
                WorstScore = results
                                        .Where(x => x.Score > 0)       // ignore zero scores
                                        .DefaultIfEmpty()              // safe if empty
                                        .Min(x => x?.Score ?? 0),


                SubjectPerformances = results
                    .GroupBy(x => x.SubjectName)
                    .Select(g => new SubjectPerformance
                    {
                        SubjectName = g.Key,
                        QuizzesTaken = g.Count(),
                        AverageScore = g.Average(x => x.Score)
                    }).ToList(),

                QuizHistory = results
                    .OrderByDescending(x => x.DateTaken)
                    .Select(x => new QuizHistoryItem
                    {
                        Date = x.DateTaken,
                        Subject = x.SubjectName,
                        Score = x.Score,
                        Correct = x.CorrectAnswers,
                        Wrong = x.WrongAnswers,
                        TimeSpent = x.TimeSpent
                    }).ToList()
            };

            return View(vm);
        }



    }
}
