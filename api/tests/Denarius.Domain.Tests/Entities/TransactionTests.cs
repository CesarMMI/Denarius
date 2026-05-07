using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Domain.Exceptions.Transactions;

namespace Denarius.Domain.Tests.Entities;

public class TransactionTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid AccountId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid TransferPeerId = Guid.NewGuid();

    private static Transaction Income(decimal amount = 1000m, string description = "Salário") =>
        new(UserId, AccountId, CategoryId, null, TransactionType.Income, amount, description, DateTime.UtcNow);

    private static Transaction Expense(decimal amount = 50m, string description = "Almoço") =>
        new(UserId, AccountId, CategoryId, null, TransactionType.Expense, amount, description, DateTime.UtcNow);

    private static Transaction Transfer(decimal amount = 200m, string description = "Transferência") =>
        new(UserId, AccountId, null, TransferPeerId, TransactionType.Transfer, amount, description, DateTime.UtcNow);

    // -------------------------------------------------------------------------
    // Creation — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_IncomeWithValidCategory_SetsPropertiesCorrectly()
    {
        var transaction = Income();

        Assert.Equal(UserId, transaction.UserId);
        Assert.Equal(AccountId, transaction.AccountId);
        Assert.Equal(CategoryId, transaction.CategoryId);
        Assert.Null(transaction.TransferPeerId);
        Assert.Equal(TransactionType.Income, transaction.Type);
        Assert.Equal(1000m, transaction.Amount);
        Assert.NotEqual(Guid.Empty, transaction.Id);
    }

    [Fact]
    public void Create_ExpenseWithValidCategory_SetsPropertiesCorrectly()
    {
        var transaction = Expense();

        Assert.Equal(TransactionType.Expense, transaction.Type);
        Assert.Equal(CategoryId, transaction.CategoryId);
        Assert.Null(transaction.TransferPeerId);
    }

    [Fact]
    public void Create_Transfer_HasTransferPeerIdAndNoCategory()
    {
        var transaction = Transfer();

        Assert.Equal(TransactionType.Transfer, transaction.Type);
        Assert.Equal(TransferPeerId, transaction.TransferPeerId);
        Assert.Null(transaction.CategoryId);
    }

    [Fact]
    public void Create_WithFutureDate_Succeeds()
    {
        var futureDate = DateTime.UtcNow.AddDays(30);

        var exception = Record.Exception(() =>
            new Transaction(UserId, AccountId, CategoryId, null, TransactionType.Income, 100m, "Salário futuro", futureDate));

        Assert.Null(exception);
    }

    // -------------------------------------------------------------------------
    // Creation — errors
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithZeroOrNegativeAmount_Throws(decimal amount)
    {
        Assert.Throws<InvalidAmountException>(() =>
            new Transaction(UserId, AccountId, CategoryId, null, TransactionType.Income, amount, "Test", DateTime.UtcNow));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidDescription_Throws(string? description)
    {
        Assert.Throws<InvalidDescriptionException>(() =>
            new Transaction(UserId, AccountId, CategoryId, null, TransactionType.Income, 100m, description!, DateTime.UtcNow));
    }

    [Theory]
    [InlineData(TransactionType.Income)]
    [InlineData(TransactionType.Expense)]
    public void Create_IncomeOrExpenseWithoutCategory_Throws(TransactionType type)
    {
        Assert.Throws<InvalidCategoryException>(() =>
            new Transaction(UserId, AccountId, null, null, type, 100m, "Test", DateTime.UtcNow));
    }

    [Fact]
    public void Create_TransferWithCategory_Throws()
    {
        Assert.Throws<InvalidCategoryException>(() =>
            new Transaction(UserId, AccountId, CategoryId, TransferPeerId, TransactionType.Transfer, 100m, "Test", DateTime.UtcNow));
    }

    [Fact]
    public void Create_TransferWithoutTransferPeerId_Throws()
    {
        Assert.Throws<InvalidTransferException>(() =>
            new Transaction(UserId, AccountId, null, null, TransactionType.Transfer, 100m, "Test", DateTime.UtcNow));
    }

    // -------------------------------------------------------------------------
    // ToDelta
    // -------------------------------------------------------------------------

    [Fact]
    public void ToDelta_Income_ReturnsPositiveAmount()
    {
        var transaction = Income(500m);

        Assert.Equal(500m, transaction.ToDelta());
    }

    [Fact]
    public void ToDelta_Expense_ReturnsNegativeAmount()
    {
        var transaction = Expense(300m);

        Assert.Equal(-300m, transaction.ToDelta());
    }

    [Fact]
    public void ToDelta_Transfer_ReturnsNegativeAmount()
    {
        var transaction = Transfer(150m);

        Assert.Equal(-150m, transaction.ToDelta());
    }

    // -------------------------------------------------------------------------
    // IsTransfer
    // -------------------------------------------------------------------------

    [Fact]
    public void IsTransfer_ForTransfer_ReturnsTrue()
    {
        Assert.True(Transfer().IsTransfer);
    }

    [Fact]
    public void IsTransfer_ForIncome_ReturnsFalse()
    {
        Assert.False(Income().IsTransfer);
    }

    [Fact]
    public void IsTransfer_ForExpense_ReturnsFalse()
    {
        Assert.False(Expense().IsTransfer);
    }

    // -------------------------------------------------------------------------
    // UpdateAmount
    // -------------------------------------------------------------------------

    [Fact]
    public void UpdateAmount_WithValidAmount_UpdatesAmount()
    {
        var transaction = Income(100m);

        transaction.UpdateAmount(250m);

        Assert.Equal(250m, transaction.Amount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateAmount_WithZeroOrNegative_Throws(decimal amount)
    {
        var transaction = Income();

        Assert.Throws<InvalidAmountException>(() => transaction.UpdateAmount(amount));
    }

    // -------------------------------------------------------------------------
    // UpdateDescription
    // -------------------------------------------------------------------------

    [Fact]
    public void UpdateDescription_WithValidDescription_UpdatesDescription()
    {
        var transaction = Income();

        transaction.UpdateDescription("Bônus anual");

        Assert.Equal("Bônus anual", transaction.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateDescription_WithInvalidDescription_Throws(string? description)
    {
        var transaction = Income();

        Assert.Throws<InvalidDescriptionException>(() => transaction.UpdateDescription(description!));
    }

    // -------------------------------------------------------------------------
    // UpdateCategory
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(TransactionType.Income)]
    [InlineData(TransactionType.Expense)]
    public void UpdateCategory_OnIncomeOrExpense_UpdatesCategory(TransactionType type)
    {
        var transaction = new Transaction(UserId, AccountId, CategoryId, null, type, 100m, "Test", DateTime.UtcNow);
        var newCategoryId = Guid.NewGuid();

        transaction.UpdateCategory(newCategoryId);

        Assert.Equal(newCategoryId, transaction.CategoryId);
    }

    [Fact]
    public void UpdateCategory_OnTransfer_Throws()
    {
        var transaction = Transfer();

        Assert.Throws<InvalidCategoryException>(() => transaction.UpdateCategory(Guid.NewGuid()));
    }

    [Theory]
    [InlineData(TransactionType.Income)]
    [InlineData(TransactionType.Expense)]
    public void UpdateCategory_WithNull_ThrowsOnIncomeOrExpense(TransactionType type)
    {
        var transaction = new Transaction(UserId, AccountId, CategoryId, null, type, 100m, "Test", DateTime.UtcNow);

        Assert.Throws<InvalidCategoryException>(() => transaction.UpdateCategory(null));
    }
}
