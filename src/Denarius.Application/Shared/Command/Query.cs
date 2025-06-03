using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.Validators;

namespace Denarius.Application.Shared.Command;

public abstract class Query
{
    public int UserId { get; set; }

    public virtual void Validate()
    {
        if (!UserId.IsValidId())
            throw new BadRequestException("User Id is required");
    }
}
