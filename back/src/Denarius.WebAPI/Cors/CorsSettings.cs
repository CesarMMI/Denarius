namespace Denarius.WebAPI.Cors;

public sealed class CorsSettings
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; init; } = [];
    public string[] AllowedMethods { get; init; } = ["*"];
    public string[] AllowedHeaders { get; init; } = ["*"];
    public bool AllowCredentials { get; init; }
}
