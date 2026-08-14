using Denarius.Application.Exceptions;
using Denarius.Application.UseCases.Categories.GetById;
using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;
using Denarius.Domain.ValueObjects;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Categories.GetById;

public class GetCategoryByIdUseCaseTests
{
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IGetCategoryByIdUseCase _useCase;

    public GetCategoryByIdUseCaseTests()
    {
        _useCase = new GetCategoryByIdUseCase(_categoryRepository);
    }

    [Fact]
    public async Task Execute_ExistingCategory_ReturnsOutput()
    {
        var category = new Category("Lazer", new Color("#FF0000"));
        _categoryRepository.GetByIdAsync(category.Id).Returns(category);

        var output = await _useCase.Execute(category.Id);

        Assert.Equal(category.Id, output.Id);
        Assert.Equal("Lazer", output.Name);
        Assert.Equal("#FF0000", output.Color);
    }

    [Fact]
    public async Task Execute_CategoryNotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _categoryRepository.GetByIdAsync(id).Returns((Category?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Execute(id));
    }
}
