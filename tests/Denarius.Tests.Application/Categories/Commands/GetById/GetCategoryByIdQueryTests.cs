using Denarius.Application.Categories.Commands.GetById;
using Denarius.Application.Shared.Exceptions;

namespace Denarius.Tests.Application.Categories.Commands.GetById;

public class GetCategoryByIdQueryTests
{
    [Fact]
    public void Validate_NotThrows_WhenValid()
    {
        // Arrange
        var query = new GetCategoryByIdQuery
        {
            UserId = 1,
            Id = 1
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
        var query = new GetCategoryByIdQuery
        {
            UserId = 1,
            Id = categoryId
        };
        // Act & Assert
        var ex = Assert.Throws<BadRequestException>(query.Validate);
        Assert.Equal("Category id is required", ex.Message);
    }
}
