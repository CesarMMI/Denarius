using Denarius.Domain.Entities;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Exceptions.Users;

namespace Denarius.Domain.Tests.Entities;

public class UserTests
{
    private static User ValidUser() =>
        new("john@example.com", "hashed_password", "John Doe");

    // -------------------------------------------------------------------------
    // Creation — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_WithValidFields_SetsPropertiesCorrectly()
    {
        var user = new User("jane@example.com", "hashed_pw", "Janet");

        Assert.Equal("jane@example.com", user.Email);
        Assert.Equal("hashed_pw", user.PasswordHash);
        Assert.Equal("Janet", user.Name);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    // -------------------------------------------------------------------------
    // Creation — invalid email
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("notanemail")]
    [InlineData("@nodomain")]
    [InlineData("noatsign.com")]
    [InlineData("two@@signs.com")]
    [InlineData("user@nodot")]
    [InlineData("user@.com")]
    [InlineData("user@domain.")]
    public void Create_WithInvalidEmail_Throws(string? email)
    {
        Assert.Throws<InvalidEmailException>(() =>
            new User(email!, "hash", "Name"));
    }

    // -------------------------------------------------------------------------
    // Creation — invalid name
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_Throws(string? name)
    {
        Assert.Throws<InvalidNameException>(() =>
            new User("user@example.com", "hash", name!));
    }

    [Theory]
    [InlineData("A")]
    [InlineData(" B")]
    public void Create_WithTooShortName_Throws(string name)
    {
        Assert.Throws<InvalidNameException>(() =>
            new User("user@example.com", "hash", name));
    }

    // -------------------------------------------------------------------------
    // Creation — invalid password hash
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidPasswordHash_Throws(string? hash)
    {
        Assert.Throws<InvalidPasswordHashException>(() =>
            new User("user@example.com", hash!, "Name"));
    }

    // -------------------------------------------------------------------------
    // UpdateName
    // -------------------------------------------------------------------------

    [Fact]
    public void UpdateName_WithValidName_SetsName()
    {
        var user = ValidUser();

        user.UpdateName("New Name");

        Assert.Equal("New Name", user.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithInvalidName_Throws(string? name)
    {
        var user = ValidUser();

        Assert.Throws<InvalidNameException>(() => user.UpdateName(name!));
    }

    [Theory]
    [InlineData("A")]
    [InlineData(" B")]
    public void UpdateName_WithTooShortName_Throws(string name)
    {
        var user = ValidUser();

        Assert.Throws<InvalidNameException>(() => user.UpdateName(name));
    }

    // -------------------------------------------------------------------------
    // ChangePassword
    // -------------------------------------------------------------------------

    [Fact]
    public void ChangePassword_WithValidHash_SetsPasswordHash()
    {
        var user = ValidUser();

        user.ChangePassword("new_hashed_password");

        Assert.Equal("new_hashed_password", user.PasswordHash);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangePassword_WithInvalidHash_Throws(string? hash)
    {
        var user = ValidUser();

        Assert.Throws<InvalidPasswordHashException>(() => user.ChangePassword(hash!));
    }
}
