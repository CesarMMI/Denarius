using Denarius.Application.Transactions.Commands.Delete;
using Denarius.Domain.Exceptions;

namespace Denarius.Tests.Application.Transactions.Commands.Delete;

public class DeleteTransactionQueryTests
{
    [Fact]
    public void Validate_NotThrows_WhenValid()
    {
        // Arrange
        var query = new DeleteTransactionQuery
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
    public void Validate_ThrowsBadRequest_WhenTransactionIdIsEmpty(int transactionId)
    {
        // Arrange
        var query = new DeleteTransactionQuery
        {
            UserId = 1,
            Id = transactionId
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Transaction id is required", ex.Message);
    }
}
