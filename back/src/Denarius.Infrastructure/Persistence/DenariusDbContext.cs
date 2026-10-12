using Denarius.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence;

public class DenariusDbContext(DbContextOptions<DenariusDbContext> options) : DbContext(options)
{
    /// <summary>The Brazilian Portuguese alphabetical order (accents and case only break ties), the same on any server.</summary>
    public const string PortugueseCollation = "pt_br_icu";

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("unaccent");
        modelBuilder.HasCollation(PortugueseCollation, locale: "pt-BR", provider: "icu", deterministic: true);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DenariusDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
