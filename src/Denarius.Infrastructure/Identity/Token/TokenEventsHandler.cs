using Denarius.Application.Shared.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Denarius.Infrastructure.Identity.Token;

internal class TokenEventsHandler : JwtBearerEvents
{
    public TokenEventsHandler()
    {
        OnAuthenticationFailed = OnAuthenticationFailedEvent;
    }

    public static string GetExceptionMessage(Exception ex)
    {
        return ex switch
        {
            SecurityTokenExpiredException => "Token expired",
            SecurityTokenInvalidSignatureException => "Invalid token signature",
            SecurityTokenInvalidIssuerException => "Invalid token issuer",
            SecurityTokenInvalidAudienceException => "Invalid token audience",
            SecurityTokenValidationException => "Invalid token",
            SecurityTokenException => "Token error",
            _ => "Unexpected authentication error"
        };
    }

    private Task OnAuthenticationFailedEvent(AuthenticationFailedContext context)
    {
        var message = GetExceptionMessage(context.Exception);
        throw new UnauthorizedException(message);
    }
}
