using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Infrastructure.Persistence.Repositories;
using Denarius.Infrastructure.Tests.Shared;

namespace Denarius.Infrastructure.Tests.Repositories;

public class TransactionRepositoryTests : RepositoryTestBase
{
    private readonly TransactionRepository _sut;

    public TransactionRepositoryTests()
    {
        _sut = new TransactionRepository(Context);
    }

    private async Task<Account> SeedAccountAsync(Guid userId)
    {
        var account = new Account(userId, "Test Account", "BRL", "#000000");
        await Context.Accounts.AddAsync(account);
        await Context.SaveChangesAsync();
        return account;
    }

    private async Task<Category> SeedCategoryAsync(Guid userId, CategoryType type = CategoryType.Expense)
    {
        var category = new Category(userId, "Test Category", "#FF0000", type);
        await Context.Categories.AddAsync(category);
        await Context.SaveChangesAsync();
        return category;
    }

    private static Transaction ExpenseTransaction(Guid userId, Guid accountId, Guid categoryId, DateTime? date = null) =>
        new(userId, accountId, categoryId, null, TransactionType.Expense, 100m, "Test expense", date ?? DateTime.UtcNow);

    private static Transaction IncomeTransaction(Guid userId, Guid accountId, Guid categoryId, DateTime? date = null) =>
        new(userId, accountId, categoryId, null, TransactionType.Income, 200m, "Test income", date ?? DateTime.UtcNow);

    private static Transaction TransferTransaction(Guid userId, Guid accountId, Guid peerId, DateTime? date = null) =>
        new(userId, accountId, null, peerId, TransactionType.Transfer, 300m, "Test transfer", date ?? DateTime.UtcNow);

    // -------------------------------------------------------------------------
    // AddAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAsync_WithValidTransaction_PersistsTransaction()
    {
        var userId = Guid.NewGuid();
        var account = await SeedAccountAsync(userId);
        var category = await SeedCategoryAsync(userId);
        var transaction = ExpenseTransaction(userId, account.Id, category.Id);

        await _sut.AddAsync(transaction);
        await Context.SaveChangesAsync();

        using var freshContext = CreateFreshContext();
        var persisted = await freshContext.Transactions.FindAsync(transaction.Id);
        Assert.NotNull(persisted);
        Assert.Equal(transaction.Amount, persisted.Amount);
        Assert.Equal(transaction.Description, persisted.Description);
        Assert.Equal(transaction.Type, persisted.Type);
        Assert.Equal(transaction.CategoryId, persisted.CategoryId);
    }

    // -------------------------------------------------------------------------
    // AddRangeAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddRangeAsync_WithMultipleTransactions_PersistsAll()
    {
        var userId = Guid.NewGuid();
        var source = await SeedAccountAsync(userId);
        var destination = await SeedAccountAsync(userId);
        var outgoing = TransferTransaction(userId, source.Id, Guid.NewGuid());
        var incoming = TransferTransaction(userId, destination.Id, Guid.NewGuid());

        await _sut.AddRangeAsync([outgoing, incoming]);
        await Context.SaveChangesAsync();

        using var freshContext = CreateFreshContext();
        Assert.NotNull(await freshContext.Transactions.FindAsync(outgoing.Id));
        Assert.NotNull(await freshContext.Transactions.FindAsync(incoming.Id));
    }

    // -------------------------------------------------------------------------
    // GetByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_WithExistingTransaction_ReturnsTransaction()
    {
        var userId = Guid.NewGuid();
        var account = await SeedAccountAsync(userId);
        var category = await SeedCategoryAsync(userId);
        var transaction = ExpenseTransaction(userId, account.Id, category.Id);
        await Context.Transactions.AddAsync(transaction);
        await Context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(transaction.Id, userId);

        Assert.NotNull(result);
        Assert.Equal(transaction.Id, result.Id);
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
        var userId = Guid.NewGuid();
        var account = await SeedAccountAsync(userId);
        var category = await SeedCategoryAsync(userId);
        var transaction = ExpenseTransaction(userId, account.Id, category.Id);
        await Context.Transactions.AddAsync(transaction);
        await Context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(transaction.Id, Guid.NewGuid());

        Assert.Null(result);
    }

