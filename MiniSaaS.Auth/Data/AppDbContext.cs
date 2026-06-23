using Microsoft.EntityFrameworkCore;
using MiniSaaS.Auth.Models;

namespace MiniSaaS.Auth.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
    }
}
