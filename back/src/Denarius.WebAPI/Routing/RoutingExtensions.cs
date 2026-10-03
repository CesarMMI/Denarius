using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Denarius.WebAPI.Routing;

public static class RoutingExtensions
{
    /// <summary>Renders route tokens such as <c>[controller]</c> in camelCase (<c>CategoriesController</c> → <c>categories</c>).</summary>
    public static MvcOptions UseCamelCaseRoutes(this MvcOptions options)
    {
        options.Conventions.Add(new RouteTokenTransformerConvention(new CamelCaseRouteTransformer()));
        return options;
    }
}
