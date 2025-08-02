using Denarius.Application.Categories.Commands.GetAll;
using Denarius.Domain.Enums;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Moq;

namespace Denarius.Tests.Application.Categories.Commands.GetAll;

public class GetAllCategoriesCommandTests
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
        var query = new GetAllCategoriesQuery() { UserId = categoryMock.UserId };
        var command = new GetAllCategoriesCommand(categoryRepositoryMock.Object);

        categoryRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<int>())).ReturnsAsync([categoryMock]);

        // Act
        var result = await command.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(categoryMock.Id, result[0].Id);
        Assert.Equal(categoryMock.Name, result[0].Name);
        Assert.Equal(categoryMock.Color, result[0].Color);
        Assert.Equal(categoryMock.Type, result[0].Type);
    }
}