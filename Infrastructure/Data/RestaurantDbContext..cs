using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Domain.Entities;

namespace RestaurantManagementSystem.infrastructure.Data
{
    public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options) { }

        public DbSet<Menu> Menus { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ArchevedOrder> ArchevedOrders { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Categorie> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Order va OrderItem o'rtasidagi bog'lanishni sozlash
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne()
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);  // Buyurtma o'chirilganda OrderItems ham o'chiriladi

             modelBuilder.Entity<Menu>()
            .HasOne(m => m.Categorie)
            .WithMany(c => c.Menus)
            .HasForeignKey(m => m.CategorieId)
            .OnDelete(DeleteBehavior.Cascade);

        }


    }
}