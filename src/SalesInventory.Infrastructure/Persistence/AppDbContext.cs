using Microsoft.EntityFrameworkCore;
using SalesInventory.Domain.Entities;

namespace SalesInventory.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            e.Property(c => c.Description).HasMaxLength(500);
            e.HasData(
                new Category { Id = 1, Name = "Văn phòng phẩm" },
                new Category { Id = 2, Name = "Đồ gia dụng" },
                new Category { Id = 3, Name = "Thực phẩm" });
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Sku).HasMaxLength(50).IsRequired();
            e.HasIndex(p => p.Sku).IsUnique();
            e.Property(p => p.Name).HasMaxLength(200).IsRequired();
            e.Property(p => p.PurchasePrice).HasPrecision(18, 2);
            e.Property(p => p.SalePrice).HasPrecision(18, 2);
            // Optimistic concurrency on stock (was [ConcurrencyCheck] on the entity).
            e.Property(p => p.StockQuantity).IsConcurrencyToken();
            e.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Products_PurchasePrice_NonNegative", "[PurchasePrice] >= 0");
                t.HasCheckConstraint("CK_Products_SalePrice_NonNegative", "[SalePrice] >= 0");
                t.HasCheckConstraint("CK_Products_StockQuantity_NonNegative", "[StockQuantity] >= 0");
            });
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.Property(o => o.CustomerName).HasMaxLength(200).IsRequired();
            e.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            e.HasMany(o => o.Items).WithOne().HasForeignKey(i => i.OrderId);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.ToTable("OrderItems");
            e.Property(i => i.ProductName).HasMaxLength(200);
            e.Property(i => i.UnitPrice).HasPrecision(18, 2);
            // Sản phẩm đã có trong đơn hàng thì không được xóa.
            e.HasOne<Product>().WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
