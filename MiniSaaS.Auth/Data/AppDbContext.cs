using Microsoft.EntityFrameworkCore;
using MiniSaaS.Auth.Models;
using MiniSaaS.Auth.Services;

namespace MiniSaaS.Auth.Data
{
    public class AppDbContext : DbContext
    {
        private readonly ITenantProvider _tenantProvider;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            ITenantProvider tenantProvider)
            : base(options)
        {
            _tenantProvider = tenantProvider;
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public DbSet<Tenant> Tenants => Set<Tenant>();

        public Guid CurrentTenantId => _tenantProvider.TenantId;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasQueryFilter(u =>
                    CurrentTenantId == Guid.Empty
                    || u.TenantId == CurrentTenantId);
        }
    }
}