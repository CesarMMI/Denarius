using Denarius.Application.Categories.Commands.Create;
using Denarius.Application.Categories.Commands.Delete;
using Denarius.Application.Categories.Commands.GetAll;
using Denarius.Application.Categories.Commands.GetById;
using Denarius.Application.Categories.Commands.GetTypes;
using Denarius.Application.Categories.Commands.Update;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class CategoriesController(
    ICreateCategoryCommand createCategoryCommand,
    IDeleteCategoryCommand deleteCategoryCommand,
    IGetAllCategoriesCommand getAllCategoriesCommand,
    IGetCategoryByIdCommand getCategoryByIdCommand,
    IUpdateCategoryCommand updateCategoryCommand,
    IGetCategoryTypesCommand getCategoryTypesCommand
) : Controller
{
    [HttpPost]
    [Authorize]
    public Task<IActionResult> Create([FromBody] CreateCategoryQuery body)
    {
        return HandleCommand(createCategoryCommand, body, account => Created(HttpContext.Request.Path + "/" + account.Id, account));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public Task<IActionResult> Delete([FromRoute] int id)
    {
        return HandleCommand(deleteCategoryCommand, new() { Id = id }, _ => NoContent());
    }

    [HttpGet]
    [Authorize]
    public Task<IActionResult> GetAll()
    {
        return HandleCommand(getAllCategoriesCommand, new());
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public Task<IActionResult> GetById([FromRoute] int id)
    {
        return HandleCommand(getCategoryByIdCommand, new() { Id = id });
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCategoryQuery body)
    {
        body.Id = id;
        return HandleCommand(updateCategoryCommand, body);
    }

    [HttpGet("types")]
    public Task<IActionResult> GetTypes()
    {
        return HandleCommand(getCategoryTypesCommand, new());
    }
}
