using Denarius.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Denarius.Infrastructure.EntityTypeConfigurations;

internal class TransactionEntityTypeConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder
            .HasKey(x => x.Id);
        builder
            .OwnsOne(x => x.Title)
            .Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(50);
        builder
            .Property(x => x.Date)
            .IsRequired();
        builder
            .Property(x => x.Type)
            .HasPrecision(1)
            .IsRequired();
        builder
            .OwnsOne(x => x.Amount)
            .Property(x => x.Value)
            .HasPrecision(18, 2)
            .IsRequired();
        builder
            .OwnsOne(x => x.Amount)
            .Property(x => x.Code)
            .HasMaxLength(3)
            .IsRequired();
        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey("AccountId")
            .IsRequired();
        builder.HasOne(x => x.Tag)
            .WithMany()
            .HasForeignKey("TagId");
        builder
            .Property(x => x.CreatedAt)
            .IsRequired();
        builder
            .Property(x => x.UpdatedAt)
            .IsRequired();
    }
}
