using Denarius.Application.Categories.Commands.Create;
using Denarius.Domain.Enums;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Domain.UnitOfWork;
using Moq;

namespace Denarius.Tests.Application.Categories.Commands.Create;

public class CreateCategoryCommandTests
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
        var query = new CreateCategoryQuery()
        {
            Name = categoryMock.Name,
            Color = categoryMock.Color,
            Type = categoryMock.Type,
            UserId = 1
        };
        var command = new CreateCategoryCommand(unitOfWorkMock.Object, categoryRepositoryMock.Object);

        categoryRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Category>())).ReturnsAsync(categoryMock);

        // Act
        var result = await command.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categoryMock.Id, result.Id);
        Assert.Equal(categoryMock.Name, result.Name);
        Assert.Equal(categoryMock.Color, result.Color);
        Assert.Equal(categoryMock.Type, result.Type);
    }
}
