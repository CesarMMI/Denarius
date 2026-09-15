using Denarius.Application.IO.Categories;
using Denarius.Application.UseCases.Categories.Create;
using Denarius.Application.UseCases.Categories.Delete;
using Denarius.Application.UseCases.Categories.GetById;
using Denarius.Application.UseCases.Categories.List;
using Denarius.Application.UseCases.Categories.Update;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ICreateCategoryUseCase createCategoryUseCase, IUpdateCategoryUseCase updateCategoryUseCase, IDeleteCategoryUseCase deleteCategoryUseCase, IListCategoriesUseCase listCategoriesUseCase, IGetCategoryByIdUseCase getCategoryByIdUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryOutput>>> List(
        [FromQuery] string? name,
        [FromQuery] bool? withTransaction,
        [FromQuery] DateTime? dateRef,
        [FromQuery] CategoryOrderField orderBy = CategoryOrderField.Name,
        [FromQuery] bool asc = true)
    {
        var output = await listCategoriesUseCase.Execute(new ListCategoriesInput(name, withTransaction, dateRef, orderBy, asc));
        return Ok(output);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryOutput>> GetById(Guid id)
    {
        var output = await getCategoryByIdUseCase.Execute(id);
        return Ok(output);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryOutput>> Create([FromBody] CreateCategoryInput input)
    {
        var output = await createCategoryUseCase.Execute(input);
        return Created($"/api/categories/{output.Id}", output);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryOutput>> Update(Guid id, [FromBody] UpdateCategoryInput input)
    {
        var output = await updateCategoryUseCase.Execute((id, input));
        return Ok(output);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await deleteCategoryUseCase.Execute(id);
        return NoContent();
    }
}
