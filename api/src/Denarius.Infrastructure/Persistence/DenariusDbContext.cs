using Denarius.Domain.Entities;
using Denarius.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence;

public class DenariusDbContext(DbContextOptions<DenariusDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
    }
}