    // -------------------------------------------------------------------------
    // ListByUserAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ListByUserAsync_WithNoFilters_ReturnsAllUserTransactions()
    {
        var userId = Guid.NewGuid();
        var account = await SeedAccountAsync(userId);
        var category = await SeedCategoryAsync(userId);
        await Context.Transactions.AddRangeAsync(
            ExpenseTransaction(userId, account.Id, category.Id),
            ExpenseTransaction(userId, account.Id, category.Id));
        await Context.SaveChangesAsync();

        var result = await _sut.ListByUserAsync(userId, null, null, null, null, null);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ListByUserAsync_FiltersByAccountId()
    {
        var userId = Guid.NewGuid();
        var account1 = await SeedAccountAsync(userId);
        var account2 = await SeedAccountAsync(userId);
        var category = await SeedCategoryAsync(userId);
        await Context.Transactions.AddRangeAsync(
            ExpenseTransaction(userId, account1.Id, category.Id),
            ExpenseTransaction(userId, account2.Id, category.Id));
        await Context.SaveChangesAsync();

        var result = await _sut.ListByUserAsync(userId, account1.Id, null, null, null, null);

        Assert.Single(result);
        Assert.All(result, t => Assert.Equal(account1.Id, t.AccountId));
    }

    [Fact]
    public async Task ListByUserAsync_FiltersByCategoryId()
    {
        var userId = Guid.NewGuid();
        var account = await SeedAccountAsync(userId);
        var category1 = await SeedCategoryAsync(userId);
        var category2 = await SeedCategoryAsync(userId);
        await Context.Transactions.AddRangeAsync(
            ExpenseTransaction(userId, account.Id, category1.Id),
            ExpenseTransaction(userId, account.Id, category2.Id));
        await Context.SaveChangesAsync();

        var result = await _sut.ListByUserAsync(userId, null, category1.Id, null, null, null);

        Assert.Single(result);
        Assert.All(result, t => Assert.Equal(category1.Id, t.CategoryId));
    }

    [Fact]
    public async Task ListByUserAsync_FiltersByType()
    {
        var userId = Guid.NewGuid();
        var account = await SeedAccountAsync(userId);
        var expenseCategory = await SeedCategoryAsync(userId, CategoryType.Expense);
        var incomeCategory = await SeedCategoryAsync(userId, CategoryType.Income);
        await Context.Transactions.AddRangeAsync(
            ExpenseTransaction(userId, account.Id, expenseCategory.Id),
            IncomeTransaction(userId, account.Id, incomeCategory.Id));
        await Context.SaveChangesAsync();

        var result = await _sut.ListByUserAsync(userId, null, null, TransactionType.Expense, null, null);

        Assert.Single(result);
        Assert.All(result, t => Assert.Equal(TransactionType.Expense, t.Type));
    }

    [Fact]
    public async Task ListByUserAsync_FiltersByStartDate()
    {
        var userId = Guid.NewGuid();
        var account = await SeedAccountAsync(userId);
        var category = await SeedCategoryAsync(userId);
        var today = DateTime.UtcNow.Date;
        await Context.Transactions.AddRangeAsync(
            ExpenseTransaction(userId, account.Id, category.Id, today),
            ExpenseTransaction(userId, account.Id, category.Id, today.AddDays(-1)));
        await Context.SaveChangesAsync();

        var result = await _sut.ListByUserAsync(userId, null, null, null, today, null);

        Assert.Single(result);
        Assert.All(result, t => Assert.True(t.Date >= today));
    }

    [Fact]
    public async Task ListByUserAsync_FiltersByEndDate()
    {
        var userId = Guid.NewGuid();
        var account = await SeedAccountAsync(userId);
        var category = await SeedCategoryAsync(userId);
        var today = DateTime.UtcNow.Date;
        await Context.Transactions.AddRangeAsync(
            ExpenseTransaction(userId, account.Id, category.Id, today),
            ExpenseTransaction(userId, account.Id, category.Id, today.AddDays(1)));
        await Context.SaveChangesAsync();

        var result = await _sut.ListByUserAsync(userId, null, null, null, null, today);

        Assert.Single(result);
        Assert.All(result, t => Assert.True(t.Date <= today));
    }

    [Fact]
    public async Task ListByUserAsync_DoesNotReturnOtherUsersTransactions()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var account = await SeedAccountAsync(userId);
        var otherAccount = await SeedAccountAsync(otherUserId);
        var category = await SeedCategoryAsync(userId);
        var otherCategory = await SeedCategoryAsync(otherUserId);
        await Context.Transactions.AddRangeAsync(
            ExpenseTransaction(userId, account.Id, category.Id),
            ExpenseTransaction(otherUserId, otherAccount.Id, otherCategory.Id));
        await Context.SaveChangesAsync();

        var result = await _sut.ListByUserAsync(userId, null, null, null, null, null);

        Assert.Single(result);
        Assert.All(result, t => Assert.Equal(userId, t.UserId));
    }

