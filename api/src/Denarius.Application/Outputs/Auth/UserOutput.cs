using Denarius.Domain.Entities;

namespace Denarius.Application.Outputs.Auth;

public record UserOutput(Guid Id, string Email, string Name, DateTime CreatedAt)
{
    public static UserOutput FromEntity(User user) =>
        new(user.Id, user.Email, user.Name, user.CreatedAt);
}
