namespace Denarius.Application.Domain.Queries.Auth;

public class LoginQuery : Query
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
