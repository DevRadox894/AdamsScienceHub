using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AdamsScienceHub.Data
{
    // Factory EF uses at design time for migrations
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite("Data Source=adamshub.db"); // Same as Program.cs

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
