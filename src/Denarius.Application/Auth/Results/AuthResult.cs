namespace Denarius.Application.Auth.Results;

public class AuthResult
{
    public UserResult User { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
