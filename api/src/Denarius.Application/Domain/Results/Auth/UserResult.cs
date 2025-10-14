using Denarius.Domain.Models;

namespace Denarius.Application.Domain.Results.Auth;

public readonly struct UserResult
{
    public string Name { get; init; }
    public string Email { get; init; }
}

internal static class UserResultExtensions
{
    public static UserResult ToResult(this User user)
    {
        return new UserResult
        {
            Name = user.Name,
            Email = user.Email
        };
    }
}
