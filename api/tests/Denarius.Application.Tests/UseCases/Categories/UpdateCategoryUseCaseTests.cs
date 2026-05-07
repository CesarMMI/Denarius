using Denarius.Application.Exceptions.Categories;
using Denarius.Application.Inputs.Categories;
using Denarius.Application.UseCases.Categories;
using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Categories;

public class UpdateCategoryUseCaseTests
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateCategoryUseCase _sut;

    public UpdateCategoryUseCaseTests()
    {
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new UpdateCategoryUseCase(_categoryRepository, _unitOfWork);
    }

    // -------------------------------------------------------------------------
    // Execute — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WithValidInput_ReturnsUpdatedCategory()
    {
        var userId = Guid.NewGuid();
        var category = new Category(userId, "Salário", "#00FF00", CategoryType.Income);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        var result = await _sut.Execute(new UpdateCategoryInput(userId, category.Id, "Freelance", "#0000FF"));

        Assert.Equal("Freelance", result.Name);
        Assert.Equal("#0000FF", result.Color);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_DoesNotChangeType()
    {
        var userId = Guid.NewGuid();
        var category = new Category(userId, "Salário", "#00FF00", CategoryType.Income);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        var result = await _sut.Execute(new UpdateCategoryInput(userId, category.Id, "Freelance", "#0000FF"));

        Assert.Equal(CategoryType.Income, result.Type);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_CallsUpdateAsyncAndCommit()
    {
        var userId = Guid.NewGuid();
        var category = new Category(userId, "Salário", "#00FF00", CategoryType.Income);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        await _sut.Execute(new UpdateCategoryInput(userId, category.Id, "Freelance", "#0000FF"));

        await _categoryRepository.Received(1).UpdateAsync(category);
        await _unitOfWork.Received(1).CommitAsync();
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
            _sut.Execute(new UpdateCategoryInput(userId, categoryId, "Freelance", "#0000FF")));
    }

    [Fact]
    public async Task ExecuteAsync_WithCategoryBelongingToAnotherUser_ThrowsCategoryNotFoundException()
    {
        var requestingUserId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        _categoryRepository.GetByIdAsync(categoryId, requestingUserId).Returns((Category?)null);

        await Assert.ThrowsAsync<CategoryNotFoundException>(() =>
            _sut.Execute(new UpdateCategoryInput(requestingUserId, categoryId, "Freelance", "#0000FF")));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExecuteAsync_WithInvalidName_Throws(string? name)
    {
        var userId = Guid.NewGuid();
        var category = new Category(userId, "Salário", "#00FF00", CategoryType.Income);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        await Assert.ThrowsAsync<InvalidNameException>(() =>
            _sut.Execute(new UpdateCategoryInput(userId, category.Id, name!, "#000000")));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExecuteAsync_WithInvalidColor_Throws(string? color)
    {
        var userId = Guid.NewGuid();
        var category = new Category(userId, "Salário", "#00FF00", CategoryType.Income);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        await Assert.ThrowsAsync<InvalidColorException>(() =>
            _sut.Execute(new UpdateCategoryInput(userId, category.Id, "Freelance", color!)));
    }
}
