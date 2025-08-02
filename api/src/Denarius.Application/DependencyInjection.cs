using Denarius.Application.Commands.Accounts;
using Denarius.Application.Commands.Auth;
using Denarius.Application.Commands.Categories;
using Denarius.Application.Commands.Transactions;
using Denarius.Application.Domain.Commands.Accounts;
using Denarius.Application.Domain.Commands.Auth;
using Denarius.Application.Domain.Commands.Categories;
using Denarius.Application.Domain.Commands.Transactions;
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
