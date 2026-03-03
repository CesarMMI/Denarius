using Denarius.Domain.Entities;
using Denarius.Infrastructure.EntityTypeConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Contexts;

internal class AppDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccountEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TagEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionEntityTypeConfiguration());
    }
}
