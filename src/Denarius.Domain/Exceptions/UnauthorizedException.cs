namespace Denarius.Domain.Exceptions;

public class UnauthorizedException(string message) : AppException(message)
{
    public override int StatusCode => 401;
    public override string Title => "Unauthorized";
}
