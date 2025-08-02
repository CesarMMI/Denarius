using Denarius.Application.Domain.Results.Auth;
using Denarius.Domain.Models;

namespace Denarius.Application.Extensions;

internal static class UserExtensions
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
