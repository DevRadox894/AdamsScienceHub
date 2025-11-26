using AdamsScienceHub.Models;

namespace AdamsScienceHub.Data
{
    public static class DbSeeder
    {
        public static void SeedAdmin(ApplicationDbContext context)
        {
            // If admin already exists, do nothing
            if (context.Users.Any(u => u.Role == "Admin"))
                return;

            // Create an admin account
            var admin = new User
            {
                FullName = "Adams Rasheed O",
                Email = "adamsrasheed44@gmail.com",
                PasswordHash = ApplicationDbContext.HashPassword("AdamsIsAdmin123"),
                Role = "Admin"
            };

            context.Users.Add(admin);
            context.SaveChanges();
        }
    }
}
