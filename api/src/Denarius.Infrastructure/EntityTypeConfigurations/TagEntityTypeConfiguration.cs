using Denarius.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Denarius.Infrastructure.EntityTypeConfigurations;

internal class TagEntityTypeConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
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
            .Property(x => x.CreatedAt)
            .IsRequired();
        builder
            .Property(x => x.UpdatedAt)
            .IsRequired();
    }
}
