using Denarius.Application.Exceptions;
using Denarius.Application.IO.Transactions;
using Denarius.Application.UseCases.Transactions.Update;
using Denarius.Domain.Entities;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Repositories;
using Denarius.Domain.ValueObjects;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Transactions.Update;

public class UpdateTransactionUseCaseTests
{
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUpdateTransactionUseCase _useCase;

    public UpdateTransactionUseCaseTests()
    {
        _useCase = new UpdateTransactionUseCase(_transactionRepository, _categoryRepository, _unitOfWork);
    }

    private static Category ValidCategory => new("Mercado", new Color("#FF0000"));

    [Fact]
    public async Task Execute_ValidInput_PersistsTransactionAndReturnsOutput()
    {
        var category = ValidCategory;
        var transaction = new Transaction("Compras", DateTime.UtcNow, 150.75m, category.Id);
        _transactionRepository.GetByIdAsync(transaction.Id).Returns(transaction);
        _categoryRepository.GetByIdAsync(category.Id).Returns(category);
        var newDate = DateTime.UtcNow.AddDays(1);
        var input = new UpdateTransactionInput("Farmácia", newDate, 42.5m, category.Id);

        var output = await _useCase.Execute((transaction.Id, input));

        Assert.Equal(transaction.Id, output.Id);
        Assert.Equal("Farmácia", output.Description);
        Assert.Equal(newDate, output.Date);
        Assert.Equal(42.5m, output.Value);
        await _transactionRepository.Received(1).UpdateAsync(Arg.Is<Transaction>(t => t.Id == transaction.Id && t.Description == "Farmácia"));
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Execute_TransactionNotFound_ThrowsNotFoundExceptionAndDoesNotPersist()
    {
        var id = Guid.NewGuid();
        _transactionRepository.GetByIdAsync(id).Returns((Transaction?)null);
        var input = new UpdateTransactionInput("Farmácia", DateTime.UtcNow, 42.5m, Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Execute((id, input)));
        await _transactionRepository.DidNotReceive().UpdateAsync(Arg.Any<Transaction>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Execute_CategoryNotFound_ThrowsNotFoundExceptionAndDoesNotPersist()
    {
        var transaction = new Transaction("Compras", DateTime.UtcNow, 150.75m, Guid.NewGuid());
        _transactionRepository.GetByIdAsync(transaction.Id).Returns(transaction);
        var newCategoryId = Guid.NewGuid();
        _categoryRepository.GetByIdAsync(newCategoryId).Returns((Category?)null);
        var input = new UpdateTransactionInput("Farmácia", DateTime.UtcNow, 42.5m, newCategoryId);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Execute((transaction.Id, input)));
        await _transactionRepository.DidNotReceive().UpdateAsync(Arg.Any<Transaction>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Execute_ZeroValue_ThrowsDomainExceptionAndDoesNotPersist()
    {
        var category = ValidCategory;
        var transaction = new Transaction("Compras", DateTime.UtcNow, 150.75m, category.Id);
        _transactionRepository.GetByIdAsync(transaction.Id).Returns(transaction);
        _categoryRepository.GetByIdAsync(category.Id).Returns(category);
        var input = new UpdateTransactionInput("Farmácia", DateTime.UtcNow, 0m, category.Id);

        await Assert.ThrowsAsync<DomainException>(() => _useCase.Execute((transaction.Id, input)));
        await _transactionRepository.DidNotReceive().UpdateAsync(Arg.Any<Transaction>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }
}
