using Denarius.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Denarius.Infrastructure.Persistence.Ef.ModelConfigs;

internal class EfUserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(u => u.Email)
            .IsUnique();
        
        builder.Property(u => u.HashedPassword)
            .IsRequired();
        
        builder.Property(u => u.CreatedAt)
            .IsRequired();
        builder.Property(u => u.UpdatedAt)
            .IsRequired();
    }
}
