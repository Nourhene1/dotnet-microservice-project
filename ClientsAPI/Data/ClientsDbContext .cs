using ClientsAPI.models;
using ClientsAPI.models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ClientsAPI.Data
{
    public class ClientsDbContext : DbContext
    {
        public ClientsDbContext(DbContextOptions<ClientsDbContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Reclamation> Reclamations { get; set; }
    }
}
