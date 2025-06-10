using Denarius.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.Ef.ModelConfigs;

internal static class EfTransactionModelConfig
{
    public static ModelBuilder ConfigureTransactionModel(this ModelBuilder modelBuilder)
    {
        // PK
        modelBuilder.Entity<Transaction>()
            .HasKey(t => t.Id);
        // Fields
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .IsRequired(true)
            .HasColumnType("DECIMAL(18, 2)");
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Date)
            .IsRequired();
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(50);
        // Account Relation
        modelBuilder.Entity<Transaction>()
            .Property(t => t.AccountId)
            .IsRequired();
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
        // Category Relation
        modelBuilder.Entity<Transaction>()
            .Property(t => t.CategoryId)
            .IsRequired(false);
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.ClientSetNull);
        // Timestamps
        modelBuilder.Entity<Transaction>()
            .Property(t => t.CreatedAt)
            .IsRequired();
        modelBuilder.Entity<Transaction>()
            .Property(t => t.UpdatedAt)
            .IsRequired();
        return modelBuilder;
    }
}
