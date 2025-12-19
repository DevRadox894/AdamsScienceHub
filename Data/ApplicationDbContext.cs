using AdamsScienceHub.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace AdamsScienceHub.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<UserSubjectProgress> UserSubjectProgress { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuizResult> QuizResults { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Material configuration
            modelBuilder.Entity<Material>(entity =>
            {
                entity.HasKey(e => e.MaterialId);

                // Configure MaterialId as identity (auto-increment)
                entity.Property(e => e.MaterialId)
                    .ValueGeneratedOnAdd()
                    .UseIdentityByDefaultColumn();

                // Relationship with Subject
                entity.HasOne(m => m.Subject)
                    .WithMany() // If Subject doesn't have Materials collection
                    .HasForeignKey(m => m.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Question configuration (your existing code)
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Subject)
                .WithMany(s => s.Questions)
                .HasForeignKey(q => q.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public static string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

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