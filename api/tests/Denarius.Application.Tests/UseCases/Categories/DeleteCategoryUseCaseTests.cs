using Denarius.Application.Exceptions.Categories;
using Denarius.Application.Inputs.Categories;
using Denarius.Application.UseCases.Categories;
using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Categories;

public class DeleteCategoryUseCaseTests
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteCategoryUseCase _sut;

    public DeleteCategoryUseCaseTests()
    {
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new DeleteCategoryUseCase(_categoryRepository, _unitOfWork);
    }

    // -------------------------------------------------------------------------
    // Execute — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WithExistingCategory_CallsNullifyDeleteAndCommit()
    {
        var userId = Guid.NewGuid();
        var category = new Category(userId, "Alimentação", "#FF0000", CategoryType.Expense);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        await _sut.Execute(new DeleteCategoryInput(userId, category.Id));

        await _categoryRepository.Received(1).NullifyTransactionCategoriesAsync(category.Id);
        await _categoryRepository.Received(1).DeleteAsync(category);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingCategory_NullifiesBeforeDelete()
    {
        var userId = Guid.NewGuid();
        var category = new Category(userId, "Alimentação", "#FF0000", CategoryType.Expense);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        var callOrder = new List<string>();
        _categoryRepository
            .When(r => r.NullifyTransactionCategoriesAsync(category.Id))
            .Do(_ => callOrder.Add("nullify"));
        _categoryRepository
            .When(r => r.DeleteAsync(category))
            .Do(_ => callOrder.Add("delete"));

        await _sut.Execute(new DeleteCategoryInput(userId, category.Id));

        Assert.Equal(["nullify", "delete"], callOrder);
    }

    // -------------------------------------------------------------------------
    // Execute — errors
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WithNonExistentCategory_ThrowsCategoryNotFoundException()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        _categoryRepository.GetByIdAsync(categoryId, userId).Returns((Category?)null);

        await Assert.ThrowsAsync<CategoryNotFoundException>(() =>
            _sut.Execute(new DeleteCategoryInput(userId, categoryId)));
    }

    [Fact]
    public async Task ExecuteAsync_WithCategoryBelongingToAnotherUser_ThrowsCategoryNotFoundException()
    {
        var requestingUserId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        _categoryRepository.GetByIdAsync(categoryId, requestingUserId).Returns((Category?)null);

        await Assert.ThrowsAsync<CategoryNotFoundException>(() =>
            _sut.Execute(new DeleteCategoryInput(requestingUserId, categoryId)));
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentCategory_DoesNotCallDeleteAsync()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        _categoryRepository.GetByIdAsync(categoryId, userId).Returns((Category?)null);

        await Assert.ThrowsAsync<CategoryNotFoundException>(() =>
            _sut.Execute(new DeleteCategoryInput(userId, categoryId)));

        await _categoryRepository.DidNotReceive().DeleteAsync(Arg.Any<Category>());
    }
}
