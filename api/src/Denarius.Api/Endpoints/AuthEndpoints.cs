using Denarius.Application.Inputs.Auth;
using Denarius.Application.Interfaces.UseCases.Auth;
using Denarius.Api.Requests.Auth;

namespace Denarius.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (RegisterUserRequest request, IRegisterUserUseCase useCase) =>
        {
            var result = await useCase.Execute(new RegisterUserInput(request.Email, request.Password, request.Name));
            return Results.Created($"/api/users/{result.Id}", result);
        });

        group.MapPost("/login", async (LoginRequest request, ILoginUseCase useCase) =>
        {
            var result = await useCase.Execute(new LoginInput(request.Email, request.Password));
            return Results.Ok(result);
        });

        return app;
    }
}
