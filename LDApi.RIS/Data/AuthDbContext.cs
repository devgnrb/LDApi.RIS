using LDApi.RIS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace LDApi.RIS.Data
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<LoginAuditLog> LoginAuditLogs => Set<LoginAuditLog>();

        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }
    }
    // Ce fallback permet à EF Core CLI de créer le DbContext pour les migrations,
    // sans avoir besoin que tous les services DI soient satisfaits.
    public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
    {
        public AuthDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AuthDbContext>();
            // Utilise la même configuration que celle de runtime — exemple SQLite
            builder.UseSqlite("Data Source=LDARisDb.db")
            .EnableSensitiveDataLogging();
            return new AuthDbContext(builder.Options);
        }
    }
}
