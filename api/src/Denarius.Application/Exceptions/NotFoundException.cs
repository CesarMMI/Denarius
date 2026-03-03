namespace Denarius.Application.Exceptions;

public sealed class NotFoundException(string message) : ApplicationException(message)
{
}