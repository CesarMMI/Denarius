using Denarius.Application.Exceptions.Transactions;
using Denarius.Application.Inputs.Transactions;
using Denarius.Application.UseCases.Transactions;
using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Transactions;

public class GetTransactionByIdUseCaseTests
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly GetTransactionByIdUseCase _sut;

    public GetTransactionByIdUseCaseTests()
    {
        _transactionRepository = Substitute.For<ITransactionRepository>();
        _sut = new GetTransactionByIdUseCase(_transactionRepository);
    }

    // -------------------------------------------------------------------------
    // Execute — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithExistingTransaction_ReturnsTransaction()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var transaction = new Transaction(userId, Guid.NewGuid(), categoryId, null, TransactionType.Income, 500m, "Salário", DateTime.UtcNow);
        _transactionRepository.GetByIdAsync(transaction.Id, userId).Returns(transaction);

        var result = await _sut.Execute(new GetTransactionByIdInput(userId, transaction.Id));

        Assert.Equal(transaction.Id, result.Id);
    }

    // -------------------------------------------------------------------------
    // Execute — errors
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithNonExistentTransaction_ThrowsTransactionNotFoundException()
    {
        var userId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        _transactionRepository.GetByIdAsync(transactionId, userId).Returns((Transaction?)null);

        await Assert.ThrowsAsync<TransactionNotFoundException>(() =>
            _sut.Execute(new GetTransactionByIdInput(userId, transactionId)));
    }

    [Fact]
    public async Task Execute_WithTransactionBelongingToAnotherUser_ThrowsTransactionNotFoundException()
    {
        var requestingUserId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        _transactionRepository.GetByIdAsync(transactionId, requestingUserId).Returns((Transaction?)null);

        await Assert.ThrowsAsync<TransactionNotFoundException>(() =>
            _sut.Execute(new GetTransactionByIdInput(requestingUserId, transactionId)));
    }
}
