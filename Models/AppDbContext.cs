using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CAFE_MENU.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductProperty> ProductProperties { get; set; }

    public virtual DbSet<Property> Properties { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        try
        {
            IConfigurationRoot config = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
           .Build();

            optionsBuilder.UseSqlServer(config.GetConnectionString("DevConnection"));
        }
        catch (Exception)
        {
            throw;
        }

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0BA7C8CA84");

            entity.Property(e => e.CategoryName).HasMaxLength(255);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK__Categorie__Paren__267ABA7A");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6CD5E4E2CA2");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImagePath).HasMaxLength(500);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(255);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Products__Catego__2B3F6F97");
        });

        modelBuilder.Entity<ProductProperty>(entity =>
        {
            entity.HasKey(e => e.ProductPropertyId).HasName("PK__ProductP__4D7EDFA500BC5DFD");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductProperties)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductPr__Produ__36B12243");

            entity.HasOne(d => d.Property).WithMany(p => p.ProductProperties)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductPr__Prope__37A5467C");
        });

        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(e => e.PropertyId).HasName("PK__Properti__70C9A735C00E3100");

            entity.Property(e => e.Key).HasMaxLength(255);
            entity.Property(e => e.Value).HasMaxLength(255);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C9E38806D");

            entity.HasIndex(e => e.UserName, "UQ__Users__C9F284568D6B8FF2").IsUnique();

            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.HashPassword).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.SaltPassword).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(100);
        });
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C40E6B1EF62A115A");

            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(18, 2)") // TotalPrice için decimal türü
                .IsRequired(); // Zorunlu olduğunu belirtiyoruz

            // Diğer ilişkiler ve konfigürasyonlar
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
