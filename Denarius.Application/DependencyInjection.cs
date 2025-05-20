using Denarius.Application.Auth.Commands.Login;
using Denarius.Application.Auth.Commands.Refresh;
using Denarius.Application.Auth.Commands.Register;
using Microsoft.Extensions.DependencyInjection;

namespace Denarius.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services
            .AddAuth();
    }

    private static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddScoped<ILoginCommand, LoginCommand>();
        services.AddScoped<IRegisterCommand, RegisterCommand>();
        services.AddScoped<IRefreshCommand, RefreshCommand>();
        return services;
    }
}
