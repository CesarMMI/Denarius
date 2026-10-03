using System.Text.Json;

namespace Denarius.WebAPI.Routing;

public sealed class CamelCaseRouteTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value) =>
        value is null ? null : JsonNamingPolicy.CamelCase.ConvertName(value.ToString()!);
}
