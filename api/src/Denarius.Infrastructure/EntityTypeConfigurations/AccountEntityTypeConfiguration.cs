using Denarius.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Denarius.Infrastructure.EntityTypeConfigurations;

internal class AccountEntityTypeConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder
            .HasKey(x => x.Id);
        builder
            .OwnsOne(x => x.Name)
            .Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(50);
        builder
            .OwnsOne(x => x.Color)
            .Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(9);
        builder
            .OwnsOne(x => x.InitialBalance)
            .Property(x => x.Value)
            .HasPrecision(18,2)
            .IsRequired();
        builder
            .OwnsOne(x => x.InitialBalance)
            .Property(x => x.Code)
            .HasMaxLength(3)
            .IsRequired();
        builder
            .Property(x => x.CreatedAt)
            .IsRequired();
        builder
            .Property(x => x.UpdatedAt)
            .IsRequired();
    }
}
