using Denarius.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence;

public class DenariusDbContext(DbContextOptions<DenariusDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DenariusDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
