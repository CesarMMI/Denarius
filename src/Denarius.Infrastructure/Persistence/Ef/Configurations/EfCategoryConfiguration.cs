using Denarius.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Denarius.Infrastructure.Persistence.Ef.ModelConfigs;

internal class EfCategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Type)
            .IsRequired();

        builder.Property(c => c.Color)
            .IsRequired(false)
            .HasMaxLength(7);

        builder
            .Property(c => c.UserId)
            .IsRequired();
        builder
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(c => c.CreatedAt)
            .IsRequired();
        builder
            .Property(c => c.UpdatedAt)
            .IsRequired();
    }
}
