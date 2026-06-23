using Microsoft.EntityFrameworkCore;
using MiniSaaS.Auth.Models;
using MiniSaaS.Auth.Services;

namespace MiniSaaS.Auth.Data
{
    public class AppDbContext : DbContext
    {
        private readonly CurrentUserService _currentUser;


        public AppDbContext(DbContextOptions<AppDbContext> options, CurrentUserService currentUser) : base(options) { _currentUser = currentUser; }

        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasQueryFilter(x =>
                    x.TenantId == _currentUser.TenantId);

            modelBuilder.Entity<RefreshToken>()
                .HasQueryFilter(x =>
                    x.User.TenantId == _currentUser.TenantId);
        }


    }
}
