using Denarius.Application.Shared.Exceptions;

namespace Denarius.Application.Shared.Command;

public class Query
{
    public int UserId { get; set; }

    public virtual void Validate()
    {
        if (UserId < 1)
            throw new BadRequestException("User id is required");
    }
}
