using Denarius.Application.Accounts.Commands.Delete;
using Denarius.Domain.Exceptions;

namespace Denarius.Tests.Application.Accounts.Commands.Delete;

public class DeleteAccountQueryTests
{
    [Fact]
    public void Validate_NotThrows_WhenValid()
    {
        // Arrange
        var query = new DeleteAccountQuery
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
        var query = new DeleteAccountQuery
        {
            UserId = 1,
            Id = accountId
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Account id is required", ex.Message);
    }
}