    [Fact]
    public async Task ListByUserAsync_WithNoMatches_ReturnsEmptyList()
    {
        var result = await _sut.ListByUserAsync(Guid.NewGuid(), null, null, null, null, null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ListByUserAsync_ReturnsOrderedByDateDescending()
    {
        var userId = Guid.NewGuid();
        var account = await SeedAccountAsync(userId);
        var category = await SeedCategoryAsync(userId);
        var today = DateTime.UtcNow.Date;
        await Context.Transactions.AddRangeAsync(
            ExpenseTransaction(userId, account.Id, category.Id, today),
            ExpenseTransaction(userId, account.Id, category.Id, today.AddDays(-1)),
            ExpenseTransaction(userId, account.Id, category.Id, today.AddDays(1)));
        await Context.SaveChangesAsync();

        var result = (await _sut.ListByUserAsync(userId, null, null, null, null, null)).ToList();

        Assert.True(result[0].Date >= result[1].Date);
        Assert.True(result[1].Date >= result[2].Date);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_WithModifiedTransaction_PersistsChanges()
    {
        var userId = Guid.NewGuid();
        var account = await SeedAccountAsync(userId);
        var category = await SeedCategoryAsync(userId);
        var transaction = ExpenseTransaction(userId, account.Id, category.Id);
        await Context.Transactions.AddAsync(transaction);
        await Context.SaveChangesAsync();

        transaction.UpdateAmount(500m);
        transaction.UpdateDescription("Updated description");
        await _sut.UpdateAsync(transaction);
        await Context.SaveChangesAsync();

        using var freshContext = CreateFreshContext();
        var persisted = await freshContext.Transactions.FindAsync(transaction.Id);
        Assert.Equal(500m, persisted!.Amount);
        Assert.Equal("Updated description", persisted.Description);
    }

    // -------------------------------------------------------------------------
    // UpdateRangeAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateRangeAsync_WithMultipleTransactions_PersistsChanges()
    {
        var userId = Guid.NewGuid();
        var account = await SeedAccountAsync(userId);
        var category = await SeedCategoryAsync(userId);
        var t1 = ExpenseTransaction(userId, account.Id, category.Id);
        var t2 = ExpenseTransaction(userId, account.Id, category.Id);
        await Context.Transactions.AddRangeAsync(t1, t2);
        await Context.SaveChangesAsync();

        t1.UpdateAmount(111m);
        t2.UpdateAmount(222m);
        await _sut.UpdateRangeAsync([t1, t2]);
        await Context.SaveChangesAsync();

        using var freshContext = CreateFreshContext();
        Assert.Equal(111m, (await freshContext.Transactions.FindAsync(t1.Id))!.Amount);
        Assert.Equal(222m, (await freshContext.Transactions.FindAsync(t2.Id))!.Amount);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_WithExistingTransaction_RemovesIt()
    {
        var userId = Guid.NewGuid();
        var account = await SeedAccountAsync(userId);
        var category = await SeedCategoryAsync(userId);
        var transaction = ExpenseTransaction(userId, account.Id, category.Id);
        await Context.Transactions.AddAsync(transaction);
        await Context.SaveChangesAsync();

        await _sut.DeleteAsync(transaction);
        await Context.SaveChangesAsync();

        using var freshContext = CreateFreshContext();
        Assert.Null(await freshContext.Transactions.FindAsync(transaction.Id));
    }

    // -------------------------------------------------------------------------
    // DeleteRangeAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteRangeAsync_WithMultipleTransactions_RemovesAll()
    {
        var userId = Guid.NewGuid();
        var account = await SeedAccountAsync(userId);
        var category = await SeedCategoryAsync(userId);
        var t1 = ExpenseTransaction(userId, account.Id, category.Id);
        var t2 = ExpenseTransaction(userId, account.Id, category.Id);
        await Context.Transactions.AddRangeAsync(t1, t2);
        await Context.SaveChangesAsync();

        await _sut.DeleteRangeAsync([t1, t2]);
        await Context.SaveChangesAsync();

        using var freshContext = CreateFreshContext();
        Assert.Null(await freshContext.Transactions.FindAsync(t1.Id));
        Assert.Null(await freshContext.Transactions.FindAsync(t2.Id));
    }
}
