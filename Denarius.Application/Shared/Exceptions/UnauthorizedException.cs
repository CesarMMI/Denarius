﻿namespace Denarius.Application.Shared.Exceptions;

public class UnauthorizedException(string message) : AppException(message)
{
    public override int StatusCode => 401;
    public override string Title => "Unauthorized";
}
