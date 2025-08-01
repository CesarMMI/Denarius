using Denarius.Domain.Models;
using Denarius.Infrastructure.Persistence.Ef.Configurations;
using Denarius.Infrastructure.Persistence.Ef.ModelConfigs;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.Ef.AppDbContext;

internal class EfAppDbContext(DbContextOptions<EfAppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new EfUserConfiguration());
        modelBuilder.ApplyConfiguration(new EfAccountConfiguration());
        modelBuilder.ApplyConfiguration(new EfCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new EfTransactionConfiguration());
    }
}
