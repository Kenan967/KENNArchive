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
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    public DbSet<EmailVerificationCode> EmailVerificationCodes
        => Set<EmailVerificationCode>();

    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<ReturnRequest> ReturnRequests => Set<ReturnRequest>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    // Məhsul rəyləri.
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Məhsulun qiyməti.
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        // Məhsul və ölçülər arasındakı əlaqə.
        modelBuilder.Entity<ProductVariant>()
            .HasOne(v => v.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Eyni məhsul üçün eyni ölçü təkrar yaradıla bilməz.
        modelBuilder.Entity<ProductVariant>()
            .HasIndex(v => new { v.ProductId, v.Size })
            .IsUnique();

        modelBuilder.Entity<ProductVariant>()
            .Property(v => v.Size)
            .HasMaxLength(10)
            .IsRequired();

        // Sifarişin ümumi qiyməti.
        modelBuilder.Entity<Order>()
            .Property(o => o.TotalPrice)
            .HasPrecision(18, 2);

        // Sifariş elementlərinin qiymətləri.
        modelBuilder.Entity<OrderItem>()
            .Property(o => o.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(o => o.TotalPrice)
            .HasPrecision(18, 2);

        // Məhsul rəyi ilə məhsul arasındakı əlaqə.
        modelBuilder.Entity<ProductReview>()
            .HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Məhsul rəyi ilə istifadəçi arasındakı əlaqə.
        modelBuilder.Entity<ProductReview>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Rəy mətni maksimum 1000 simvol ola bilər.
        modelBuilder.Entity<ProductReview>()
            .Property(r => r.Comment)
            .HasMaxLength(1000)
            .IsRequired();

        // Məhsulun rəylərini axtarmaq üçün indeks.
        modelBuilder.Entity<ProductReview>()
            .HasIndex(r => r.ProductId);
    }
}