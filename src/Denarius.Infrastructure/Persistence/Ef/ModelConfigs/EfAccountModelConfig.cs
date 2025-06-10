using Denarius.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.Ef.ModelConfigs;

internal static class EfAccountModelConfig
{
    public static ModelBuilder ConfigureAccountModel(this ModelBuilder modelBuilder)
    {
        // PK
        modelBuilder.Entity<Account>()
            .HasKey(a => a.Id);
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
        // Transaction Relation
        modelBuilder.Entity<Account>()
            .HasMany(a => a.Transactions)
            .WithOne(t => t.Account);
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
            .Property(a => a.CreatedAt)
            .IsRequired();
        modelBuilder.Entity<Account>()
            .Property(a => a.UpdatedAt)
            .IsRequired();
        return modelBuilder;
    }
}
