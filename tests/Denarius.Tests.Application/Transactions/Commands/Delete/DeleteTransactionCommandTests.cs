using Denarius.Application.Transactions.Commands.Delete;
using Denarius.Application.Shared.Exceptions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Moq;
using Denarius.Application.Shared.UnitOfWork;

namespace Denarius.Tests.Application.Transactions.Commands.Delete;

public class DeleteTransactionCommandTests
{
    private static readonly Mock<IUnitOfWork> unitOfWorkMock = new();
    private static readonly Mock<IAccountRepository> accountRepositoryMock = new();
    private static readonly Mock<ITransactionRepository> transactionRepositoryMock = new();

    private static readonly DeleteTransactionCommand command = new(
        unitOfWorkMock.Object,
        accountRepositoryMock.Object,
        transactionRepositoryMock.Object
    );

    [Fact]
    public async Task Execute_ReturnsTransactionResult_WhenValid()
    {
        // Arrange
        var initialBalance = 1000;
        var accountMock = new Account()
        {
            Id = 1,
            Name = "Test Account",
            Color = "#fafafa",
            Balance = initialBalance,
            UserId = 1,
        };
        var transactionMock = new Transaction()
        {
            Id = 1,
            Amount = 100,
            Date = DateTime.Now,
            Description = "Test Transaction",
            AccountId = accountMock.Id,
        };

        var query = new DeleteTransactionQuery()
        {
            UserId = accountMock.UserId,
            Id = transactionMock.Id
        };

        accountRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(accountMock);
        accountRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Account>()))
            .ReturnsAsync(accountMock);
        transactionRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(transactionMock);
        transactionRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<Transaction>()))
            .ReturnsAsync(transactionMock);

        // Act
        var result = await command.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transactionMock.Id, result.Id);
        Assert.Equal(transactionMock.Amount, result.Amount);
        Assert.Equal(transactionMock.Date, result.Date);
        Assert.Equal(transactionMock.Description, result.Description);
        Assert.Equal(transactionMock.AccountId, result.AccountId);
        Assert.Equal(transactionMock.CategoryId, result.CategoryId);
        Assert.NotEqual(initialBalance, accountMock.Balance);
    }

    [Fact]
    public async Task Execute_ThrowsNotFound_WhenTransactionIsNotFound()
    {
        // Arrange
        var query = new DeleteTransactionQuery()
        {
            UserId = 1,
            Id = 1
        };

        transactionRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((Transaction)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => command.Execute(query));
        Assert.Equal("Transaction not found", ex.Message);
    }
}
