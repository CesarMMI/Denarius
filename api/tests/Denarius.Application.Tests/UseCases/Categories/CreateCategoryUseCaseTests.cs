using Denarius.Application.Inputs.Categories;
using Denarius.Application.UseCases.Categories;
using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Categories;

public class CreateCategoryUseCaseTests
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateCategoryUseCase _sut;

    public CreateCategoryUseCaseTests()
    {
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new CreateCategoryUseCase(_categoryRepository, _unitOfWork);
    }

    // -------------------------------------------------------------------------
    // Execute — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WithValidInput_ReturnsCreatedCategory()
    {
        var input = new CreateCategoryInput(Guid.NewGuid(), "Salário", "#00FF00", CategoryType.Income);

        var result = await _sut.Execute(input);

        Assert.Equal("Salário", result.Name);
        Assert.Equal("#00FF00", result.Color);
        Assert.Equal(CategoryType.Income, result.Type);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_CallsAddAsyncAndCommit()
    {
        var input = new CreateCategoryInput(Guid.NewGuid(), "Salário", "#00FF00", CategoryType.Income);

        await _sut.Execute(input);

        await _categoryRepository.Received(1).AddAsync(Arg.Any<Category>());
        await _unitOfWork.Received(1).CommitAsync();
    }

    // -------------------------------------------------------------------------
    // Execute — errors
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExecuteAsync_WithInvalidName_Throws(string? name)
    {
        var input = new CreateCategoryInput(Guid.NewGuid(), name!, "#000000", CategoryType.Income);

        await Assert.ThrowsAsync<InvalidNameException>(() => _sut.Execute(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExecuteAsync_WithInvalidColor_Throws(string? color)
    {
        var input = new CreateCategoryInput(Guid.NewGuid(), "Salário", color!, CategoryType.Income);

        await Assert.ThrowsAsync<InvalidColorException>(() => _sut.Execute(input));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidInput_DoesNotCallAddAsync()
    {
        var input = new CreateCategoryInput(Guid.NewGuid(), "", "#000000", CategoryType.Income);

        await Assert.ThrowsAsync<InvalidNameException>(() => _sut.Execute(input));

        await _categoryRepository.DidNotReceive().AddAsync(Arg.Any<Category>());
    }
}
