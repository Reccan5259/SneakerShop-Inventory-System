using Microsoft.EntityFrameworkCore;
using SneakerShop.Api.Models;

namespace SneakerShop.Api.Data
{
    public class SneakerShopDbContext : DbContext
    {
        public SneakerShopDbContext(
            DbContextOptions<SneakerShopDbContext> options)
            : base(options)
        {
        }

        public DbSet<Item> Items => Set<Item>();

        public DbSet<User> Users => Set<User>();

        public DbSet<InventoryTransaction> InventoryTransactions =>
            Set<InventoryTransaction>();

        public DbSet<Order> Orders => Set<Order>();

        public DbSet<OrderLine> OrderLines => Set<OrderLine>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Item>()
                .HasIndex(item => item.Code)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasIndex(order => order.OrderNumber)
                .IsUnique();

            modelBuilder.Entity<Item>()
                .Property(item => item.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(order => order.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderLine>()
                .Property(line => line.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderLine>()
                .Property(line => line.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderLine>()
                .HasOne(line => line.Order)
                .WithMany(order => order.Lines)
                .HasForeignKey(line => line.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderLine>()
                .HasOne(line => line.Item)
                .WithMany()
                .HasForeignKey(line => line.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryTransaction>()
                .HasOne(transaction => transaction.Item)
                .WithMany()
                .HasForeignKey(transaction => transaction.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Item>().HasData(
                new Item
                {
                    Id = 1,
                    Name = "Air Force 1",
                    Code = "NK-AF1-WHT-09",
                    Brand = "Nike",
                    UnitPrice = 5495m,
                    Quantity = 10,
                    Size = 9m,
                    Colorway = "White",
                    Category = "Lifestyle",
                    ReleaseType = "Regular",
                    AuthenticityStatus = "Verified",
                    BoxCode = "NK-AF1-2026-001",
                    ReorderLevel = 5,
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026, 1, 1, 0, 0, 0,
                        DateTimeKind.Utc)
                },
                new Item
                {
                    Id = 2,
                    Name = "Samba OG",
                    Code = "AD-SMB-BLK-085",
                    Brand = "Adidas",
                    UnitPrice = 6800m,
                    Quantity = 7,
                    Size = 8.5m,
                    Colorway = "Black and White",
                    Category = "Lifestyle",
                    ReleaseType = "Regular",
                    AuthenticityStatus = "Verified",
                    BoxCode = "AD-SMB-2026-002",
                    ReorderLevel = 5,
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026, 1, 1, 0, 0, 0,
                        DateTimeKind.Utc)
                },
                new Item
                {
                    Id = 3,
                    Name = "574 Core",
                    Code = "NB-574-GRY-10",
                    Brand = "New Balance",
                    UnitPrice = 6595m,
                    Quantity = 4,
                    Size = 10m,
                    Colorway = "Grey",
                    Category = "Lifestyle",
                    ReleaseType = "Regular",
                    AuthenticityStatus = "Verified",
                    BoxCode = "NB-574-2026-003",
                    ReorderLevel = 5,
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026, 1, 1, 0, 0, 0,
                        DateTimeKind.Utc)
                }
            );
        }
    }
}