using System.Security.Claims;
using Denarius.Application.Inputs.Accounts;
using Denarius.Application.Interfaces.UseCases.Accounts;
using Denarius.Api.Extensions;
using Denarius.Api.Requests.Accounts;

namespace Denarius.Api.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts").RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, IListAccountsUseCase useCase) =>
        {
            var result = await useCase.Execute(new ListAccountsInput(user.GetUserId()));
            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, IGetAccountByIdUseCase useCase) =>
        {
            var result = await useCase.Execute(new GetAccountByIdInput(user.GetUserId(), id));
            return Results.Ok(result);
        });

        group.MapPost("/", async (CreateAccountRequest request, ClaimsPrincipal user, ICreateAccountUseCase useCase) =>
        {
            var result = await useCase.Execute(new CreateAccountInput(user.GetUserId(), request.Name, request.CurrencyCode, request.Color));
            return Results.Created($"/api/accounts/{result.Id}", result);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateAccountRequest request, ClaimsPrincipal user, IUpdateAccountUseCase useCase) =>
        {
            var result = await useCase.Execute(new UpdateAccountInput(user.GetUserId(), id, request.Name, request.Color));
            return Results.Ok(result);
        });

        group.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal user, IDeactivateAccountUseCase useCase) =>
        {
            await useCase.Execute(new DeactivateAccountInput(user.GetUserId(), id));
            return Results.NoContent();
        });

        return app;
    }
}
