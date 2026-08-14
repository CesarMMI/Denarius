using Denarius.Domain.Entities;
using Denarius.Domain.Exceptions;
using Denarius.Domain.ValueObjects;

namespace Denarius.Domain.Tests.Entities;

public class CategoryTests
{
    private static Color ValidColor => new("#FF0000");

    [Fact]
    public void Constructor_ValidNameAndColor_CreatesCategory()
    {
        var category = new Category("Lazer", ValidColor);

        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal("Lazer", category.Name);
        Assert.Equal(ValidColor, category.Color);
        Assert.True((category.UpdatedAt - category.CreatedAt).Duration() < TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_NameWithSurroundingWhitespace_TrimsName()
    {
        var category = new Category("  Lazer  ", ValidColor);

        Assert.Equal("Lazer", category.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyOrWhitespaceName_ThrowsDomainException(string? name)
    {
        Assert.Throws<DomainException>(() => new Category(name!, ValidColor));
    }

    [Fact]
    public void Constructor_NameLongerThanMaxLength_ThrowsDomainException()
    {
        var name = new string('a', Category.NameMaxLength + 1);

        Assert.Throws<DomainException>(() => new Category(name, ValidColor));
    }

    [Fact]
    public void Update_ValidNameAndColor_UpdatesFieldsAndTimestamp()
    {
        var category = new Category("Lazer", ValidColor);
        var newColor = new Color("#00FF00");

        category.Update("Transporte", newColor);

        Assert.Equal("Transporte", category.Name);
        Assert.Equal(newColor, category.Color);
        Assert.True(category.UpdatedAt >= category.CreatedAt);
    }

    [Fact]
    public void Update_EmptyName_ThrowsDomainExceptionAndKeepsOriginalState()
    {
        var category = new Category("Lazer", ValidColor);

        Assert.Throws<DomainException>(() => category.Update("", ValidColor));
        Assert.Equal("Lazer", category.Name);
    }
}
