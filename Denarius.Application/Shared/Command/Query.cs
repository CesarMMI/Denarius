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

    protected static void ValidateString(string value, string name, int min, int max)
    {
        if (!value.IsValidString())
            throw new BadRequestException(name + " is required");
        if (value.Length < min)
            throw new BadRequestException(name + " length can't be lower than " + min);
        if (value.Length > max)
            throw new BadRequestException(name + " length can't be greater than " + max);
    }
}
