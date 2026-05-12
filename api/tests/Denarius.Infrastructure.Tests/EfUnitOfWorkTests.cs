using Denarius.Domain.Entities;
using Denarius.Infrastructure.Persistence;
using Denarius.Infrastructure.Tests.Shared;

namespace Denarius.Infrastructure.Tests;

public class EfUnitOfWorkTests : RepositoryTestBase
{
    private readonly EfUnitOfWork _sut;

    public EfUnitOfWorkTests()
    {
        _sut = new EfUnitOfWork(Context);
    }

    // -------------------------------------------------------------------------
    // CommitAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CommitAsync_WithStagedChanges_PersistsToDatabase()
    {
        var account = new Account(Guid.NewGuid(), "Test", "BRL", "#000000");
        await Context.Accounts.AddAsync(account);

        await _sut.CommitAsync();

        using var freshContext = CreateFreshContext();
        Assert.NotNull(await freshContext.Accounts.FindAsync(account.Id));
    }

    [Fact]
    public async Task CommitAsync_WithNoStagedChanges_DoesNotThrow()
    {
        var exception = await Record.ExceptionAsync(() => _sut.CommitAsync());

        Assert.Null(exception);
    }
}
