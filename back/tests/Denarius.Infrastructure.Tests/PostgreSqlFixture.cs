using Denarius.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Denarius.Infrastructure.Tests;

/// <summary>A disposable PostgreSQL in Docker with the migrations applied, shared by the tests of a class.</summary>
public class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18.0").Build();

    public virtual async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    public DenariusDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<DenariusDbContext>().UseNpgsql(_postgres.GetConnectionString()).Options);
}
