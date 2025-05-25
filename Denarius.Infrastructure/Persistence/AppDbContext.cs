using Denarius.Domain.Models;
using Denarius.Infrastructure.Persistence.ModelConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence;

internal class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder
            .ConfigureUserModel()
            .ConfigureAccountModel()
            .ConfigureCategoryModel()
            .ConfigureTransactionModel();
    }
}
