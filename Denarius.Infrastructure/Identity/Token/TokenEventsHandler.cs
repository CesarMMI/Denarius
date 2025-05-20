using Denarius.Application.Shared.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Denarius.Infrastructure.Identity.Token;

internal class TokenEventsHandler : JwtBearerEvents
{
    public TokenEventsHandler()
    {
        OnAuthenticationFailed = OnAuthenticationFailedEvent;
        OnChallenge = OnChallengeEvent;
    }

    private Task OnAuthenticationFailedEvent(AuthenticationFailedContext context)
    {
        switch (context.Exception)
        {
            case SecurityTokenExpiredException _:
                throw new UnauthorizedException("Token expired");
            case SecurityTokenInvalidSignatureException _:
                throw new UnauthorizedException("Invalid token");
            case SecurityTokenInvalidIssuerException _:
                throw new UnauthorizedException("Invalid issuer");
            case SecurityTokenException _:
                throw new UnauthorizedException("Token authentication failed");
        }
        return Task.CompletedTask;
    }

    private Task OnChallengeEvent(JwtBearerChallengeContext context)
    {
        if (!context.Response.HasStarted)
        {
            throw new UnauthorizedException("Invalid or missing token");
        }
        return Task.CompletedTask;
    }
}
