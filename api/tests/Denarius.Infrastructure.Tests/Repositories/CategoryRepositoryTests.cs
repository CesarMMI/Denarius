using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Infrastructure.Persistence.Repositories;
using Denarius.Infrastructure.Tests.Shared;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Tests.Repositories;

public class CategoryRepositoryTests : RepositoryTestBase
{
    private readonly CategoryRepository _sut;

    public CategoryRepositoryTests()
    {
        _sut = new CategoryRepository(Context);
    }

    private static Category ValidCategory(Guid userId, string name = "Alimentação", CategoryType type = CategoryType.Expense) =>
        new(userId, name, "#FF0000", type);

    private async Task<Account> SeedAccountAsync(Guid userId)
    {
        var account = new Account(userId, "Test Account", "BRL", "#000000");
        await Context.Accounts.AddAsync(account);
        await Context.SaveChangesAsync();
        return account;
    }

    private async Task<Transaction> SeedTransactionAsync(Guid userId, Guid accountId, Guid categoryId)
    {
        var transaction = new Transaction(userId, accountId, categoryId, null, TransactionType.Expense, 100m, "Test", DateTime.UtcNow);
        await Context.Transactions.AddAsync(transaction);
        await Context.SaveChangesAsync();
        return transaction;
    }

    // -------------------------------------------------------------------------
    // AddAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAsync_WithValidCategory_PersistsCategory()
    {
        var category = ValidCategory(Guid.NewGuid());

        await _sut.AddAsync(category);
        await Context.SaveChangesAsync();

        using var freshContext = CreateFreshContext();
        var persisted = await freshContext.Categories.FindAsync(category.Id);
        Assert.NotNull(persisted);
        Assert.Equal(category.Name, persisted.Name);
        Assert.Equal(category.Color, persisted.Color);
        Assert.Equal(category.Type, persisted.Type);
    }

    // -------------------------------------------------------------------------
    // GetByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_WithExistingCategory_ReturnsCategory()
    {
        var category = ValidCategory(Guid.NewGuid());
        await Context.Categories.AddAsync(category);
        await Context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(category.Id, category.UserId);

        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
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
        var category = ValidCategory(Guid.NewGuid());
        await Context.Categories.AddAsync(category);
        await Context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(category.Id, Guid.NewGuid());

        Assert.Null(result);
    }

    // -------------------------------------------------------------------------
    // ListByUserAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ListByUserAsync_WithoutTypeFilter_ReturnsAllUserCategories()
    {
        var userId = Guid.NewGuid();
        await Context.Categories.AddRangeAsync(
            ValidCategory(userId, "Salário", CategoryType.Income),
            ValidCategory(userId, "Alimentação", CategoryType.Expense));
        await Context.SaveChangesAsync();

        var result = await _sut.ListByUserAsync(userId, null);

        Assert.Equal(2, result.Count());
    }

    [Theory]
    [InlineData(CategoryType.Income)]
    [InlineData(CategoryType.Expense)]
    public async Task ListByUserAsync_WithTypeFilter_ReturnsOnlyMatchingCategories(CategoryType type)
    {
        var userId = Guid.NewGuid();
        await Context.Categories.AddRangeAsync(
            ValidCategory(userId, "Salário", CategoryType.Income),
            ValidCategory(userId, "Alimentação", CategoryType.Expense));
        await Context.SaveChangesAsync();

        var result = await _sut.ListByUserAsync(userId, type);

        Assert.Single(result);
        Assert.All(result, c => Assert.Equal(type, c.Type));
    }

    [Fact]
    public async Task ListByUserAsync_DoesNotReturnOtherUsersCategories()
    {
        var userId = Guid.NewGuid();
        await Context.Categories.AddRangeAsync(
            ValidCategory(userId),
            ValidCategory(Guid.NewGuid()));
        await Context.SaveChangesAsync();

        var result = await _sut.ListByUserAsync(userId, null);

        Assert.All(result, c => Assert.Equal(userId, c.UserId));
    }

    [Fact]
    public async Task ListByUserAsync_WithNoCategories_ReturnsEmptyList()
    {
        var result = await _sut.ListByUserAsync(Guid.NewGuid(), null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ListByUserAsync_ReturnsOrderedByName()
    {
        var userId = Guid.NewGuid();
        await Context.Categories.AddRangeAsync(
            ValidCategory(userId, "Transporte"),
            ValidCategory(userId, "Alimentação"),
            ValidCategory(userId, "Saúde"));
        await Context.SaveChangesAsync();

        var result = (await _sut.ListByUserAsync(userId, null)).ToList();

        Assert.Equal("Alimentação", result[0].Name);
        Assert.Equal("Saúde", result[1].Name);
        Assert.Equal("Transporte", result[2].Name);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_WithModifiedCategory_PersistsChanges()
    {
        var category = ValidCategory(Guid.NewGuid());
        await Context.Categories.AddAsync(category);
        await Context.SaveChangesAsync();

        category.Update("New Name", "#00FF00");
        await _sut.UpdateAsync(category);
        await Context.SaveChangesAsync();

        using var freshContext = CreateFreshContext();
        var persisted = await freshContext.Categories.FindAsync(category.Id);
        Assert.Equal("New Name", persisted!.Name);
        Assert.Equal("#00FF00", persisted.Color);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_WithExistingCategory_RemovesIt()
    {
        var category = ValidCategory(Guid.NewGuid());
        await Context.Categories.AddAsync(category);
        await Context.SaveChangesAsync();

        await _sut.DeleteAsync(category);
        await Context.SaveChangesAsync();

        using var freshContext = CreateFreshContext();
        Assert.Null(await freshContext.Categories.FindAsync(category.Id));
    }

    // -------------------------------------------------------------------------
    // NullifyTransactionCategoriesAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task NullifyTransactionCategoriesAsync_SetsCategoryIdToNull()
    {
        var userId = Guid.NewGuid();
        var account = await SeedAccountAsync(userId);
        var category = ValidCategory(userId);
        await Context.Categories.AddAsync(category);
        await Context.SaveChangesAsync();
        var transaction = await SeedTransactionAsync(userId, account.Id, category.Id);

        await _sut.NullifyTransactionCategoriesAsync(category.Id);

        // AnyAsync avoids materializing the entity (which would fail constructor
        // validation since CategoryId is now null for an Expense transaction).
        using var freshContext = CreateFreshContext();
        var stillHasCategory = await freshContext.Transactions
            .Where(t => t.Id == transaction.Id && t.CategoryId != null)
            .AnyAsync();
        Assert.False(stillHasCategory);
    }

    [Fact]
    public async Task NullifyTransactionCategoriesAsync_DoesNotAffectOtherCategoryTransactions()
    {
        var userId = Guid.NewGuid();
        var account = await SeedAccountAsync(userId);
        var category1 = ValidCategory(userId, "Alimentação");
        var category2 = ValidCategory(userId, "Transporte");
        await Context.Categories.AddRangeAsync(category1, category2);
        await Context.SaveChangesAsync();
        await SeedTransactionAsync(userId, account.Id, category1.Id);
        var transaction2 = await SeedTransactionAsync(userId, account.Id, category2.Id);

        await _sut.NullifyTransactionCategoriesAsync(category1.Id);

        using var freshContext = CreateFreshContext();
        var category2TransactionUnchanged = await freshContext.Transactions
            .Where(t => t.Id == transaction2.Id && t.CategoryId == category2.Id)
            .AnyAsync();
        Assert.True(category2TransactionUnchanged);
    }
}
