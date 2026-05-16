using Denarius.Application.Inputs.Transactions;
using Denarius.Application.Interfaces.UseCases.Transactions;
using Denarius.Api.Requests.Transactions;
using Denarius.Domain.Enums;

namespace Denarius.Api.Endpoints;

public static class TransactionEndpoints
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/transactions");

        group.MapGet("/", async (
            Guid? accountId,
            Guid? categoryId,
            TransactionType? type,
            DateTime? startDate,
            DateTime? endDate,
            IListTransactionsUseCase useCase) =>
        {
            var result = await useCase.Execute(new ListTransactionsInput(
                CurrentUser.Id, accountId, categoryId, type, startDate, endDate));
            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", async (Guid id, IGetTransactionByIdUseCase useCase) =>
        {
            var result = await useCase.Execute(new GetTransactionByIdInput(CurrentUser.Id, id));
            return Results.Ok(result);
        });

        group.MapPost("/", async (CreateTransactionRequest request, ICreateTransactionUseCase useCase) =>
        {
            var result = await useCase.Execute(new CreateTransactionInput(
                CurrentUser.Id,
                request.AccountId,
                request.CategoryId,
                request.Type,
                request.Amount,
                request.Description,
                request.Date));
            return Results.Created($"/api/transactions/{result.Id}", result);
        });

        group.MapPost("/transfers", async (CreateTransferRequest request, ICreateTransferUseCase useCase) =>
        {
            var result = await useCase.Execute(new CreateTransferInput(
                CurrentUser.Id,
                request.SourceAccountId,
                request.DestinationAccountId,
                request.Amount,
                request.Description,
                request.Date));
            return Results.Created($"/api/transactions/{result.Outgoing.Id}", result);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateTransactionRequest request, IUpdateTransactionUseCase useCase) =>
        {
            var result = await useCase.Execute(new UpdateTransactionInput(
                CurrentUser.Id, id, request.Amount, request.Description, request.CategoryId));
            return Results.Ok(result);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IDeleteTransactionUseCase useCase) =>
        {
            await useCase.Execute(new DeleteTransactionInput(CurrentUser.Id, id));
            return Results.NoContent();
        });

        return app;
    }
}
