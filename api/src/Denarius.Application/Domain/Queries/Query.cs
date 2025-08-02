using Denarius.Application.Extensions;
using Denarius.Domain.Exceptions;

namespace Denarius.Application.Domain.Queries;

public abstract class Query
{
    public int UserId { get; set; }

    public virtual void Validate()
    {
        if (!UserId.IsValidId())
            throw new BadRequestException("User id is required");
    }
}
