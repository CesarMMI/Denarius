using Denarius.Application.Accounts.Commands.GetAll;

namespace Denarius.Tests.Application.Accounts.Commands.GetAll;

public class GetAllAccountsQueryTests
{
    [Fact]
    public void Validate_NotThrows_WhenValid()
    {
        // Arrange
        var query = new GetAllAccountsQuery
        {
            UserId = 1
        };
        // Act
        query.Validate();
        // Assert
        Assert.True(true);
    }
}
