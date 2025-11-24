using Microsoft.EntityFrameworkCore;
using InterventionsAPI.Models;

namespace InterventionsAPI.Data
{
    public class InterventionDbContext : DbContext
    {
        public InterventionDbContext(DbContextOptions<InterventionDbContext> options)
            : base(options) { }

        public DbSet<Intervention> Interventions { get; set; }
    }
}
