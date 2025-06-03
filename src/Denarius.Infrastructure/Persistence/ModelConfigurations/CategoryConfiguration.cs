using Denarius.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.ModelConfigurations;

internal static class CategoryConfiguration
{
    public static ModelBuilder ConfigureCategoryModel(this ModelBuilder modelBuilder)
    {
        // PK
        modelBuilder.Entity<Category>()
            .HasKey(c => c.Id);
        // Fields
        modelBuilder.Entity<Category>()
            .Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50);
        modelBuilder.Entity<Category>()
            .Property(c => c.Type)
            .IsRequired();
        modelBuilder.Entity<Category>()
            .Property(c => c.Color)
            .IsRequired(false)
            .HasMaxLength(7);
        // Transaction Relation
        modelBuilder.Entity<Category>()
            .HasMany(c => c.Transactions)
            .WithOne(t => t.Category);
        // User Relation
        modelBuilder.Entity<Category>()
            .Property(c => c.UserId)
            .IsRequired();
        modelBuilder.Entity<Category>()
            .HasOne(c => c.User)
            .WithMany(u => u.Categories)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        // Timestamps
        modelBuilder.Entity<Category>()
            .Property(c => c.CreatedAt)
            .IsRequired();
        modelBuilder.Entity<Category>()
            .Property(c => c.UpdatedAt)
            .IsRequired();
        return modelBuilder;
    }
}
