namespace Denarius.Application.Exceptions;

public abstract class NotFoundException : AppException
{
    protected NotFoundException(string resourceName, Guid id)
        : base($"{resourceName} '{id}' not found.") { }
}
