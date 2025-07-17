using Microsoft.EntityFrameworkCore;
using DSWIntegral.Models;

namespace DSWIntegral.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; } = default!;

        // Más DbSet para Order, OrderItem, Customer...
    }
}