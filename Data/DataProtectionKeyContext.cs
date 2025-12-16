using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdamsScienceHub.Data
{
    public class DataProtectionKeyContext : DbContext, IDataProtectionKeyContext
    {
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        public DataProtectionKeyContext(DbContextOptions<DataProtectionKeyContext> options)
            : base(options)
        { }
    }
}
