using Denarius.Application.Categories.Commands.Create;
using Denarius.Application.Shared.Exceptions;
using Denarius.Domain.Enums;

namespace Denarius.Tests.Application.Categories.Commands.Create;

public class CreateCategoryQueryTests
{
    [Fact]
    public void Validate_NotThrows_WhenValid()
    {
        // Arrange
        var query = new CreateCategoryQuery
        {
            Name = "Test Category",
            Color = "#FFFFFF",
            Type = ECategoryType.Expense,
            UserId = 1
        };
        // Act
        query.Validate();
        // Assert
        Assert.True(true);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Validate_ThrowsBadRequest_WhenNameIsEmpty(string name)
    {
        // Arrange
        var query = new CreateCategoryQuery
        {
            Name = name,
            Color = "#FFFFFF",
            Type = ECategoryType.Expense,
            UserId = 1
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Name is required", ex.Message);
    }

    [Fact]
    public void Validate_ThrowsBadRequest_WhenNameIsTooShort()
    {
        // Arrange
        var query = new CreateCategoryQuery
        {
            Name = "ab",
            Color = "#FFFFFF",
            Type = ECategoryType.Expense,
            UserId = 1
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Name length can't be lower than 3", ex.Message);
    }

    [Fact]
    public void Validate_ThrowsBadRequest_WhenNameIsTooLong()
    {
        // Arrange
        var query = new CreateCategoryQuery
        {
            Name = new('a', 51),
            Color = "#FFFFFF",
            Type = ECategoryType.Expense,
            UserId = 1
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Name length can't be greater than 50", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("#F")]
    [InlineData("#FF")]
    [InlineData("#FFFF")]
    [InlineData("#FFFFF")]
    public void Validate_ThrowsBadRequest_WhenColorIsInvalid(string color)
    {
        // Arrange
        var query = new CreateCategoryQuery
        {
            Name = "Test Category",
            Color = color,
            Type = ECategoryType.Expense,
            UserId = 1,
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Invalid color", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(5)]
    public void Validate_ThrowsBadRequest_WhenTypeIsEmpty(ECategoryType type)
    {
        // Arrange
        var query = new CreateCategoryQuery
        {
            Name = "Test Category",
            Color = "#FFFFFF",
            Type = type,
            UserId = 1
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Invalid type", ex.Message);
    }
}
