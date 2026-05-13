using Denarius.Application.Interfaces.UseCases.Accounts;
using Denarius.Application.Interfaces.UseCases.Categories;
using Denarius.Application.Interfaces.UseCases.Transactions;
using Denarius.Application.UseCases.Accounts;
using Denarius.Application.UseCases.Categories;
using Denarius.Application.UseCases.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace Denarius.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateAccountUseCase, CreateAccountUseCase>();
        services.AddScoped<IGetAccountByIdUseCase, GetAccountByIdUseCase>();
        services.AddScoped<IListAccountsUseCase, ListAccountsUseCase>();
        services.AddScoped<IUpdateAccountUseCase, UpdateAccountUseCase>();
        services.AddScoped<IDeactivateAccountUseCase, DeactivateAccountUseCase>();

        services.AddScoped<ICreateCategoryUseCase, CreateCategoryUseCase>();
        services.AddScoped<IGetCategoryByIdUseCase, GetCategoryByIdUseCase>();
        services.AddScoped<IListCategoriesUseCase, ListCategoriesUseCase>();
        services.AddScoped<IUpdateCategoryUseCase, UpdateCategoryUseCase>();
        services.AddScoped<IDeleteCategoryUseCase, DeleteCategoryUseCase>();

        services.AddScoped<ICreateTransactionUseCase, CreateTransactionUseCase>();
        services.AddScoped<ICreateTransferUseCase, CreateTransferUseCase>();
        services.AddScoped<IGetTransactionByIdUseCase, GetTransactionByIdUseCase>();
        services.AddScoped<IListTransactionsUseCase, ListTransactionsUseCase>();
        services.AddScoped<IUpdateTransactionUseCase, UpdateTransactionUseCase>();
        services.AddScoped<IDeleteTransactionUseCase, DeleteTransactionUseCase>();

        return services;
    }
}
