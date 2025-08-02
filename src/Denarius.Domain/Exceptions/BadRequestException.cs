namespace Denarius.Domain.Exceptions;

public class BadRequestException(string message) : AppException(message)
{
    public override int StatusCode => 400;
    public override string Title => "Bad Request";
}