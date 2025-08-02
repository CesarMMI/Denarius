namespace Denarius.Application.Domain.Results.Auth;

public readonly struct AuthResult
{
    public UserResult User { get; init; }
    public string AccessToken { get; init; }
    public string RefreshToken { get; init; }
}
