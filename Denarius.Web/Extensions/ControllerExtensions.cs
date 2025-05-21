using Denarius.Application.Shared.Command;
using Denarius.Application.Shared.Exceptions;
using System.Security.Claims;

namespace Denarius.Web.Extensions;

public static class ControllerExtensions
{
    public static T SetRequestUserId<T>(this T request, HttpContext context) where T : Query
    {
        try
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            request.UserId = int.Parse(userId!);
            return request;
        }
        catch (Exception)
        {
            throw new BadRequestException("User id is required");
        }
    }
}
