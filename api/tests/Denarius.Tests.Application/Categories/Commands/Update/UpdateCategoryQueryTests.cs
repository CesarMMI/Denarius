using Denarius.Application.Categories.Commands.Update;
using Denarius.Domain.Exceptions;

namespace Denarius.Tests.Application.Categories.Commands.Update;

public class UpdateCategoryQueryTests
{
    [Fact]
    public void Validate_NotThrows_WhenValid()
    {
        // Arrange
        var query = new UpdateCategoryQuery
        {
            Id = 1,
            Name = "Test Category",
            Color = "#FFFFFF",
            UserId = 1,
        };
        // Act
        query.Validate();
        // Assert
        Assert.True(true);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ThrowsBadRequest_WhenIdIsEmpty(int id)
    {
        // Arrange
        var query = new UpdateCategoryQuery
        {
            Id = id,
            Name = "Test Category",
            Color = "#FFFFFF",
            UserId = 1,
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Category id is required", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Validate_ThrowsBadRequest_WhenNameIsEmpty(string name)
    {
        // Arrange
        var query = new UpdateCategoryQuery
        {
            Id = 1,            
            Name = name,
            Color = "#FFFFFF",
            UserId = 1,
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Name is required", ex.Message);
    }

    [Fact]
    public void Validate_ThrowsBadRequest_WhenNameIsTooShort()
    {
        // Arrange
        var query = new UpdateCategoryQuery
        {
            Id = 1,
            Name = "ab",
            Color = "#FFFFFF",
            UserId = 1,
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Name length can't be lower than 3", ex.Message);
    }

    [Fact]
    public void Validate_ThrowsBadRequest_WhenNameIsTooLong()
    {
        // Arrange
        var query = new UpdateCategoryQuery
        {
            Id = 1,
            Name = new('a', 51),
            Color = "#FFFFFF",
            UserId = 1,
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
        var query = new UpdateCategoryQuery
        {
            Id = 1,
            Name = "Test Category",
            Color = color,
            UserId = 1,
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Invalid color", ex.Message);
    }
}
