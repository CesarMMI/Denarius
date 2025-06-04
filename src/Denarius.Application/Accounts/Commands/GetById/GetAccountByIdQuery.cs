using Denarius.Application.Shared.Command;
using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.Validators;

namespace Denarius.Application.Accounts.Commands.GetById;

public class GetAccountByIdQuery : Query
{
    public int Id { get; set; }

    public override void Validate()
    {
        base.Validate();

        if (!Id.IsValidId())
            throw new BadRequestException("Account id is required");
    }
}
