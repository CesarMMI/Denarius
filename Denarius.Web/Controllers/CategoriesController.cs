using Denarius.Application.Categories.Commands.Create;
using Denarius.Application.Categories.Commands.Delete;
using Denarius.Application.Categories.Commands.GetAll;
using Denarius.Application.Categories.Commands.GetById;
using Denarius.Application.Categories.Commands.GetTypes;
using Denarius.Application.Categories.Commands.Update;
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
    IGetCategoryByIdCommand getCategoryByIdCommand,
    IUpdateCategoryCommand updateCategoryCommand,
    IGetCategoryTypesCommand getCategoryTypesCommand
) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateCategoryQuery body)
    {
        body.SetRequestUserId(HttpContext);
        var account = await createCategoryCommand.Execute(body);
        return Created(HttpContext.Request.Path + "/" + account.Id, account);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var body = new DeleteCategoryQuery { Id = id };
        body.SetRequestUserId(HttpContext);
        await deleteCategoryCommand.Execute(body);
        return NoContent();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var body = new GetAllCategoriesQuery();
        body.SetRequestUserId(HttpContext);
        return Ok(await getAllCategoriesCommand.Execute(body));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var body = new GetCategoryByIdQuery { Id = id };
        body.SetRequestUserId(HttpContext);
        return Ok(await getCategoryByIdCommand.Execute(body));
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCategoryQuery body)
    {
        body.Id = id;
        body.SetRequestUserId(HttpContext);
        return Ok(await updateCategoryCommand.Execute(body));
    }

    [HttpGet("types")]
    public async Task<IActionResult> GetTypes()
    {
        return Ok(await getCategoryTypesCommand.Execute(new()));
    }
}
