using Denarius.Application.UseCases.Categories.Create;
using Denarius.Application.UseCases.Categories.Delete;
using Denarius.Application.UseCases.Categories.GetById;
using Denarius.Application.UseCases.Categories.List;
using Denarius.Application.UseCases.Categories.Update;
using Denarius.Application.UseCases.Transactions.Create;
using Denarius.Application.UseCases.Transactions.Delete;
using Denarius.Application.UseCases.Transactions.GetById;
using Denarius.Application.UseCases.Transactions.List;
using Denarius.Application.UseCases.Transactions.Update;
using Microsoft.Extensions.DependencyInjection;

namespace Denarius.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateCategoryUseCase, CreateCategoryUseCase>();
        services.AddScoped<IUpdateCategoryUseCase, UpdateCategoryUseCase>();
        services.AddScoped<IDeleteCategoryUseCase, DeleteCategoryUseCase>();
        services.AddScoped<IListCategoriesUseCase, ListCategoriesUseCase>();
        services.AddScoped<IGetCategoryByIdUseCase, GetCategoryByIdUseCase>();

        services.AddScoped<ICreateTransactionUseCase, CreateTransactionUseCase>();
        services.AddScoped<IUpdateTransactionUseCase, UpdateTransactionUseCase>();
        services.AddScoped<IDeleteTransactionUseCase, DeleteTransactionUseCase>();
        services.AddScoped<IListTransactionsUseCase, ListTransactionsUseCase>();
        services.AddScoped<IGetTransactionByIdUseCase, GetTransactionByIdUseCase>();

        return services;
    }
}
