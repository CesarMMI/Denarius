using Denarius.Application.Exceptions;
using Denarius.Application.UseCases.Categories.Delete;
using Denarius.Domain.Entities;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Repositories;
using Denarius.Domain.ValueObjects;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Categories.Delete;

public class DeleteCategoryUseCaseTests
{
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDeleteCategoryUseCase _useCase;

    public DeleteCategoryUseCaseTests()
    {
        _useCase = new DeleteCategoryUseCase(_categoryRepository, _transactionRepository, _unitOfWork);
    }

    [Fact]
    public async Task Execute_ExistingCategory_DeletesAndPersists()
    {
        var category = new Category("Lazer", new Color("#FF0000"));
        _categoryRepository.GetByIdAsync(category.Id).Returns(category);
        _transactionRepository.ExistsByCategoryIdAsync(category.Id).Returns(false);

        await _useCase.Execute(category.Id);

        await _categoryRepository.Received(1).DeleteAsync(category);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Execute_CategoryNotFound_ThrowsNotFoundExceptionAndDoesNotPersist()
    {
        var id = Guid.NewGuid();
        _categoryRepository.GetByIdAsync(id).Returns((Category?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Execute(id));
        await _categoryRepository.DidNotReceive().DeleteAsync(Arg.Any<Category>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Execute_CategoryHasTransactions_ThrowsDomainExceptionAndDoesNotPersist()
    {
        var category = new Category("Lazer", new Color("#FF0000"));
        _categoryRepository.GetByIdAsync(category.Id).Returns(category);
        _transactionRepository.ExistsByCategoryIdAsync(category.Id).Returns(true);

        await Assert.ThrowsAsync<DomainException>(() => _useCase.Execute(category.Id));
        await _categoryRepository.DidNotReceive().DeleteAsync(Arg.Any<Category>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }
}
