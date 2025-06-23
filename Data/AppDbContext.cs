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

        // Más DbSet para Order, OrderItem, Customer...
    }
}