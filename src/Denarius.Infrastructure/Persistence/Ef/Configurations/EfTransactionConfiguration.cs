using Denarius.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Denarius.Infrastructure.Persistence.Ef.ModelConfigs;

internal class EfTransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .IsRequired(true)
            .HasColumnType("DECIMAL(18, 2)");

        builder.Property(t => t.Date)
            .IsRequired();

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.AccountId)
            .IsRequired();
        builder.HasOne(t => t.Account)
            .WithMany()
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(t => t.CategoryId)
            .IsRequired(false);
        builder.HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Property(t => t.CreatedAt)
            .IsRequired();
        builder.Property(t => t.UpdatedAt)
            .IsRequired();
    }
}
