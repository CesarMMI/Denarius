using Denarius.Domain.Entities;
using Denarius.Infrastructure.Persistence.Repositories;
using Denarius.Infrastructure.Tests.Shared;

namespace Denarius.Infrastructure.Tests.Repositories;

public class UserRepositoryTests : RepositoryTestBase
{
    private readonly UserRepository _sut;

    public UserRepositoryTests()
    {
        _sut = new UserRepository(Context);
    }

    private static User ValidUser(string email = "john@example.com", string name = "Johnny") =>
        new(email, "hashed_password", name);

    // -------------------------------------------------------------------------
    // AddAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAsync_WithValidUser_PersistsUser()
    {
        var user = ValidUser();

        await _sut.AddAsync(user);
        await Context.SaveChangesAsync();

        using var freshContext = CreateFreshContext();
        var persisted = await freshContext.Users.FindAsync(user.Id);
        Assert.NotNull(persisted);
        Assert.Equal(user.Email, persisted.Email);
        Assert.Equal(user.Name, persisted.Name);
        Assert.Equal(user.PasswordHash, persisted.PasswordHash);
    }

    // -------------------------------------------------------------------------
    // GetByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ReturnsUser()
    {
        var user = ValidUser();
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    // -------------------------------------------------------------------------
    // GetByEmailAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ReturnsUser()
    {
        var user = ValidUser();
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();

        var result = await _sut.GetByEmailAsync(user.Email);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistentEmail_ReturnsNull()
    {
        var result = await _sut.GetByEmailAsync("nobody@example.com");

        Assert.Null(result);
    }

    // -------------------------------------------------------------------------
    // ExistsByEmailAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExistsByEmailAsync_WithExistingEmail_ReturnsTrue()
    {
        var user = ValidUser();
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();

        var result = await _sut.ExistsByEmailAsync(user.Email);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithNonExistentEmail_ReturnsFalse()
    {
        var result = await _sut.ExistsByEmailAsync("nobody@example.com");

        Assert.False(result);
    }
}
