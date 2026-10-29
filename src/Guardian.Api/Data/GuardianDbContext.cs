using Guardian.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Guardian.Api.Data;

// Контекст Entity Framework для PostgreSQL.
public class GuardianDbContext(DbContextOptions<GuardianDbContext> options) : DbContext(options)
{
    public DbSet<Visitor> Visitors => Set<Visitor>();
    public DbSet<VisitorAction> VisitorActions => Set<VisitorAction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Visitor>(entity =>
        {
            entity.ToTable("Visitors");
            entity.HasKey(visitor => visitor.Id);
            entity.Property(visitor => visitor.Trust).IsRequired();
            entity.HasCheckConstraint("CK_Visitors_Trust", "\"Trust\" >= 0 AND \"Trust\" <= 100");
        });

        modelBuilder.Entity<VisitorAction>(entity =>
        {
            entity.ToTable("VisitorActions");
            entity.HasKey(action => action.Id);
            entity.Property(action => action.Action).HasMaxLength(20).IsRequired();
            entity.Property(action => action.CreatedAt).IsRequired();
            entity.HasOne(action => action.Visitor)
                .WithMany(visitor => visitor.Actions)
                .HasForeignKey(action => action.VisitorId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(action => new { action.VisitorId, action.Action }).IsUnique();
        });
    }
}
