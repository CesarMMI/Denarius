using Denarius.Domain.Entities;
using Denarius.Infrastructure.Persistence.Repositories;
using Denarius.Infrastructure.Tests.Shared;

namespace Denarius.Infrastructure.Tests.Repositories;

public class AccountRepositoryTests : RepositoryTestBase
{
    private readonly AccountRepository _sut;

    public AccountRepositoryTests()
    {
        _sut = new AccountRepository(Context);
    }

    private static Account ValidAccount(Guid userId, string name = "Nubank") =>
        new(userId, name, "BRL", "#8A05BE");

    // -------------------------------------------------------------------------
    // AddAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAsync_WithValidAccount_PersistsAccount()
    {
        var account = ValidAccount(Guid.NewGuid());

        await _sut.AddAsync(account);
        await Context.SaveChangesAsync();

        using var freshContext = CreateFreshContext();
        var persisted = await freshContext.Accounts.FindAsync(account.Id);
        Assert.NotNull(persisted);
        Assert.Equal(account.Name, persisted.Name);
        Assert.Equal(account.CurrencyCode, persisted.CurrencyCode);
        Assert.Equal(account.Color, persisted.Color);
        Assert.Equal(0, persisted.Balance);
        Assert.True(persisted.IsActive);
    }

    // -------------------------------------------------------------------------
    // GetByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_WithExistingAccount_ReturnsAccount()
    {
        var account = ValidAccount(Guid.NewGuid());
        await Context.Accounts.AddAsync(account);
        await Context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(account.Id, account.UserId);

        Assert.NotNull(result);
        Assert.Equal(account.Id, result.Id);
        Assert.Equal(account.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithDifferentUserId_ReturnsNull()
    {
        var account = ValidAccount(Guid.NewGuid());
        await Context.Accounts.AddAsync(account);
        await Context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(account.Id, Guid.NewGuid());

        Assert.Null(result);
    }

    // -------------------------------------------------------------------------
    // ListByUserAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ListByUserAsync_WithExistingAccounts_ReturnsAllUserAccounts()
    {
        var userId = Guid.NewGuid();
        await Context.Accounts.AddRangeAsync(ValidAccount(userId, "Bradesco"), ValidAccount(userId, "Nubank"));
        await Context.SaveChangesAsync();

        var result = await _sut.ListByUserAsync(userId);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ListByUserAsync_DoesNotReturnOtherUsersAccounts()
    {
        var userId = Guid.NewGuid();
        await Context.Accounts.AddRangeAsync(ValidAccount(userId), ValidAccount(Guid.NewGuid()));
        await Context.SaveChangesAsync();

        var result = await _sut.ListByUserAsync(userId);

        Assert.All(result, a => Assert.Equal(userId, a.UserId));
    }

    [Fact]
    public async Task ListByUserAsync_WithNoAccounts_ReturnsEmptyList()
    {
        var result = await _sut.ListByUserAsync(Guid.NewGuid());

        Assert.Empty(result);
    }

    [Fact]
    public async Task ListByUserAsync_ReturnsAccountsOrderedByName()
    {
        var userId = Guid.NewGuid();
        await Context.Accounts.AddRangeAsync(
            ValidAccount(userId, "Nubank"),
            ValidAccount(userId, "Bradesco"),
            ValidAccount(userId, "Itaú"));
        await Context.SaveChangesAsync();

        var result = (await _sut.ListByUserAsync(userId)).ToList();

        Assert.Equal("Bradesco", result[0].Name);
        Assert.Equal("Itaú", result[1].Name);
        Assert.Equal("Nubank", result[2].Name);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_WithModifiedAccount_PersistsChanges()
    {
        var account = ValidAccount(Guid.NewGuid());
        await Context.Accounts.AddAsync(account);
        await Context.SaveChangesAsync();

        account.Update("Updated Name", "#FFFFFF");
        await _sut.UpdateAsync(account);
        await Context.SaveChangesAsync();

        using var freshContext = CreateFreshContext();
        var persisted = await freshContext.Accounts.FindAsync(account.Id);
        Assert.Equal("Updated Name", persisted!.Name);
        Assert.Equal("#FFFFFF", persisted.Color);
    }
}
