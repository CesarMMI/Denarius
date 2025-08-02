using Denarius.Application.Domain.Queries;
using System.Security.Claims;

namespace Denarius.Web.Extensions;

internal static class QueryExtensions
{
    public static T WithId<T>(this T query, int id) where T : QueryId
    {
        query.Id = id;
        return query;
    }

    public static T WithUserId<T>(this T query, HttpContext httpContext) where T : Query
    {
        var user = httpContext.User;
        var subClaim = user.FindFirst("sub") ?? user.FindFirst(ClaimTypes.NameIdentifier);

        if (subClaim != null && int.TryParse(subClaim.Value, out var userId))
            query.UserId = userId;

        return query;
    }
}
