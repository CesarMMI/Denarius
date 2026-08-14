namespace Denarius.Application.Exceptions;

public class NotFoundException(string message) : AppException(message)
{
}
