using Denarius.Application.Exceptions;
using Denarius.Application.IO.Transactions;
using Denarius.Application.UseCases.Transactions.Create;
using Denarius.Domain.Entities;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Repositories;
using Denarius.Domain.ValueObjects;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Transactions.Create;

public class CreateTransactionUseCaseTests
{
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICreateTransactionUseCase _useCase;

    public CreateTransactionUseCaseTests()
    {
        _useCase = new CreateTransactionUseCase(_transactionRepository, _categoryRepository, _unitOfWork);
    }

    private static Category ValidCategory => new("Mercado", new Color("#FF0000"));

    [Fact]
    public async Task Execute_ValidInput_PersistsTransactionAndReturnsOutput()
    {
        var category = ValidCategory;
        _categoryRepository.GetByIdAsync(category.Id).Returns(category);
        var date = DateTime.UtcNow;
        var input = new CreateTransactionInput("Compras", date, 150.75m, category.Id);

        var output = await _useCase.Execute(input);

        Assert.Equal("Compras", output.Description);
        Assert.Equal(date, output.Date);
        Assert.Equal(150.75m, output.Value);
        Assert.Equal(category.Id, output.CategoryId);
        await _transactionRepository.Received(1).AddAsync(Arg.Is<Transaction>(t => t.CategoryId == category.Id && t.Value == 150.75m));
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Execute_CategoryNotFound_ThrowsNotFoundExceptionAndDoesNotPersist()
    {
        var categoryId = Guid.NewGuid();
        _categoryRepository.GetByIdAsync(categoryId).Returns((Category?)null);
        var input = new CreateTransactionInput("Compras", DateTime.UtcNow, 150.75m, categoryId);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Execute(input));
        await _transactionRepository.DidNotReceive().AddAsync(Arg.Any<Transaction>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Execute_ZeroValue_ThrowsDomainExceptionAndDoesNotPersist()
    {
        var category = ValidCategory;
        _categoryRepository.GetByIdAsync(category.Id).Returns(category);
        var input = new CreateTransactionInput("Compras", DateTime.UtcNow, 0m, category.Id);

        await Assert.ThrowsAsync<DomainException>(() => _useCase.Execute(input));
        await _transactionRepository.DidNotReceive().AddAsync(Arg.Any<Transaction>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }
}
