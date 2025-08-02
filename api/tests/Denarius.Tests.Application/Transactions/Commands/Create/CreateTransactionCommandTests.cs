using Denarius.Application.Commands.Transactions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Domain.UnitOfWork;
using Moq;

namespace Denarius.Tests.Application.Transactions.Commands.Create;

public class CreateTransactionCommandTests
{
    private static readonly Mock<IUnitOfWork> unitOfWorkMock = new();
    private static readonly Mock<IAccountRepository> accountRepositoryMock = new();
    private static readonly Mock<ICategoryRepository> categoryRepositoryMock = new();
    private static readonly Mock<ITransactionRepository> transactionRepositoryMock = new();

    private static readonly CreateTransactionCommand command = new(
        unitOfWorkMock.Object,
        accountRepositoryMock.Object,
        categoryRepositoryMock.Object,
        transactionRepositoryMock.Object
    );

    [Fact]
    public async Task Execute_ReturnsTransactionResult_WhenValid()
    {
        // Arrange
        var accountMock = new Account()
        {
            Id = 1,
            Name = "Test Account",
            Color = "#fafafa",
            Balance = 1000,
            UserId = 1,
        };
        var categoryMock = new Category()
        {
            Id = 1,
            Name = "Test Category",
            Color = "#ff0000",
            UserId = 1,
        };
        var transactionMock = new Transaction()
        {
            Id = 1,
            Amount = 100,
            Date = DateTime.Now,
            Description = "Test Transaction",
            AccountId = accountMock.Id,
            CategoryId = categoryMock.Id,
        };

        var query = new CreateTransactionQuery()
        {
            Amount = transactionMock.Amount,
            Date = transactionMock.Date,
            Description = transactionMock.Description,
            AccountId = transactionMock.AccountId,
            CategoryId = transactionMock.CategoryId,
            UserId = accountMock.UserId
        };

        accountRepositoryMock
            .Setup(x => x.GetByIdAsync(query.AccountId, It.IsAny<int>()))
            .ReturnsAsync(accountMock);
        accountRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Account>()))
            .ReturnsAsync(accountMock);
        categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(query.CategoryId.Value, It.IsAny<int>()))
            .ReturnsAsync(categoryMock);
        transactionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Transaction>()))
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

    [Theory]
    [InlineData(100, 10)]
    [InlineData(100, -10)]
    [InlineData(-100, 10)]
    [InlineData(-100, -10)]
    [InlineData(0, 10)]
    [InlineData(0, -10)]
    public async Task Execute_ReturnsTransactionResult_WhenUpdateAccountBalance(decimal initialBalance, decimal amount)
    {
        // Arrange
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
            Amount = amount,
            Date = DateTime.Now,
            Description = "Test Transaction",
            AccountId = accountMock.Id,
        };

        var query = new CreateTransactionQuery()
        {
            Amount = transactionMock.Amount,
            Date = transactionMock.Date,
            Description = transactionMock.Description,
            AccountId = transactionMock.AccountId,
            CategoryId = transactionMock.CategoryId,
            UserId = accountMock.UserId
        };

        accountRepositoryMock
            .Setup(x => x.GetByIdAsync(query.AccountId, It.IsAny<int>()))
            .ReturnsAsync(accountMock);
        accountRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Account>()))
            .ReturnsAsync(accountMock);
        transactionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Transaction>()))
            .ReturnsAsync(transactionMock);

        // Act
        var result = await command.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(initialBalance, accountMock.Balance);
        Assert.Equal(initialBalance + amount, accountMock.Balance);
    }
}
