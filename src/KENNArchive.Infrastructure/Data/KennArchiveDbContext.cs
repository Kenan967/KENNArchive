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
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
    }
}