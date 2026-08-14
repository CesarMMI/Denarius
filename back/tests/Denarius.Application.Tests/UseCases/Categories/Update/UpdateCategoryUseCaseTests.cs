using Denarius.Application.Exceptions;
using Denarius.Application.IO.Categories;
using Denarius.Application.UseCases.Categories.Update;
using Denarius.Domain.Entities;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Repositories;
using Denarius.Domain.ValueObjects;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Categories.Update;

public class UpdateCategoryUseCaseTests
{
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUpdateCategoryUseCase _useCase;

    public UpdateCategoryUseCaseTests()
    {
        _useCase = new UpdateCategoryUseCase(_categoryRepository, _unitOfWork);
    }

    [Fact]
    public async Task Execute_ValidInput_PersistsCategoryAndReturnsOutput()
    {
        var category = new Category("Lazer", new Color("#FF0000"));
        _categoryRepository.GetByIdAsync(category.Id).Returns(category);
        var input = new UpdateCategoryInput("Transporte", "#00FF00");

        var output = await _useCase.Execute((category.Id, input));

        Assert.Equal(category.Id, output.Id);
        Assert.Equal("Transporte", output.Name);
        Assert.Equal("#00FF00", output.Color);
        await _categoryRepository.Received(1).UpdateAsync(Arg.Is<Category>(c => c.Id == category.Id && c.Name == "Transporte"));
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Execute_CategoryNotFound_ThrowsNotFoundExceptionAndDoesNotPersist()
    {
        var id = Guid.NewGuid();
        _categoryRepository.GetByIdAsync(id).Returns((Category?)null);
        var input = new UpdateCategoryInput("Transporte", "#00FF00");

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Execute((id, input)));
        await _categoryRepository.DidNotReceive().UpdateAsync(Arg.Any<Category>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Execute_InvalidColor_ThrowsDomainExceptionAndDoesNotPersist()
    {
        var category = new Category("Lazer", new Color("#FF0000"));
        _categoryRepository.GetByIdAsync(category.Id).Returns(category);
        var input = new UpdateCategoryInput("Transporte", "not-a-color");

        await Assert.ThrowsAsync<DomainException>(() => _useCase.Execute((category.Id, input)));
        await _categoryRepository.DidNotReceive().UpdateAsync(Arg.Any<Category>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Execute_EmptyName_ThrowsDomainExceptionAndDoesNotPersist()
    {
        var category = new Category("Lazer", new Color("#FF0000"));
        _categoryRepository.GetByIdAsync(category.Id).Returns(category);
        var input = new UpdateCategoryInput("", "#00FF00");

        await Assert.ThrowsAsync<DomainException>(() => _useCase.Execute((category.Id, input)));
        await _categoryRepository.DidNotReceive().UpdateAsync(Arg.Any<Category>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }
}
