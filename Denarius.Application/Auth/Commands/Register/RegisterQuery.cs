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
        ValidateString(Name, nameof(Name), 3, 50);

        if (!Email.IsValidEmail())
            throw new BadRequestException("Invalid email");

        ValidateString(Password, nameof(Password), 5, 100);
    }
}
