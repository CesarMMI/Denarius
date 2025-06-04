using Denarius.Application.Accounts.Commands.GetAll;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Moq;

namespace Denarius.Tests.Application.Accounts.Commands.GetAll;

public class GetAllAccountsCommandTests
{
    private static readonly Mock<IAccountRepository> accountRepositoryMock = new();

    [Fact]
    public async Task Execute_ReturnsAccountResult_WhenValid()
    {
        // Arrange
        var accountMock = new Account()
        {
            Id = 1,
            Name = "Test Account",
            Color = "#FFFFFF",
            UserId = 1,
            Balance = 100
        };
        var query = new GetAllAccountsQuery() { UserId = accountMock.UserId };
        var command = new GetAllAccountsCommand(accountRepositoryMock.Object);

        accountRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<int>())).ReturnsAsync([accountMock]);

        // Act
        var result = await command.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(accountMock.Id, result[0].Id);
        Assert.Equal(accountMock.Name, result[0].Name);
        Assert.Equal(accountMock.Color, result[0].Color);
        Assert.Equal(accountMock.Balance, result[0].Balance);
    }
}