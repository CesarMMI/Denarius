using Denarius.Application.Categories.Commands.GetTypes;

namespace Denarius.Tests.Application.Categories.Commands.GetTypes;

public class GetCategoryTypesCommandTests
{
    [Fact]
    public async Task Execute_ReturnsCategoryResult_WhenValid()
    {
        // Arrange
        var query = new GetCategoryTypesQuery() { };
        var command = new GetCategoryTypesCommand();

        // Act
        var result = await command.Execute(query);

        // Assert
        Assert.NotNull(result);
    }
}
