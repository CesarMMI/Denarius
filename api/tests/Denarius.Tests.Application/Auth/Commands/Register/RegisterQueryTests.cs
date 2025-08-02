using Denarius.Application.Auth.Commands.Register;
using Denarius.Domain.Exceptions;

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
    public void Validate_ThrowsBadRequest_WhenNameIsEmpty(string name)
    {
        // Arrange
        var query = new RegisterQuery
        {
            Name = name,
            Email = "valid@email.com",
            Password = "validPassword"
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Name is required", ex.Message);
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
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Name length can't be lower than 3", ex.Message);
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
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Name length can't be greater than 50", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Validate_ThrowsBadRequest_WhenEmailIsEmpty(string email)
    {
        // Arrange
        var query = new RegisterQuery
        {
            Name = "Valid Name",
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
        var query = new RegisterQuery
        {
            Name = "Valid Name",
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
        var query = new RegisterQuery
        {
            Name = "Valid Name",
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
        var query = new RegisterQuery
        {
            Name = "Valid Name",
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
        var query = new RegisterQuery
        {
            Name = "Valid Name",
            Email = "user@test.com",
            Password = new('a', 101)
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Password length can't be greater than 100", ex.Message);
    }
}
