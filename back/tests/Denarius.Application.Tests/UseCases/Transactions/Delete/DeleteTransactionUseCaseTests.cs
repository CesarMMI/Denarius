using Denarius.Application.Exceptions;
using Denarius.Application.UseCases.Transactions.Delete;
using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Transactions.Delete;

public class DeleteTransactionUseCaseTests
{
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDeleteTransactionUseCase _useCase;

    public DeleteTransactionUseCaseTests()
    {
        _useCase = new DeleteTransactionUseCase(_transactionRepository, _unitOfWork);
    }

    [Fact]
    public async Task Execute_ExistingTransaction_DeletesAndPersists()
    {
        var transaction = new Transaction("Compras", DateTime.UtcNow, 150.75m, Guid.NewGuid());
        _transactionRepository.GetByIdAsync(transaction.Id).Returns(transaction);

        await _useCase.Execute(transaction.Id);

        await _transactionRepository.Received(1).DeleteAsync(transaction);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Execute_TransactionNotFound_ThrowsNotFoundExceptionAndDoesNotPersist()
    {
        var id = Guid.NewGuid();
        _transactionRepository.GetByIdAsync(id).Returns((Transaction?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Execute(id));
        await _transactionRepository.DidNotReceive().DeleteAsync(Arg.Any<Transaction>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }
}
