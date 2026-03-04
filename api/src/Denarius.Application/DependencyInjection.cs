using Denarius.Application.Interfaces.Accounts;
using Denarius.Application.Interfaces.Tags;
using Denarius.Application.Interfaces.Transactions;
using Denarius.Application.UseCases.Accounts;
using Denarius.Application.UseCases.Tags;
using Denarius.Application.UseCases.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace Denarius.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection builder)
    {
        builder.AddScoped<ICreateAccountUseCase, CreateAccountUseCase>();
        builder.AddScoped<IDeleteAccountUseCase, DeleteAccountUseCase>();
        builder.AddScoped<IGetAllAccountsUseCase, GetAllAccountsUseCase>();
        builder.AddScoped<IUpdateAccountUseCase, UpdateAccountUseCase>();

        builder.AddScoped<ICreateTagUseCase, CreateTagUseCase>();
        builder.AddScoped<IDeleteTagUseCase, DeleteTagUseCase>();
        builder.AddScoped<IGetAllTagsUseCase, GetAllTagsUseCase>();
        builder.AddScoped<IUpdateTagUseCase, UpdateTagUseCase>();

        builder.AddScoped<ICreateTransactionUseCase, CreateTransactionUseCase>();
        builder.AddScoped<IDeleteTransactionUseCase, DeleteTransactionUseCase>();
        builder.AddScoped<IGetAllTransactionsUseCase, GetAllTransactionsUseCase>();
        builder.AddScoped<IUpdateTransactionUseCase, UpdateTransactionUseCase>();

        return builder;
    }
}
