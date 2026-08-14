namespace Denarius.WebAPI.Cors;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSettings = configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ?? new CorsSettings();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(corsSettings.AllowedOrigins);

                if (corsSettings.AllowedMethods.Contains("*")) policy.AllowAnyMethod();
                else policy.WithMethods(corsSettings.AllowedMethods);

                if (corsSettings.AllowedHeaders.Contains("*")) policy.AllowAnyHeader();
                else policy.WithHeaders(corsSettings.AllowedHeaders);

                if (corsSettings.AllowCredentials) policy.AllowCredentials();
            });
        });

        return services;
    }
}
