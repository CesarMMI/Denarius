using Denarius.Application.Extensions;
using Denarius.Domain.Exceptions;

namespace Denarius.Application.Domain.Queries;

public abstract class QueryId : Query
{
    public int Id { get; set; }

    public override void Validate()
    {
        base.Validate();

        if (!Id.IsValidId())
            throw new BadRequestException("Id is required");
    }
}
