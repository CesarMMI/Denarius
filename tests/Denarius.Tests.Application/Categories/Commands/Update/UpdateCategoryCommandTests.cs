using Denarius.Application.Categories.Commands.Update;
using Denarius.Application.Shared.Exceptions;
using Denarius.Domain.Enums;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Moq;

namespace Denarius.Tests.Application.Categories.Commands.Update;

public class UpdateCategoryCommandTests
{
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
        var query = new UpdateCategoryQuery()
        {
            Id = categoryMock.Id,
            Name = "Updated Category Name",
            Color = "#000",
            UserId = categoryMock.UserId
        };
        var command = new UpdateCategoryCommand(categoryRepositoryMock.Object);

        categoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(categoryMock);
        categoryRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Category>())).ReturnsAsync(categoryMock);

        // Act
        var result = await command.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categoryMock.Id, result.Id);
        Assert.Equal(query.Name, result.Name);
        Assert.Equal(query.Color, result.Color);
        Assert.Equal(categoryMock.Type, result.Type);
    }

    [Fact]
    public async Task Execute_ThrowsNotFound_WhenCategoryIsNotFound()
    {
        // Arrange
        var query = new UpdateCategoryQuery()
        {
            Id = 1,
            Name = "Test Category",
            Color = "#FFFFFF",
            UserId = 1,
        };
        var command = new UpdateCategoryCommand(categoryRepositoryMock.Object);

        categoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((Category)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => command.Execute(query));
        Assert.Equal("Category not found", ex.Message);
    }
}
