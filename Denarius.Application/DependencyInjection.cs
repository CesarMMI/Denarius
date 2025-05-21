using Denarius.Application.Accounts.Commands.Create;
using Denarius.Application.Accounts.Commands.Delete;
using Denarius.Application.Accounts.Commands.GetAll;
using Denarius.Application.Accounts.Commands.GetById;
using Denarius.Application.Accounts.Commands.Update;
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
            .AddAuth()
            .AddAccounts();
    }

    private static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddScoped<ILoginCommand, LoginCommand>();
        services.AddScoped<IRegisterCommand, RegisterCommand>();
        services.AddScoped<IRefreshCommand, RefreshCommand>();
        return services;
    }

    private static IServiceCollection AddAccounts(this IServiceCollection services)
    {
        services.AddScoped<ICreateAccountCommand, CreateAccountCommand>();
        services.AddScoped<IDeleteAccountCommand, DeleteAccountCommand>();
        services.AddScoped<IGetAllAccountsCommand, GetAllAccountsCommand>();
        services.AddScoped<IGetAccountByIdCommand, GetAccountByIdCommand>();
        services.AddScoped<IUpdateAccountCommand, UpdateAccountCommand>();
        return services;
    }
}
