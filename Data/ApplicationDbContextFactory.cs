using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AdamsScienceHub.Data
{
    public class ApplicationDbContextFactory
        : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // ✅ DESIGN-TIME ONLY (local dev / migrations)
            optionsBuilder.UseSqlite("Data Source=adamshub-dev.db");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
