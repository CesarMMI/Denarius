using Denarius.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.ModelConfigurations;

internal static class CategoryConfiguration
{
    public static ModelBuilder ConfigureCategoryModel(this ModelBuilder modelBuilder)
    {
        // PK
        modelBuilder.Entity<Category>()
            .HasKey(u => u.Id);
        // Fields
        modelBuilder.Entity<Category>()
            .Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(50);
        modelBuilder.Entity<Category>()
            .Property(a => a.Type)
            .IsRequired();
        modelBuilder.Entity<Category>()
            .Property(a => a.Color)
            .IsRequired(false)
            .HasMaxLength(7);
        // User Relation
        modelBuilder.Entity<Category>()
            .Property(a => a.UserId)
            .IsRequired();
        modelBuilder.Entity<Category>()
            .HasOne(a => a.User)
            .WithMany(u => u.Categories)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        // Timestamps
        modelBuilder.Entity<Category>()
            .Property(u => u.CreatedAt)
            .IsRequired();
        modelBuilder.Entity<Category>()
            .Property(u => u.UpdatedAt)
            .IsRequired();
        return modelBuilder;
    }
}
