using Denarius.Application.Shared.Command;
using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.Validators;

namespace Denarius.Application.Auth.Commands.Register;

public class RegisterQuery : Query
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public override void Validate()
    {
        if (!Name.IsValidString())
            throw new BadRequestException("Name is required");
        if (Name.Length < 3)
            throw new BadRequestException("Name length can't be lower than 3");
        if (Name.Length > 50)
            throw new BadRequestException("Name length can't be greater than 50");

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
