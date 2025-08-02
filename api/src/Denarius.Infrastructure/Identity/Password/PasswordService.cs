using Denarius.Domain.Models;
using Denarius.Domain.Services;
using Microsoft.AspNetCore.Identity;

namespace Denarius.Infrastructure.Identity.Password;

internal class PasswordService : IPasswordService
{
    private readonly User user = new();
    private readonly PasswordHasher<User> hasher = new();

    public string Hash(string password)
    {
        return hasher.HashPassword(user, password);
    }

    public bool Verify(string hashedPassword, string providedPassword)
    {
        return hasher.VerifyHashedPassword(user, hashedPassword, providedPassword) == PasswordVerificationResult.Success;
    }
}
