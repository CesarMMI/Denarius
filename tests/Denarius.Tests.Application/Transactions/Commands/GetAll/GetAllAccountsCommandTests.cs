using Denarius.Application.Transactions.Commands.GetAll;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Moq;

namespace Denarius.Tests.Application.Transactions.Commands.GetAll;

public class GetAllTransactionsCommandTests
{
    private static readonly Mock<ITransactionRepository> transactionRepositoryMock = new();
    private static readonly GetAllTransactionsCommand command = new(transactionRepositoryMock.Object);

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

        var query = new GetAllTransactionsQuery() { UserId = 1 };

        transactionRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<int>()))
            .ReturnsAsync([transactionMock]);

        // Act
        var result = await command.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(transactionMock.Id, result[0].Id);
        Assert.Equal(transactionMock.Amount, result[0].Amount);
        Assert.Equal(transactionMock.Date, result[0].Date);
        Assert.Equal(transactionMock.Description, result[0].Description);
        Assert.Equal(transactionMock.AccountId, result[0].AccountId);
        Assert.Equal(transactionMock.CategoryId, result[0].CategoryId);
    }
}