using AuthAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<AuthUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Email unique
            modelBuilder.Entity<AuthUser>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
