using Denarius.Application.Accounts.Commands.Create;
using Denarius.Application.Shared.UnitOfWork;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Moq;

namespace Denarius.Tests.Application.Accounts.Commands.Create;

public class CreateAccountCommandTests
{
    private static readonly Mock<IUnitOfWork> unitOfWorkMock = new();
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
        var query = new CreateAccountQuery()
        {
            Name = accountMock.Name,
            Color = accountMock.Color,
            Balance = accountMock.Balance,
            UserId = accountMock.UserId
        };
        var command = new CreateAccountCommand(unitOfWorkMock.Object, accountRepositoryMock.Object);

        accountRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Account>())).ReturnsAsync(accountMock);

        // Act
        var result = await command.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accountMock.Id, result.Id);
        Assert.Equal(accountMock.Name, result.Name);
        Assert.Equal(accountMock.Color, result.Color);
        Assert.Equal(accountMock.Balance, result.Balance);
    }
}
