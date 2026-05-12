using Denarius.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Denarius.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.UserId).IsRequired();
        builder.Property(t => t.AccountId).IsRequired();
        builder.Property(t => t.CategoryId);
        builder.Property(t => t.TransferPeerId); // plain column — no FK (transfer pairs are mutually referential)
        builder.Property(t => t.Type).IsRequired().HasConversion<int>();
        builder.Property(t => t.Amount).IsRequired().HasColumnType("TEXT");
        builder.Property(t => t.Description).IsRequired().HasMaxLength(500);
        builder.Property(t => t.Date).IsRequired();
        builder.Property(t => t.IsIncomingTransfer).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        builder.Ignore(t => t.IsTransfer);

        builder.HasOne<Account>().WithMany()
               .HasForeignKey(t => t.AccountId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Category>().WithMany()
               .HasForeignKey(t => t.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.AccountId);
        builder.HasIndex(t => new { t.UserId, t.Date });
    }
}
