using Denarius.Application.UseCases.Categories.Create;
using Denarius.Application.UseCases.Categories.Delete;
using Denarius.Application.UseCases.Categories.List;
using Denarius.Application.UseCases.Categories.Update;
using Denarius.Application.UseCases.Reports.GetCumulativeExpenseComparison;
using Denarius.Application.UseCases.Reports.GetExpensesByCategory;
using Denarius.Application.UseCases.Reports.GetIncomeVsExpense;
using Denarius.Application.UseCases.Reports.GetMonthlySummary;
using Denarius.Application.UseCases.Reports.ListMonthlyTransactions;
using Denarius.Application.UseCases.Transactions.Create;
using Denarius.Application.UseCases.Transactions.Delete;
using Denarius.Application.UseCases.Transactions.List;
using Denarius.Application.UseCases.Transactions.Update;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Denarius.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<ICreateCategoryUseCase, CreateCategoryUseCase>();
        services.AddScoped<IUpdateCategoryUseCase, UpdateCategoryUseCase>();
        services.AddScoped<IDeleteCategoryUseCase, DeleteCategoryUseCase>();
        services.AddScoped<IListCategoriesUseCase, ListCategoriesUseCase>();

        services.AddScoped<ICreateTransactionUseCase, CreateTransactionUseCase>();
        services.AddScoped<IUpdateTransactionUseCase, UpdateTransactionUseCase>();
        services.AddScoped<IDeleteTransactionUseCase, DeleteTransactionUseCase>();
        services.AddScoped<IListTransactionsUseCase, ListTransactionsUseCase>();

        services.AddScoped<IGetMonthlySummaryUseCase, GetMonthlySummaryUseCase>();
        services.AddScoped<IGetExpensesByCategoryUseCase, GetExpensesByCategoryUseCase>();
        services.AddScoped<IGetIncomeVsExpenseUseCase, GetIncomeVsExpenseUseCase>();
        services.AddScoped<IGetCumulativeExpenseComparisonUseCase, GetCumulativeExpenseComparisonUseCase>();
        services.AddScoped<IListMonthlyTransactionsUseCase, ListMonthlyTransactionsUseCase>();

        return services;
    }
}
