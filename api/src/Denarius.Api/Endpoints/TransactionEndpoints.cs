using System.Security.Claims;
using Denarius.Application.Inputs.Transactions;
using Denarius.Application.Interfaces.UseCases.Transactions;
using Denarius.Application.Outputs.Transactions;
using Denarius.Api.Extensions;
using Denarius.Api.Requests.Transactions;
using Denarius.Domain.Enums;

namespace Denarius.Api.Endpoints;

public static class TransactionEndpoints
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/transactions").RequireAuthorization().WithTags("Transactions");

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
        })
        .WithName("ListTransactions")
        .WithSummary("List transactions with optional filters")
        .Produces<IEnumerable<TransactionOutput>>()
        .ProducesProblem(400)
        .ProducesProblem(401);

        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, IGetTransactionByIdUseCase useCase) =>
        {
            var result = await useCase.Execute(new GetTransactionByIdInput(user.GetUserId(), id));
            return Results.Ok(result);
        })
        .WithName("GetTransactionById")
        .WithSummary("Get a transaction by id")
        .Produces<TransactionOutput>()
        .ProducesProblem(401)
        .ProducesProblem(404);

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
        })
        .WithName("CreateTransaction")
        .WithSummary("Create an income or expense transaction")
        .Produces<TransactionOutput>(201)
        .ProducesProblem(400)
        .ProducesProblem(401);

        group.MapPost("/transfer", async (CreateTransferRequest request, ClaimsPrincipal user, ICreateTransferUseCase useCase) =>
        {
            var result = await useCase.Execute(new CreateTransferInput(
                user.GetUserId(),
                request.SourceAccountId,
                request.DestinationAccountId,
                request.Amount,
                request.Description,
                request.Date));
            return Results.Created($"/api/transactions/{result.Outgoing.Id}", result);
        })
        .WithName("CreateTransfer")
        .WithSummary("Create a transfer between two accounts")
        .Produces<CreateTransferOutput>(201)
        .ProducesProblem(400)
        .ProducesProblem(401);

        group.MapPut("/{id:guid}", async (Guid id, UpdateTransactionRequest request, ClaimsPrincipal user, IUpdateTransactionUseCase useCase) =>
        {
            var result = await useCase.Execute(new UpdateTransactionInput(
                user.GetUserId(), id, request.Amount, request.Description, request.CategoryId));
            return Results.Ok(result);
        })
        .WithName("UpdateTransaction")
        .WithSummary("Update a transaction's amount, description, or category")
        .Produces<TransactionOutput>()
        .ProducesProblem(400)
        .ProducesProblem(401)
        .ProducesProblem(404);

        group.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal user, IDeleteTransactionUseCase useCase) =>
        {
            await useCase.Execute(new DeleteTransactionInput(user.GetUserId(), id));
            return Results.NoContent();
        })
        .WithName("DeleteTransaction")
        .WithSummary("Delete a transaction")
        .Produces(204)
        .ProducesProblem(401)
        .ProducesProblem(404);

        return app;
    }
}
