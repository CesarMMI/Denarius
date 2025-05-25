using Denarius.Application.Shared.Command;
using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.Validators;

namespace Denarius.Application.Accounts.Commands.Create;

public class CreateAccountQuery : Query
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public decimal Balance { get; set; } = 0;

    public override void Validate()
    {
        base.Validate();

        ValidateString(Name, nameof(Name), 3, 50);

        if (Color is not null && !Color.IsValidColor())
            throw new BadRequestException("Invalid color");
    }
}
