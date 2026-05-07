using Denarius.Application.Exceptions.Categories;
using Denarius.Application.Inputs.Categories;
using Denarius.Application.UseCases.Categories;
using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Categories;

public class GetCategoryByIdUseCaseTests
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly GetCategoryByIdUseCase _sut;

    public GetCategoryByIdUseCaseTests()
    {
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _sut = new GetCategoryByIdUseCase(_categoryRepository);
    }

    // -------------------------------------------------------------------------
    // Execute — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WithExistingCategory_ReturnsCategory()
    {
        var userId = Guid.NewGuid();
        var category = new Category(userId, "Salário", "#00FF00", CategoryType.Income);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        var result = await _sut.Execute(new GetCategoryByIdInput(userId, category.Id));

        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
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
            _sut.Execute(new GetCategoryByIdInput(userId, categoryId)));
    }

    [Fact]
    public async Task ExecuteAsync_WithCategoryBelongingToAnotherUser_ThrowsCategoryNotFoundException()
    {
        var requestingUserId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        _categoryRepository.GetByIdAsync(categoryId, requestingUserId).Returns((Category?)null);

        await Assert.ThrowsAsync<CategoryNotFoundException>(() =>
            _sut.Execute(new GetCategoryByIdInput(requestingUserId, categoryId)));
    }
}
