using System.Security.Claims;
using Denarius.Application.Inputs.Transactions;
using Denarius.Application.Interfaces.UseCases.Transactions;
using Denarius.Api.Extensions;
using Denarius.Api.Requests.Transactions;
using Denarius.Domain.Enums;

namespace Denarius.Api.Endpoints;

public static class TransactionEndpoints
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/transactions").RequireAuthorization();

        group.MapGet("/", async (
            Guid? accountId,
            Guid? categoryId,
            TransactionType? type,
            DateTime? startDate,
            DateTime? endDate,
            ClaimsPrincipal user,
            IListTransactionsUseCase useCase) =>
        {
            var result = await useCase.Execute(new ListTransactionsInput(
                user.GetUserId(), accountId, categoryId, type, startDate, endDate));
            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, IGetTransactionByIdUseCase useCase) =>
        {
            var result = await useCase.Execute(new GetTransactionByIdInput(user.GetUserId(), id));
            return Results.Ok(result);
        });

        group.MapPost("/", async (CreateTransactionRequest request, ClaimsPrincipal user, ICreateTransactionUseCase useCase) =>
        {
            var result = await useCase.Execute(new CreateTransactionInput(
                user.GetUserId(),
                request.AccountId,
                request.CategoryId,
                request.Type,
                request.Amount,
                request.Description,
                request.Date));
            return Results.Created($"/api/transactions/{result.Id}", result);
        });

        group.MapPost("/transfers", async (CreateTransferRequest request, ClaimsPrincipal user, ICreateTransferUseCase useCase) =>
        {
            var result = await useCase.Execute(new CreateTransferInput(
                user.GetUserId(),
                request.SourceAccountId,
                request.DestinationAccountId,
                request.Amount,
                request.Description,
                request.Date));
            return Results.Created($"/api/transactions/{result.Outgoing.Id}", result);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateTransactionRequest request, ClaimsPrincipal user, IUpdateTransactionUseCase useCase) =>
        {
            var result = await useCase.Execute(new UpdateTransactionInput(
                user.GetUserId(), id, request.Amount, request.Description, request.CategoryId));
            return Results.Ok(result);
        });

        group.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal user, IDeleteTransactionUseCase useCase) =>
        {
            await useCase.Execute(new DeleteTransactionInput(user.GetUserId(), id));
            return Results.NoContent();
        });

        return app;
    }
}
