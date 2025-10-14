namespace Denarius.Application.Domain.Queries.Auth;

public class RefreshQuery : Query
{
    public string RefreshToken { get; set; } = string.Empty;
}
