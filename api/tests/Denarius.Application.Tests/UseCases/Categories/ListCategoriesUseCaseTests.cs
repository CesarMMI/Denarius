using Denarius.Application.Inputs.Categories;
using Denarius.Application.UseCases.Categories;
using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Categories;

public class ListCategoriesUseCaseTests
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ListCategoriesUseCase _sut;

    public ListCategoriesUseCaseTests()
    {
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _sut = new ListCategoriesUseCase(_categoryRepository);
    }

    // -------------------------------------------------------------------------
    // Execute — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WithoutTypeFilter_ReturnsAllCategories()
    {
        var userId = Guid.NewGuid();
        var categories = new List<Category>
        {
            new(userId, "Salário", "#00FF00", CategoryType.Income),
            new(userId, "Alimentação", "#FF0000", CategoryType.Expense)
        };
        _categoryRepository.ListByUserAsync(userId, null).Returns(categories);

        var result = await _sut.Execute(new ListCategoriesInput(userId, null));

        Assert.Equal(2, result.Count());
    }

    [Theory]
    [InlineData(CategoryType.Income)]
    [InlineData(CategoryType.Expense)]
    public async Task ExecuteAsync_WithTypeFilter_PassesTypeToRepository(CategoryType type)
    {
        var userId = Guid.NewGuid();
        _categoryRepository.ListByUserAsync(userId, type).Returns(Enumerable.Empty<Category>());

        await _sut.Execute(new ListCategoriesInput(userId, type));

        await _categoryRepository.Received(1).ListByUserAsync(userId, type);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoCategories_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        _categoryRepository.ListByUserAsync(userId, null).Returns(Enumerable.Empty<Category>());

        var result = await _sut.Execute(new ListCategoriesInput(userId, null));

        Assert.Empty(result);
    }
}
