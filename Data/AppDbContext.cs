using Microsoft.EntityFrameworkCore;
using URUN.Models;

namespace URUN.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet tanımları
    public DbSet<Product> Products { get; set; } // Product tablosu
    public DbSet<Category> Categories { get; set; } // Category tablosu 
    public DbSet<User> Users { get; set; } // User tablosu (Keyless olarak tanımlandı)

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // İlişki tanımları
        modelBuilder.Entity<Category>()
            .HasMany(c => c.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId);

        // Tablo adını özelleştir
        modelBuilder.Entity<Category>().ToTable("Category");

        // User tablosu için birincil anahtar olmadığını belirtin
        modelBuilder.Entity<User>().HasNoKey();

        base.OnModelCreating(modelBuilder);
    }
}
