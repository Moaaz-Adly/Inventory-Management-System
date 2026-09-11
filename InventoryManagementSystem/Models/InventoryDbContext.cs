using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace InventoryManagementSystem.Models
{
    public class InventoryDbContext : DbContext 
    {

        public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial catalog=ITIPro;Integrated Security=true;TrustServerCertificate=true");
        }

    }
}
