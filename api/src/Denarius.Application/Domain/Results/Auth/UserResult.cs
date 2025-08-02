namespace Denarius.Application.Domain.Results.Auth;

public readonly struct UserResult
{
    public string Name { get; init; }
    public string Email { get; init; }
}
