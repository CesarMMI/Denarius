using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Denarius.Api.Extensions;

namespace Denarius.Api.Tests.Extensions;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_WithValidSubClaim_ReturnsUserId()
    {
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = principal.GetUserId();

        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetUserId_WithMissingSubClaim_ThrowsInvalidOperationException()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<Claim>()));

        Assert.Throws<InvalidOperationException>(() => principal.GetUserId());
    }
}
