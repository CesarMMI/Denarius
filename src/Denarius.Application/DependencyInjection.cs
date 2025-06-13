using Denarius.Application.Accounts.Commands.Create;
using Denarius.Application.Accounts.Commands.Delete;
using Denarius.Application.Accounts.Commands.GetAll;
using Denarius.Application.Accounts.Commands.Update;
using Denarius.Application.Auth.Commands.Login;
using Denarius.Application.Auth.Commands.Refresh;
using Denarius.Application.Auth.Commands.Register;
using Denarius.Application.Categories.Commands.Create;
using Denarius.Application.Categories.Commands.Delete;
using Denarius.Application.Categories.Commands.GetAll;
using Denarius.Application.Categories.Commands.GetTypes;
using Denarius.Application.Categories.Commands.Update;
using Denarius.Application.Transactions.Commands.Create;
using Denarius.Application.Transactions.Commands.Delete;
using Denarius.Application.Transactions.Commands.GetAll;
using Denarius.Application.Transactions.Commands.Update;
using Microsoft.Extensions.DependencyInjection;

namespace Denarius.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services
            .AddAuth()
            .AddAccounts()
            .AddCategories()
            .AddTransactions();
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
        services.AddScoped<IUpdateAccountCommand, UpdateAccountCommand>();
        return services;
    }

    private static IServiceCollection AddCategories(this IServiceCollection services)
    {
        services.AddScoped<ICreateCategoryCommand, CreateCategoryCommand>();
        services.AddScoped<IDeleteCategoryCommand, DeleteCategoryCommand>();
        services.AddScoped<IGetAllCategoriesCommand, GetAllCategoriesCommand>();
        services.AddScoped<IUpdateCategoryCommand, UpdateCategoryCommand>();
        services.AddScoped<IGetCategoryTypesCommand, GetCategoryTypesCommand>();
        return services;
    }

    private static IServiceCollection AddTransactions(this IServiceCollection services)
    {
        services.AddScoped<ICreateTransactionCommand, CreateTransactionCommand>();
        services.AddScoped<IDeleteTransactionCommand, DeleteTransactionCommand>();
        services.AddScoped<IGetAllTransactionsCommand, GetAllTransactionsCommand>();
        services.AddScoped<IUpdateTransactionCommand, UpdateTransactionCommand>();
        return services;
    }
}
