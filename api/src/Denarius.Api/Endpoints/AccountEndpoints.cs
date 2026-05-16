using Denarius.Application.Inputs.Accounts;
using Denarius.Application.Interfaces.UseCases.Accounts;
using Denarius.Api.Requests.Accounts;

namespace Denarius.Api.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts");

        group.MapGet("/", async (IListAccountsUseCase useCase) =>
        {
            var result = await useCase.Execute(new ListAccountsInput(CurrentUser.Id));
            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", async (Guid id, IGetAccountByIdUseCase useCase) =>
        {
            var result = await useCase.Execute(new GetAccountByIdInput(CurrentUser.Id, id));
            return Results.Ok(result);
        });

        group.MapPost("/", async (CreateAccountRequest request, ICreateAccountUseCase useCase) =>
        {
            var result = await useCase.Execute(new CreateAccountInput(CurrentUser.Id, request.Name, request.CurrencyCode, request.Color));
            return Results.Created($"/api/accounts/{result.Id}", result);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateAccountRequest request, IUpdateAccountUseCase useCase) =>
        {
            var result = await useCase.Execute(new UpdateAccountInput(CurrentUser.Id, id, request.Name, request.Color));
            return Results.Ok(result);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IDeactivateAccountUseCase useCase) =>
        {
            await useCase.Execute(new DeactivateAccountInput(CurrentUser.Id, id));
            return Results.NoContent();
        });

        return app;
    }
}
