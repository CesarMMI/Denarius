using Denarius.Application.Accounts.Commands.GetById;
using Denarius.Application.Shared.Exceptions;

namespace Denarius.Tests.Application.Accounts.Commands.GetById;

public class GetAccountByIdQueryTests
{
    [Fact]
    public void Validate_NotThrows_WhenValid()
    {
        // Arrange
        var query = new GetAccountByIdQuery
        {
            UserId = 1,
            Id = 1
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
    public void Validate_ThrowsBadRequest_WhenAccountIdIsEmpty(int accountId)
    {
        // Arrange
        var query = new GetAccountByIdQuery
        {
            UserId = 1,
            Id = accountId
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Account id is required", ex.Message);
    }
}
