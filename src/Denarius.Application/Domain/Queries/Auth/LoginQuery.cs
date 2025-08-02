using Denarius.Application.Extensions;
using Denarius.Domain.Exceptions;

namespace Denarius.Application.Domain.Queries.Auth;

public class LoginQuery : Query
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public override void Validate()
    {
        if (!Email.IsValidString())
            throw new BadRequestException("Email is required");
        if (!Email.IsValidEmail())
            throw new BadRequestException("Invalid email");

        if (!Password.IsValidString())
            throw new BadRequestException("Password is required");
        if (Password.Length < 5)
            throw new BadRequestException("Password length can't be lower than 5");
        if (Password.Length > 100)
            throw new BadRequestException("Password length can't be greater than 100");
    }
}
