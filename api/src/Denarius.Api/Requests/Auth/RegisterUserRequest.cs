namespace Denarius.Api.Requests.Auth;

public record RegisterUserRequest(string Email, string Password, string Name);
