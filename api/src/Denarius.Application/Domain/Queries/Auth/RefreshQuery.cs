using Denarius.Application.Extensions;
using Denarius.Domain.Exceptions;

namespace Denarius.Application.Domain.Queries.Auth;

public class RefreshQuery : Query
{
    public string RefreshToken { get; set; } = string.Empty;

    public override void Validate()
    {
        if (!RefreshToken.IsValidString())
            throw new BadRequestException("Refresh token is required");
    }
}
