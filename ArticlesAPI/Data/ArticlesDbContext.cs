using Microsoft.EntityFrameworkCore;
using ArticlesAPI.Models;

namespace ArticlesAPI.Data
{
    public class ArticlesDbContext : DbContext
    {
        public ArticlesDbContext(DbContextOptions<ArticlesDbContext> options)
            : base(options) { }

        public DbSet<Article> Articles { get; set; }
    }
}
