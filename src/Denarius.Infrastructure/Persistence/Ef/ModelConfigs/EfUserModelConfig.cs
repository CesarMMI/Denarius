using Denarius.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.Ef.ModelConfigs;

internal static class EfUserModelConfig
{
    public static ModelBuilder ConfigureUserModel(this ModelBuilder modelBuilder)
    {
        // PK
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);
        // Fields
        modelBuilder.Entity<User>()
            .Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(50);
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        modelBuilder.Entity<User>()
            .Property(u => u.HashedPassword)
            .IsRequired();
        // Account Relation
        modelBuilder.Entity<User>()
            .HasMany(u => u.Accounts)
            .WithOne(s => s.User);
        // Category Relation
        modelBuilder.Entity<User>()
            .HasMany(u => u.Categories)
            .WithOne(s => s.User);
        // Timestamps
        modelBuilder.Entity<User>()
            .Property(u => u.CreatedAt)
            .IsRequired();
        modelBuilder.Entity<User>()
            .Property(u => u.UpdatedAt)
            .IsRequired();
        return modelBuilder;
    }
}
