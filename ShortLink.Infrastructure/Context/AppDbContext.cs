using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ShortLink.Domain.Entities;
using ShortLink.Domain.ValueObject;

namespace ShortLink.Infrastructure.Context;

public sealed class AppDbContext : DbContext
{
    public DbSet<Link> Links { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Link>(entity =>
        {
            entity.ToTable("Links");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(e => e.ShortCode)
                .HasConversion(
                    v => v.Value,
                    v => ShortCode.Create(v),
                    new ValueComparer<ShortCode>(
                        (l, r) => l != null && r != null && l.Value == r.Value,
                        v => v.Value.GetHashCode(),
                        v => ShortCode.Create(v.Value)))
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("ShortCode");

            entity.HasIndex(e => e.ShortCode)
                .IsUnique()
                .HasDatabaseName("IX_Links_ShortCode");

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime");

            entity.Property(e => e.ClickCount)
                .IsRequired();
        });
    }
}