using System.Security.Claims;
using Denarius.Application.Inputs.Categories;
using Denarius.Application.Interfaces.UseCases.Categories;
using Denarius.Application.Outputs.Categories;
using Denarius.Api.Extensions;
using Denarius.Api.Requests.Categories;
using Denarius.Domain.Enums;

namespace Denarius.Api.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").RequireAuthorization().WithTags("Categories");

        group.MapGet("/", async (CategoryType? type, ClaimsPrincipal user, IListCategoriesUseCase useCase) =>
        {
            var result = await useCase.Execute(new ListCategoriesInput(user.GetUserId(), type));
            return Results.Ok(result);
        })
        .WithName("ListCategories")
        .WithSummary("List categories, optionally filtered by type")
        .Produces<IEnumerable<CategoryOutput>>()
        .ProducesProblem(401);

        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, IGetCategoryByIdUseCase useCase) =>
        {
            var result = await useCase.Execute(new GetCategoryByIdInput(user.GetUserId(), id));
            return Results.Ok(result);
        })
        .WithName("GetCategoryById")
        .WithSummary("Get a category by id")
        .Produces<CategoryOutput>()
        .ProducesProblem(401)
        .ProducesProblem(404);

        group.MapPost("/", async (CreateCategoryRequest request, ClaimsPrincipal user, ICreateCategoryUseCase useCase) =>
        {
            var result = await useCase.Execute(new CreateCategoryInput(user.GetUserId(), request.Name, request.Color, request.Type));
            return Results.Created($"/api/categories/{result.Id}", result);
        })
        .WithName("CreateCategory")
        .WithSummary("Create a new category")
        .Produces<CategoryOutput>(201)
        .ProducesProblem(400)
        .ProducesProblem(401);

        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest request, ClaimsPrincipal user, IUpdateCategoryUseCase useCase) =>
        {
            var result = await useCase.Execute(new UpdateCategoryInput(user.GetUserId(), id, request.Name, request.Color));
            return Results.Ok(result);
        })
        .WithName("UpdateCategory")
        .WithSummary("Update a category's name and color")
        .Produces<CategoryOutput>()
        .ProducesProblem(400)
        .ProducesProblem(401)
        .ProducesProblem(404);

        group.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal user, IDeleteCategoryUseCase useCase) =>
        {
            await useCase.Execute(new DeleteCategoryInput(user.GetUserId(), id));
            return Results.NoContent();
        })
        .WithName("DeleteCategory")
        .WithSummary("Delete a category")
        .Produces(204)
        .ProducesProblem(401)
        .ProducesProblem(404);

        return app;
    }
}
