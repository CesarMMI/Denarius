using Denarius.Application.Auth.Commands.Refresh;
using Denarius.Application.Shared.Exceptions;

namespace Denarius.Tests.Application.Auth.Commands.Refresh;

public class RefreshQueryTests
{
    [Fact]
    public void Validate_NotThrows_WhenValid()
    {
        // Arrange
        var query = new RefreshQuery { RefreshToken = "refresh-token", };
        // Act
        query.Validate();
        // Assert
        Assert.True(true);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Validate_ThrowsBadRequest_WhenRefreshTokenIsInvalid(string refreshToken)
    {
        // Arrange
        var query = new RefreshQuery { RefreshToken = refreshToken };
        // Act & Assert
        Assert.Throws<BadRequestException>(query.Validate);
    }
}
