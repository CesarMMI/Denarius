using Denarius.Application.Categories.Commands.GetAll;

namespace Denarius.Tests.Application.Categories.Commands.GetAll;

public class GetAllCategoriesQueryTests
{
    [Fact]
    public void Validate_NotThrows_WhenValid()
    {
        // Arrange
        var query = new GetAllCategoriesQuery
        {
            UserId = 1
        };
        // Act
        query.Validate();
        // Assert
        Assert.True(true);
    }
}
