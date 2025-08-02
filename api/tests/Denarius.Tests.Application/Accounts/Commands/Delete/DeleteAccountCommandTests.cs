using Denarius.Application.Accounts.Commands.Delete;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Domain.UnitOfWork;
using Moq;

namespace Denarius.Tests.Application.Accounts.Commands.Delete;

public class DeleteAccountCommandTests
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
        var query = new DeleteAccountQuery()
        {
            UserId = accountMock.UserId,
            Id = accountMock.Id
        };
        var command = new DeleteAccountCommand(unitOfWorkMock.Object, accountRepositoryMock.Object);

        accountRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(accountMock);
        accountRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Account>())).ReturnsAsync(accountMock);

        // Act
        var result = await command.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accountMock.Id, result.Id);
        Assert.Equal(accountMock.Name, result.Name);
        Assert.Equal(accountMock.Color, result.Color);
        Assert.Equal(accountMock.Balance, result.Balance);
    }

    [Fact]
    public async Task Execute_ThrowsNotFound_WhenAccountIsNotFound()
    {
        // Arrange
        var query = new DeleteAccountQuery()
        {
            UserId = 1,
            Id = 1
        };
        var command = new DeleteAccountCommand(unitOfWorkMock.Object, accountRepositoryMock.Object);

        accountRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((Account)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => command.Execute(query));
        Assert.Equal("Account not found", ex.Message);
    }
}
