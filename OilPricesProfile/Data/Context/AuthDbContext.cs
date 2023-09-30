using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OilPricesProfile.Models;

namespace OilPricesProfile.Data.Context
{
    public class AuthDbContext : IdentityDbContext<User>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
            // Database.EnsureCreated();
        }

        // DbSet for User entity is automatically created by IdentityDbContext
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> Roles { get; set; }
        // Your other DbSet properties and configurations go here

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}