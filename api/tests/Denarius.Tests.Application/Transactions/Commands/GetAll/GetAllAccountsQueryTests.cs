using Denarius.Application.Transactions.Commands.GetAll;

namespace Denarius.Tests.Application.Transactions.Commands.GetAll;

public class GetAllTransactionsQueryTests
{
    [Fact]
    public void Validate_NotThrows_WhenValid()
    {
        // Arrange
        var query = new GetAllTransactionsQuery
        {
            UserId = 1
        };
        // Act
        query.Validate();
        // Assert
        Assert.True(true);
    }
}
