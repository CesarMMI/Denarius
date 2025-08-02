namespace Denarius.Domain.Exceptions;

public abstract class AppException(string message) : Exception(message)
{
    public abstract int StatusCode { get; }
    public abstract string Title { get; }
}
