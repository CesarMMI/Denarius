using Denarius.Domain.Models;

namespace Denarius.Application.Auth.Results;

public class UserResult
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public static class UserResultExtensions
{
    public static UserResult ToUserResult(this User user)
    {
        return new UserResult
        {
            Name = user.Name,
            Email = user.Email
        };
    }
}

