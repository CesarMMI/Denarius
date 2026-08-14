using Denarius.Application.IO.Categories;
using Denarius.Application.UseCases.Categories.List;
using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;
using Denarius.Domain.ValueObjects;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Categories.List;

public class ListCategoriesUseCaseTests
{
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IListCategoriesUseCase _useCase;

    public ListCategoriesUseCaseTests()
    {
        _useCase = new ListCategoriesUseCase(_categoryRepository);
    }

    [Fact]
    public async Task Execute_NoFilters_ReturnsAllCategoriesMappedToOutput()
    {
        var categories = new List<Category>
        {
            new("Lazer", new Color("#FF0000")),
            new("Mercado", new Color("#00FF00"))
        };

        _categoryRepository.GetAllAsync(null, null).Returns(categories);

        var output = await _useCase.Execute(new ListCategoriesInput(null, null));

        Assert.Equal(2, output.Count());
        Assert.Contains(output, o => o.Name == "Lazer" && o.Color == "#FF0000");
        Assert.Contains(output, o => o.Name == "Mercado" && o.Color == "#00FF00");
        await _categoryRepository.Received(1).GetAllAsync(null, null);
    }

    [Fact]
    public async Task Execute_WithNameAndColorFilters_PassesFiltersToRepository()
    {
        var categories = new List<Category> { new("Lazer", new Color("#FF0000")) };

        _categoryRepository.GetAllAsync("Laz", "#FF0000").Returns(categories);

        var output = await _useCase.Execute(new ListCategoriesInput("Laz", "#FF0000"));

        Assert.Single(output);
        await _categoryRepository.Received(1).GetAllAsync("Laz", "#FF0000");
    }

    [Fact]
    public async Task Execute_RepositoryReturnsNoMatches_ReturnsEmpty()
    {
        _categoryRepository.GetAllAsync("Inexistente", null).Returns([]);

        var output = await _useCase.Execute(new ListCategoriesInput("Inexistente", null));

        Assert.Empty(output);
    }
}
