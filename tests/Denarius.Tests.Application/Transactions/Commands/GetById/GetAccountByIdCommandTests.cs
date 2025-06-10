using Denarius.Application.Transactions.Commands.GetById;
using Denarius.Application.Shared.Exceptions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Moq;

namespace Denarius.Tests.Application.Transactions.Commands.GetById;

public class GetTransactionByIdCommandTests
{
    private static readonly Mock<ITransactionRepository> transactionRepositoryMock = new();
    private static readonly GetTransactionByIdCommand command = new(transactionRepositoryMock.Object);

    [Fact]
    public async Task Execute_ReturnsTransactionResult_WhenValid()
    {
        // Arrange
        var transactionMock = new Transaction()
        {
            Id = 1,
            Amount = 100,
            Date = DateTime.Now,
            Description = "Test Transaction",
            AccountId = 1,
            CategoryId = 1,
        };

        var query = new GetTransactionByIdQuery()
        {
            UserId = 1,
            Id = transactionMock.Id
        };

        transactionRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>()))
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
    }

    [Fact]
    public async Task Execute_ThrowsNotFound_WhenTransactionIsNotFound()
    {
        // Arrange
        var query = new GetTransactionByIdQuery()
        {
            UserId = 1,
            Id = 1
        };

        var command = new GetTransactionByIdCommand(transactionRepositoryMock.Object);

        transactionRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((Transaction)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => command.Execute(query));
        Assert.Equal("Transaction not found", ex.Message);
    }
}
