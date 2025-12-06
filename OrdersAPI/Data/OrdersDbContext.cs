using Microsoft.EntityFrameworkCore;
using OrdersAPI.Models;

namespace OrdersAPI.Data
{
    
        public class OrdersDbContext : DbContext
        {
            public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
                : base(options)
            {
            }

            public DbSet<Cart> Carts { get; set; }
            public DbSet<CartItem> CartItems { get; set; }

            public DbSet<Order> Orders { get; set; }
            public DbSet<OrderItem> OrderItems { get; set; }
        

    }

}
