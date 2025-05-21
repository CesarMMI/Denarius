using Denarius.Application.Shared.Command;
using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.Validators;

namespace Denarius.Application.Auth.Commands.Refresh;

public class RefreshQuery : Query
{
    public string RefreshToken { get; set; } = string.Empty;

    public override void Validate()
    {
        if (!RefreshToken.IsValidString())
            throw new BadRequestException("Refresh token is required");
    }
}
