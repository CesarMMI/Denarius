using Denarius.Application.Accounts.Commands.Create;
using Denarius.Application.Shared.Exceptions;

namespace Denarius.Tests.Application.Accounts.Commands.Create;

public class CreateAccountQueryTests
{
    [Fact]
    public void Validate_NotThrows_WhenValid()
    {
        // Arrange
        var query = new CreateAccountQuery
        {
            UserId = 1,
            Name = "Test Account",
            Color = "#FFFFFF"
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
        var query = new CreateAccountQuery
        {
            UserId = 1,
            Name = name,
            Color = "#FFFFFF"
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Name is required", ex.Message);
    }

    [Fact]
    public void Validate_ThrowsBadRequest_WhenNameIsTooShort()
    {
        // Arrange
        var query = new CreateAccountQuery
        {
            UserId = 1,
            Name = "ab",
            Color = "#FFFFFF"
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Name length can't be lower than 3", ex.Message);
    }

    [Fact]
    public void Validate_ThrowsBadRequest_WhenNameIsTooLong()
    {
        // Arrange
        var query = new CreateAccountQuery
        {
            UserId = 1,
            Name = new('a', 51),
            Color = "#FFFFFF"
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
        var query = new CreateAccountQuery
        {
            UserId = 1,
            Name = "Test Account",
            Color = color
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Invalid color", ex.Message);
    }
}
