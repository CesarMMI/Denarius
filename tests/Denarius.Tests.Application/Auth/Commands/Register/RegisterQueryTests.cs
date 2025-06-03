using Denarius.Application.Auth.Commands.Register;
using Denarius.Application.Shared.Exceptions;

namespace Denarius.Tests.Application.Auth.Commands.Register;

public class RegisterQueryTests
{
    [Fact]
    public void Validate_NotThrows_WhenValid()
    {
        // Arrange
        var query = new RegisterQuery
        {
            Name = "Valid Name",
            Email = "valid@email.com",
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
    public void Validate_ThrowsBadRequest_WhenNameIsInvalid(string name)
    {
        // Arrange
        var query = new RegisterQuery
        {
            Name = name,
            Email = "valid@email.com",
            Password = "validPassword"
        };
        // Act & Assert
        Assert.Throws<BadRequestException>(query.Validate);
    }

    [Fact]
    public void Validate_ThrowsBadRequest_WhenNameIsTooShort()
    {
        // Arrange
        var query = new RegisterQuery
        {
            Name = "ab",
            Email = "valid@email.com",
            Password = "validPassword"
        };
        // Act & Assert
        Assert.Throws<BadRequestException>(query.Validate);
    }

    [Fact]
    public void Validate_ThrowsBadRequest_WhenNameIsTooLong()
    {
        // Arrange
        var query = new RegisterQuery
        {
            Name = new('a', 51),
            Email = "valid@email.com",
            Password = "validPassword"
        };
        // Act & Assert
        Assert.Throws<BadRequestException>(query.Validate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("user")]
    [InlineData("user@")]
    [InlineData("user.com")]
    public void Validate_ThrowsBadRequest_WhenEmailIsInvalid(string email)
    {
        // Arrange
        var query = new RegisterQuery
        {
            Email = email,
            Password = "validPassword"
        };
        // Act & Assert
        Assert.Throws<BadRequestException>(query.Validate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Validate_ThrowsBadRequest_WhenPasswordIsEmpty(string password)
    {
        // Arrange
        var query = new RegisterQuery
        {
            Email = "user@test.com",
            Password = password
        };
        // Act & Assert
        Assert.Throws<BadRequestException>(query.Validate);
    }

    [Fact]
    public void Validate_ThrowsBadRequest_WhenPasswordIsTooShort()
    {
        // Arrange
        var query = new RegisterQuery
        {
            Email = "user@test.com",
            Password = "123"
        };
        // Act & Assert
        Assert.Throws<BadRequestException>(query.Validate);
    }

    [Fact]
    public void Validate_ThrowsBadRequest_WhenPasswordIsTooLong()
    {
        // Arrange
        var query = new RegisterQuery
        {
            Email = "user@test.com",
            Password = new('a', 101)
        };
        // Act & Assert
        Assert.Throws<BadRequestException>(query.Validate);
    }
}
