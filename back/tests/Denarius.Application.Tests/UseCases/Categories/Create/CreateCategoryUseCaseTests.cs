using Denarius.Application.IO.Categories;
using Denarius.Application.UseCases.Categories.Create;
using Denarius.Domain.Entities;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Categories.Create;

public class CreateCategoryUseCaseTests
{
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICreateCategoryUseCase _useCase;

    public CreateCategoryUseCaseTests()
    {
        _useCase = new CreateCategoryUseCase(_categoryRepository, _unitOfWork);
    }

    [Fact]
    public async Task Execute_ValidInput_PersistsCategoryAndReturnsOutput()
    {
        var input = new CreateCategoryInput("Lazer", "#FF0000");

        var output = await _useCase.Execute(input);

        Assert.Equal("Lazer", output.Name);
        Assert.Equal("#FF0000", output.Color);
        await _categoryRepository.Received(1).AddAsync(Arg.Is<Category>(c => c.Name == "Lazer"));
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Execute_InvalidColor_ThrowsDomainExceptionAndDoesNotPersist()
    {
        var input = new CreateCategoryInput("Lazer", "not-a-color");

        await Assert.ThrowsAsync<DomainException>(() => _useCase.Execute(input));
        await _categoryRepository.DidNotReceive().AddAsync(Arg.Any<Category>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Execute_EmptyName_ThrowsDomainExceptionAndDoesNotPersist()
    {
        var input = new CreateCategoryInput("", "#FF0000");

        await Assert.ThrowsAsync<DomainException>(() => _useCase.Execute(input));
        await _categoryRepository.DidNotReceive().AddAsync(Arg.Any<Category>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }
}
