using Denarius.Application.Domain.Commands.Categories;
using Denarius.Application.Domain.Queries.Categories;
using Denarius.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class CategoriesController(
    ICreateCategoryCommand createCategoryCommand,
    IDeleteCategoryCommand deleteCategoryCommand,
    IGetAllCategoriesCommand getAllCategoriesCommand,
    IUpdateCategoryCommand updateCategoryCommand,
    IGetCategoryTypesCommand getCategoryTypesCommand
) : ControllerBase
{
    [HttpGet("types")]
    public Task<IActionResult> GetTypes() => getCategoryTypesCommand
        .Execute(new())
        .Ok();

    [Authorize]
    [HttpGet]
    public Task<IActionResult> GetAll() => getAllCategoriesCommand
        .Execute(new GetAllCategoriesQuery()
            .WithUserId(HttpContext))
        .Ok();

    [Authorize]
    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateCategoryQuery body) => createCategoryCommand
        .Execute(body
            .WithUserId(HttpContext))
        .Created();

    [Authorize]
    [HttpPut("{id:int}")]
    public Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCategoryQuery body) => updateCategoryCommand
        .Execute(body
            .WithId(id)
            .WithUserId(HttpContext))
        .Ok();

    [Authorize]
    [HttpDelete("{id:int}")]
    public Task<IActionResult> Delete([FromRoute] int id) => deleteCategoryCommand
        .Execute(new DeleteCategoryQuery()
            .WithId(id)
            .WithUserId(HttpContext))
        .NoContent();
}
