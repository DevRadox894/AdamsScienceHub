using AdamsScienceHub.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace AdamsScienceHub.Data
{
    public static class DbSeeder
    {
        public static void SeedAdmin(ApplicationDbContext context)
        {
            // If admin already exists, do nothing
            if (context.Users.Any(u => u.Role == "Admin"))
                return;

            var admin = new User
            {
                FullName = "Adams Rasheed O",
                Email = "adamsrasheed44@gmail.com",
                PasswordHash = HashPassword("AdamsIsAdmin123"),
                Role = "Admin"
            };

            context.Users.Add(admin);
            context.SaveChanges();
        }

        // SAME hashing logic as AccountController
        private static string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return $"{Convert.ToBase64String(salt)}:{hash}";
        }
    }
}
