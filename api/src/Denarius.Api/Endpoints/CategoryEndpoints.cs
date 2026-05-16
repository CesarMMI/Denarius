using Denarius.Application.Inputs.Categories;
using Denarius.Application.Interfaces.UseCases.Categories;
using Denarius.Api.Requests.Categories;
using Denarius.Domain.Enums;

namespace Denarius.Api.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories");

        group.MapGet("/", async (CategoryType? type, IListCategoriesUseCase useCase) =>
        {
            var result = await useCase.Execute(new ListCategoriesInput(CurrentUser.Id, type));
            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", async (Guid id, IGetCategoryByIdUseCase useCase) =>
        {
            var result = await useCase.Execute(new GetCategoryByIdInput(CurrentUser.Id, id));
            return Results.Ok(result);
        });

        group.MapPost("/", async (CreateCategoryRequest request, ICreateCategoryUseCase useCase) =>
        {
            var result = await useCase.Execute(new CreateCategoryInput(CurrentUser.Id, request.Name, request.Color, request.Type));
            return Results.Created($"/api/categories/{result.Id}", result);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest request, IUpdateCategoryUseCase useCase) =>
        {
            var result = await useCase.Execute(new UpdateCategoryInput(CurrentUser.Id, id, request.Name, request.Color));
            return Results.Ok(result);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IDeleteCategoryUseCase useCase) =>
        {
            await useCase.Execute(new DeleteCategoryInput(CurrentUser.Id, id));
            return Results.NoContent();
        });

        return app;
    }
}
