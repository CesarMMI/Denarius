using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Domain.Exceptions;

namespace Denarius.Domain.Tests.Entities;

public class CategoryTests
{
    private static Category ValidIncomeCategory() =>
        new(Guid.NewGuid(), "Salário", "#00FF00", CategoryType.Income);

    private static Category ValidExpenseCategory() =>
        new(Guid.NewGuid(), "Alimentação", "#FF0000", CategoryType.Expense);

    // -------------------------------------------------------------------------
    // Creation — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_WithValidFields_SetsPropertiesCorrectly()
    {
        var userId = Guid.NewGuid();
        var category = new Category(userId, "Salário", "#00FF00", CategoryType.Income);

        Assert.Equal(userId, category.UserId);
        Assert.Equal("Salário", category.Name);
        Assert.Equal("#00FF00", category.Color);
        Assert.Equal(CategoryType.Income, category.Type);
        Assert.NotEqual(Guid.Empty, category.Id);
    }

    // -------------------------------------------------------------------------
    // Creation — errors
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidName_Throws(string? name)
    {
        Assert.Throws<InvalidNameException>(() =>
            new Category(Guid.NewGuid(), name!, "#000000", CategoryType.Income));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidColor_Throws(string? color)
    {
        Assert.Throws<InvalidColorException>(() =>
            new Category(Guid.NewGuid(), "Test", color!, CategoryType.Income));
    }

    // -------------------------------------------------------------------------
    // AcceptsTransactionType
    // -------------------------------------------------------------------------

    [Fact]
    public void AcceptsTransactionType_IncomeCategory_AcceptsOnlyIncome()
    {
        var category = ValidIncomeCategory();

        Assert.True(category.AcceptsTransactionType(TransactionType.Income));
        Assert.False(category.AcceptsTransactionType(TransactionType.Expense));
        Assert.False(category.AcceptsTransactionType(TransactionType.Transfer));
    }

    [Fact]
    public void AcceptsTransactionType_ExpenseCategory_AcceptsOnlyExpense()
    {
        var category = ValidExpenseCategory();

        Assert.False(category.AcceptsTransactionType(TransactionType.Income));
        Assert.True(category.AcceptsTransactionType(TransactionType.Expense));
        Assert.False(category.AcceptsTransactionType(TransactionType.Transfer));
    }

    [Theory]
    [InlineData(CategoryType.Income)]
    [InlineData(CategoryType.Expense)]
    public void AcceptsTransactionType_AnyCategory_DoesNotAcceptTransfer(CategoryType categoryType)
    {
        var category = new Category(Guid.NewGuid(), "Test", "#000000", categoryType);

        Assert.False(category.AcceptsTransactionType(TransactionType.Transfer));
    }
}
