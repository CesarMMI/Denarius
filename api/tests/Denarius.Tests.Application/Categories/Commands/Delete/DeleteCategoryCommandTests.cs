using Denarius.Application.Categories.Commands.Delete;
using Denarius.Domain.Enums;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Domain.UnitOfWork;
using Moq;

namespace Denarius.Tests.Application.Categories.Commands.Delete;

public class DeleteCategoryCommandTests
{
    private static readonly Mock<IUnitOfWork> unitOfWorkMock = new();
    private static readonly Mock<ICategoryRepository> categoryRepositoryMock = new();

    [Fact]
    public async Task Execute_ReturnsCategoryResult_WhenValid()
    {
        // Arrange
        var categoryMock = new Category()
        {
            Id = 1,
            Name = "Test Category",
            Color = "#FFFFFF",
            Type = ECategoryType.Expense,
            UserId = 1,
        };
        var query = new DeleteCategoryQuery()
        {
            Id = categoryMock.Id,
            UserId = categoryMock.UserId,
        };
        var command = new DeleteCategoryCommand(unitOfWorkMock.Object, categoryRepositoryMock.Object);

        categoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(categoryMock);
        categoryRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Category>())).ReturnsAsync(categoryMock);

        // Act
        var result = await command.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categoryMock.Id, result.Id);
        Assert.Equal(categoryMock.Name, result.Name);
        Assert.Equal(categoryMock.Color, result.Color);
        Assert.Equal(categoryMock.Type, result.Type);
    }

    [Fact]
    public async Task Execute_ThrowsNotFound_WhenCategoryIsNotFound()
    {
        // Arrange
        var query = new DeleteCategoryQuery()
        {
            Id = 1,
            UserId = 1,
        };
        var command = new DeleteCategoryCommand(unitOfWorkMock.Object, categoryRepositoryMock.Object);

        categoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((Category)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => command.Execute(query));
        Assert.Equal("Category not found", ex.Message);
    }
}
