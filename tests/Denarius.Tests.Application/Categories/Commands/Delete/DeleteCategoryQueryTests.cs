using Denarius.Application.Categories.Commands.Delete;
using Denarius.Application.Shared.Exceptions;

namespace Denarius.Tests.Application.Categories.Commands.Delete;

public class DeleteCategoryQueryTests
{
    [Fact]
    public void Validate_NotThrows_WhenValid()
    {
        // Arrange
        var query = new DeleteCategoryQuery
        {
            Id = 1,
            UserId = 1
        };
        // Act
        query.Validate();
        // Assert
        Assert.True(true);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ThrowsBadRequest_WhenCategoryIdIsEmpty(int categoryId)
    {
        // Arrange
        var query = new DeleteCategoryQuery
        {
            Id = categoryId,
            UserId = 1
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Category id is required", ex.Message);
    }
}
