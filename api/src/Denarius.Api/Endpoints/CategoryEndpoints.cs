using System.Security.Claims;
using Denarius.Application.Inputs.Categories;
using Denarius.Application.Interfaces.UseCases.Categories;
using Denarius.Api.Extensions;
using Denarius.Api.Requests.Categories;
using Denarius.Domain.Enums;

namespace Denarius.Api.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").RequireAuthorization();

        group.MapGet("/", async (CategoryType? type, ClaimsPrincipal user, IListCategoriesUseCase useCase) =>
        {
            var result = await useCase.Execute(new ListCategoriesInput(user.GetUserId(), type));
            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, IGetCategoryByIdUseCase useCase) =>
        {
            var result = await useCase.Execute(new GetCategoryByIdInput(user.GetUserId(), id));
            return Results.Ok(result);
        });

        group.MapPost("/", async (CreateCategoryRequest request, ClaimsPrincipal user, ICreateCategoryUseCase useCase) =>
        {
            var result = await useCase.Execute(new CreateCategoryInput(user.GetUserId(), request.Name, request.Color, request.Type));
            return Results.Created($"/api/categories/{result.Id}", result);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest request, ClaimsPrincipal user, IUpdateCategoryUseCase useCase) =>
        {
            var result = await useCase.Execute(new UpdateCategoryInput(user.GetUserId(), id, request.Name, request.Color));
            return Results.Ok(result);
        });

        group.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal user, IDeleteCategoryUseCase useCase) =>
        {
            await useCase.Execute(new DeleteCategoryInput(user.GetUserId(), id));
            return Results.NoContent();
        });

        return app;
    }
}
