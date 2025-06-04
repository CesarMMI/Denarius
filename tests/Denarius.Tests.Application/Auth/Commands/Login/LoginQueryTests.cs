using Denarius.Application.Auth.Commands.Login;
using Denarius.Application.Shared.Exceptions;

namespace Denarius.Tests.Application.Auth.Commands.Login;

public class LoginQueryTests
{
    [Fact]
    public void Validate_NotThrows_WhenValid()
    {
        // Arrange
        var query = new LoginQuery
        {
            Email = "user@example.com",
            Password = "validPassword"
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
    public void Validate_ThrowsBadRequest_WhenEmailIsEmpty(string email)
    {
        // Arrange
        var query = new LoginQuery
        {
            Email = email,
            Password = "validPassword"
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Email is required", ex.Message);
    }

    [Theory]
    [InlineData("user")]
    [InlineData("user@")]
    [InlineData("user.com")]
    public void Validate_ThrowsBadRequest_WhenEmailIsInvalid(string email)
    {
        // Arrange
        var query = new LoginQuery
        {
            Email = email,
            Password = "validPassword"
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Invalid email", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Validate_ThrowsBadRequest_WhenPasswordIsEmpty(string password)
    {
        // Arrange
        var query = new LoginQuery
        {
            Email = "user@test.com",
            Password = password
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Password is required", ex.Message);
    }

    [Fact]
    public void Validate_ThrowsBadRequest_WhenPasswordIsTooShort()
    {
        // Arrange
        var query = new LoginQuery
        {
            Email = "user@test.com",
            Password = "123"
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Password length can't be lower than 5", ex.Message);
    }

    [Fact]
    public void Validate_ThrowsBadRequest_WhenPasswordIsTooLong()
    {
        // Arrange
        var query = new LoginQuery
        {
            Email = "user@test.com",
            Password = new('a', 101)
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Password length can't be greater than 100", ex.Message);
    }
}
