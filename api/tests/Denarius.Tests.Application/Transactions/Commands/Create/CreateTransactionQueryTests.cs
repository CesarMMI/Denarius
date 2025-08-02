using Denarius.Application.Transactions.Commands.Create;
using Denarius.Domain.Exceptions;

namespace Denarius.Tests.Application.Transactions.Commands.Create;

public class CreateTransactionQueryTests
{
    [Fact]
    public void Validate_NotThrows_WhenValid()
    {
        // Arrange
        var query = new CreateTransactionQuery
        {
            UserId = 1,
            Amount = 100,
            Date = DateTime.Now,
            Description = "Test Transaction",
            AccountId = 1,
            CategoryId = 1
        };
        // Act
        query.Validate();
        // Assert
        Assert.True(true);
    }

    [Fact]
    public void Validate_ThrowsBadRequest_WhenAmountEqualsZero()
    {
        // Arrange
        var query = new CreateTransactionQuery
        {
            UserId = 1,
            Amount = 0,
            Date = DateTime.Now,
            Description = "Test Transaction",
            AccountId = 1,
            CategoryId = 1
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Amount can't be equal to 0", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Validate_ThrowsBadRequest_WhenDescriptionIsEmpty(string description)
    {
        // Arrange
        var query = new CreateTransactionQuery
        {
            UserId = 1,
            Amount = 100,
            Date = DateTime.Now,
            Description = description,
            AccountId = 1,
            CategoryId = 1
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Description is required", ex.Message);
    }
    
    [Fact]
    public void Validate_ThrowsBadRequest_WhenDescriptionIsTooShort()
    {
        // Arrange
        var query = new CreateTransactionQuery
        {
            UserId = 1,
            Amount = 100,
            Date = DateTime.Now,
            Description = "ab",
            AccountId = 1,
            CategoryId = 1
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Description length can't be lower than 3", ex.Message);
    }
    
    [Fact]
    public void Validate_ThrowsBadRequest_WhenDescriptionIsTooLong()
    {
        // Arrange
        var query = new CreateTransactionQuery
        {
            UserId = 1,
            Amount = 100,
            Date = DateTime.Now,
            Description = new('a', 51),
            AccountId = 1,
            CategoryId = 1
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Description length can't be greater than 50", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(-1)]
    [InlineData(0)]
    public void Validate_ThrowsBadRequest_WhenAccountIdIsInvalid(int accountId)
    {
        // Arrange
        var query = new CreateTransactionQuery
        {
            UserId = 1,
            Amount = 100,
            Date = DateTime.Now,
            Description = "Test Transaction",
            AccountId = accountId,
            CategoryId = 1
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Account id is required", ex.Message);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Validate_ThrowsBadRequest_WhenCategoryIdIsInvalid(int categoryId)
    {
        // Arrange
        var query = new CreateTransactionQuery
        {
            UserId = 1,
            Amount = 100,
            Date = DateTime.Now,
            Description = "Test Transaction",
            AccountId = 1,
            CategoryId = categoryId
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Invalid category id", ex.Message);
    }
}
