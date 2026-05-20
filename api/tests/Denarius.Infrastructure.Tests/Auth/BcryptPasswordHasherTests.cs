using Denarius.Infrastructure.Auth;

namespace Denarius.Infrastructure.Tests.Auth;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _sut = new();

    // -------------------------------------------------------------------------
    // Hash
    // -------------------------------------------------------------------------

    [Fact]
    public void Hash_WithPassword_ReturnsNonEmptyString()
    {
        var result = _sut.Hash("mypassword");

        Assert.NotEmpty(result);
    }

    [Fact]
    public void Hash_WithPassword_ReturnsBcryptHash()
    {
        var result = _sut.Hash("mypassword");

        Assert.StartsWith("$2", result);
    }

    [Fact]
    public void Hash_SamePasswordTwice_ReturnsDifferentHashes()
    {
        var first = _sut.Hash("mypassword");
        var second = _sut.Hash("mypassword");

        Assert.NotEqual(first, second);
    }

    // -------------------------------------------------------------------------
    // Verify
    // -------------------------------------------------------------------------

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _sut.Hash("mypassword");

        Assert.True(_sut.Verify("mypassword", hash));
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ReturnsFalse()
    {
        var hash = _sut.Hash("mypassword");

        Assert.False(_sut.Verify("wrongpassword", hash));
    }
}
