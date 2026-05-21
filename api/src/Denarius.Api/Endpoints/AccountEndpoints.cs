using System.Security.Claims;
using Denarius.Application.Inputs.Accounts;
using Denarius.Application.Interfaces.UseCases.Accounts;
using Denarius.Application.Outputs.Accounts;
using Denarius.Api.Extensions;
using Denarius.Api.Requests.Accounts;

namespace Denarius.Api.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts").RequireAuthorization().WithTags("Accounts");

        group.MapGet("/", async (ClaimsPrincipal user, IListAccountsUseCase useCase) =>
        {
            var result = await useCase.Execute(new ListAccountsInput(user.GetUserId()));
            return Results.Ok(result);
        })
        .WithName("ListAccounts")
        .WithSummary("List all accounts of the authenticated user")
        .Produces<IEnumerable<AccountOutput>>()
        .ProducesProblem(401);

        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, IGetAccountByIdUseCase useCase) =>
        {
            var result = await useCase.Execute(new GetAccountByIdInput(user.GetUserId(), id));
            return Results.Ok(result);
        })
        .WithName("GetAccountById")
        .WithSummary("Get an account by id")
        .Produces<AccountOutput>()
        .ProducesProblem(401)
        .ProducesProblem(404);

        group.MapPost("/", async (CreateAccountRequest request, ClaimsPrincipal user, ICreateAccountUseCase useCase) =>
        {
            var result = await useCase.Execute(new CreateAccountInput(user.GetUserId(), request.Name, request.CurrencyCode, request.Color));
            return Results.Created($"/api/accounts/{result.Id}", result);
        })
        .WithName("CreateAccount")
        .WithSummary("Create a new account")
        .Produces<AccountOutput>(201)
        .ProducesProblem(400)
        .ProducesProblem(401);

        group.MapPut("/{id:guid}", async (Guid id, UpdateAccountRequest request, ClaimsPrincipal user, IUpdateAccountUseCase useCase) =>
        {
            var result = await useCase.Execute(new UpdateAccountInput(user.GetUserId(), id, request.Name, request.Color));
            return Results.Ok(result);
        })
        .WithName("UpdateAccount")
        .WithSummary("Update an account's name and color")
        .Produces<AccountOutput>()
        .ProducesProblem(400)
        .ProducesProblem(401)
        .ProducesProblem(404);

        group.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal user, IDeactivateAccountUseCase useCase) =>
        {
            await useCase.Execute(new DeactivateAccountInput(user.GetUserId(), id));
            return Results.NoContent();
        })
        .WithName("DeactivateAccount")
        .WithSummary("Deactivate an account")
        .Produces(204)
        .ProducesProblem(401)
        .ProducesProblem(404);

        return app;
    }
}
