using Denarius.Application.Categories.Commands.GetTypes;

namespace Denarius.Tests.Application.Categories.Commands.GetTypes;

public class GetCategoryTypesQueryTests
{
    [Fact]
    public void Validate_NotThrows_WhenValid()
    {
        // Arrange
        var query = new GetCategoryTypesQuery { };
        // Act
        query.Validate();
        // Assert
        Assert.True(true);
    }
}
