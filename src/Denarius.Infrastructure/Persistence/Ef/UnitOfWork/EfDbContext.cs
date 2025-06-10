using Denarius.Domain.Models;
using Denarius.Infrastructure.Persistence.Ef.ModelConfigs;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.Ef.UnitOfWork;

internal class EfDbContext(DbContextOptions<EfDbContext> options) : DbContext(options)
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
