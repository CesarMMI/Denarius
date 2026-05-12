using Denarius.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Tests.Shared;

public abstract class RepositoryTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly DenariusDbContext Context;

    protected RepositoryTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<DenariusDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new DenariusDbContext(options);
        Context.Database.EnsureCreated();
    }

    protected DenariusDbContext CreateFreshContext() =>
        new(new DbContextOptionsBuilder<DenariusDbContext>()
            .UseSqlite(_connection)
            .Options);

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
