using Denarius.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.ModelConfigurations;

internal static class AccountConfiguration
{
    public static ModelBuilder ConfigureAccountModel(this ModelBuilder modelBuilder)
    {
        // PK
        modelBuilder.Entity<Account>()
            .HasKey(u => u.Id);
        // Fields
        modelBuilder.Entity<Account>()
            .Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(50);
        modelBuilder.Entity<Account>()
            .Property(a => a.Color)
            .IsRequired(false)
            .HasMaxLength(7);
        modelBuilder.Entity<Account>()
            .Property(a => a.Balance)
            .IsRequired(true)
            .HasColumnType("DECIMAL(18, 2)");
        // User Relation
        modelBuilder.Entity<Account>()
            .Property(a => a.UserId)
            .IsRequired();
        modelBuilder.Entity<Account>()
            .HasOne(a => a.User)
            .WithMany(u => u.Accounts)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        // Timestamps
        modelBuilder.Entity<Account>()
            .Property(u => u.CreatedAt)
            .IsRequired();
        modelBuilder.Entity<Account>()
            .Property(u => u.UpdatedAt)
            .IsRequired();
        return modelBuilder;
    }
}
