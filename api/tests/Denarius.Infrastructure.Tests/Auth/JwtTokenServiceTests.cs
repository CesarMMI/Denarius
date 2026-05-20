using System.IdentityModel.Tokens.Jwt;
using Denarius.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;

namespace Denarius.Infrastructure.Tests.Auth;

public class JwtTokenServiceTests
{
    private const string Secret = "test-secret-key-at-least-32-characters-long";
    private const string Issuer = "TestIssuer";
    private const string Audience = "TestAudience";
    private const int ExpiryMinutes = 30;

    private static JwtTokenService BuildSut(string? secret = Secret) =>
        new(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = secret,
                ["Jwt:Issuer"] = Issuer,
                ["Jwt:Audience"] = Audience,
                ["Jwt:ExpiryMinutes"] = ExpiryMinutes.ToString(),
            })
            .Build());

    // -------------------------------------------------------------------------
    // GenerateToken
    // -------------------------------------------------------------------------

    [Fact]
    public void GenerateToken_ReturnsNonEmptyString()
    {
        var result = BuildSut().GenerateToken(Guid.NewGuid(), "john@example.com", "Johnny");

        Assert.NotEmpty(result);
    }

    [Fact]
    public void GenerateToken_ReturnsThreePartJwt()
    {
        var result = BuildSut().GenerateToken(Guid.NewGuid(), "john@example.com", "Johnny");

        Assert.Equal(3, result.Split('.').Length);
    }

    [Fact]
    public void GenerateToken_ContainsCorrectClaims()
    {
        var userId = Guid.NewGuid();
        var token = BuildSut().GenerateToken(userId, "john@example.com", "Johnny");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(userId.ToString(), jwt.Subject);
        Assert.Equal("john@example.com", jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("Johnny", jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value);
        Assert.NotEmpty(jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value);
    }

    [Fact]
    public void GenerateToken_HasCorrectIssuerAndAudience()
    {
        var token = BuildSut().GenerateToken(Guid.NewGuid(), "john@example.com", "Johnny");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(Issuer, jwt.Issuer);
        Assert.Contains(Audience, jwt.Audiences);
    }

    [Fact]
    public void GenerateToken_ExpiryMatchesConfiguration()
    {
        var expectedExpiry = DateTime.UtcNow.AddMinutes(ExpiryMinutes);
        var token = BuildSut().GenerateToken(Guid.NewGuid(), "john@example.com", "Johnny");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // JWT ValidTo is truncated to the nearest second; allow ±2s tolerance
        Assert.InRange(jwt.ValidTo, expectedExpiry.AddSeconds(-2), expectedExpiry.AddSeconds(2));
    }

    [Fact]
    public void GenerateToken_WithMissingSecret_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            BuildSut(secret: null).GenerateToken(Guid.NewGuid(), "john@example.com", "Johnny"));
    }
}
