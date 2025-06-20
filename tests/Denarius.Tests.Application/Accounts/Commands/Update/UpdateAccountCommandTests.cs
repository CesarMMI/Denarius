﻿using Denarius.Application.Accounts.Commands.Update;
using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.UnitOfWork;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Moq;

namespace Denarius.Tests.Application.Accounts.Commands.Update;

public class UpdateAccountCommandTests
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
        var query = new UpdateAccountQuery()
        {
            Id = accountMock.Id,
            Name = "Updated Account Name",
            Color = "#000",
            UserId = accountMock.UserId
        };
        var command = new UpdateAccountCommand(unitOfWorkMock.Object, accountRepositoryMock.Object);

        accountRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(accountMock);
        accountRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Account>())).ReturnsAsync(accountMock);

        // Act
        var result = await command.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accountMock.Id, result.Id);
        Assert.Equal(query.Name, result.Name);
        Assert.Equal(query.Color, result.Color);
        Assert.Equal(accountMock.Balance, result.Balance);
    }

    [Fact]
    public async Task Execute_ThrowsNotFound_WhenAccountIsNotFound()
    {
        // Arrange
        var query = new UpdateAccountQuery()
        {
            Id = 1,
            Name = "Test Account",
            Color = "#FFFFFF",
            UserId = 1,
        };
        var command = new UpdateAccountCommand(unitOfWorkMock.Object, accountRepositoryMock.Object);

        accountRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((Account)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => command.Execute(query));
        Assert.Equal("Account not found", ex.Message);
    }
}
