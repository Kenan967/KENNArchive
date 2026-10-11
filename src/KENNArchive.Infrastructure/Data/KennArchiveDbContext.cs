using KENNArchive.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;



namespace KENNArchive.Infrastructure.Data;
public class KennArchiveDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public KennArchiveDbContext(
        DbContextOptions<KennArchiveDbContext> options)
        : base(options)
    {
    }
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Brand> Brands => Set<Brand>();


    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<EmailVerificationCode> EmailVerificationCodes
        => Set<EmailVerificationCode>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<ReturnRequest> ReturnRequests => Set<ReturnRequest>();

    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    // Müştəri və mağaza sahibi arasındakı mesajlar.
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
        // Sifarişin ümumi qiymətinin dəqiqliyini təyin edir.
        modelBuilder.Entity<Order>()
            .Property(o => o.TotalPrice)
            .HasPrecision(18, 2);

        // Sifarişdəki məhsulun qiymətinin dəqiqliyini təyin edir.
        modelBuilder.Entity<OrderItem>()
            .Property(o => o.UnitPrice)
            .HasPrecision(18, 2);

        // Sifarişdəki məhsulların ümumi qiymətinin dəqiqliyini təyin edir.
        modelBuilder.Entity<OrderItem>()
            .Property(o => o.TotalPrice)
            .HasPrecision(18, 2);
    }
}