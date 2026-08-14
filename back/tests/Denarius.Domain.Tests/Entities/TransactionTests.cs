using Denarius.Domain.Entities;
using Denarius.Domain.Exceptions;

namespace Denarius.Domain.Tests.Entities;

public class TransactionTests
{
    private static DateTime ValidDate => new(2026, 8, 14, 0, 0, 0, DateTimeKind.Utc);
    private static Guid ValidCategoryId => Guid.NewGuid();

    [Fact]
    public void Constructor_ValidFields_CreatesTransaction()
    {
        var categoryId = ValidCategoryId;

        var transaction = new Transaction("Mercado", ValidDate, 150.75m, categoryId);

        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal("Mercado", transaction.Description);
        Assert.Equal(ValidDate, transaction.Date);
        Assert.Equal(150.75m, transaction.Value);
        Assert.Equal(categoryId, transaction.CategoryId);
        Assert.True((transaction.UpdatedAt - transaction.CreatedAt).Duration() < TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_DescriptionWithSurroundingWhitespace_TrimsDescription()
    {
        var transaction = new Transaction("  Mercado  ", ValidDate, 150.75m, ValidCategoryId);

        Assert.Equal("Mercado", transaction.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyOrWhitespaceDescription_NormalizesToNull(string? description)
    {
        var transaction = new Transaction(description, ValidDate, 150.75m, ValidCategoryId);

        Assert.Null(transaction.Description);
    }

    [Fact]
    public void Constructor_DescriptionLongerThanMaxLength_ThrowsDomainException()
    {
        var description = new string('a', Transaction.DescriptionMaxLength + 1);

        Assert.Throws<DomainException>(() => new Transaction(description, ValidDate, 150.75m, ValidCategoryId));
    }

    [Fact]
    public void Constructor_DefaultDate_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new Transaction("Mercado", default, 150.75m, ValidCategoryId));
    }

    [Fact]
    public void Constructor_ZeroValue_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new Transaction("Mercado", ValidDate, 0m, ValidCategoryId));
    }

    [Fact]
    public void Constructor_NegativeValue_CreatesTransaction()
    {
        var transaction = new Transaction("Mercado", ValidDate, -50m, ValidCategoryId);

        Assert.Equal(-50m, transaction.Value);
    }

    [Fact]
    public void Constructor_EmptyCategoryId_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new Transaction("Mercado", ValidDate, 150.75m, Guid.Empty));
    }

    [Fact]
    public void Update_ValidFields_UpdatesFieldsAndTimestamp()
    {
        var transaction = new Transaction("Mercado", ValidDate, 150.75m, ValidCategoryId);
        var newDate = ValidDate.AddDays(1);
        var newCategoryId = Guid.NewGuid();

        transaction.Update("Farmácia", newDate, 42.5m, newCategoryId);

        Assert.Equal("Farmácia", transaction.Description);
        Assert.Equal(newDate, transaction.Date);
        Assert.Equal(42.5m, transaction.Value);
        Assert.Equal(newCategoryId, transaction.CategoryId);
        Assert.True(transaction.UpdatedAt >= transaction.CreatedAt);
    }

    [Fact]
    public void Update_ZeroValue_ThrowsDomainExceptionAndKeepsOriginalState()
    {
        var transaction = new Transaction("Mercado", ValidDate, 150.75m, ValidCategoryId);

        Assert.Throws<DomainException>(() => transaction.Update("Mercado", ValidDate, 0m, ValidCategoryId));
        Assert.Equal(150.75m, transaction.Value);
    }

    [Fact]
    public void Update_EmptyCategoryId_ThrowsDomainExceptionAndKeepsOriginalState()
    {
        var categoryId = ValidCategoryId;
        var transaction = new Transaction("Mercado", ValidDate, 150.75m, categoryId);

        Assert.Throws<DomainException>(() => transaction.Update("Mercado", ValidDate, 150.75m, Guid.Empty));
        Assert.Equal(categoryId, transaction.CategoryId);
    }
}
