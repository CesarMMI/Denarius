using Denarius.Application.Commands.Tags;
using Denarius.Application.Interfaces.Tags;
using Denarius.Application.Results;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.API.Controllers
{
    [ApiController]
    [Route("tags")]
    public class TagsController(
        ICreateTagUseCase createTagUseCase,
        IDeleteTagUseCase deleteTagUseCase,
        IGetAllTagsUseCase getAllTagsUseCase,
        IUpdateTagUseCase updateTagUseCase) : ControllerBase
    {

        [HttpGet]
        public IEnumerable<TagResult> GetAll() =>
            getAllTagsUseCase.Execute(new());

        [HttpPost]
        public Task<TagResult> Create([FromBody] CreateTagCommand command) =>
            createTagUseCase.Execute(command);

        [HttpPut("{id:Guid}")]
        public Task<TagResult> Update(Guid id, [FromBody] UpdateTagCommand command) =>
            updateTagUseCase.Execute(command with { Id = id });

        [HttpDelete("{id:Guid}")]
        public Task<TagResult> Delete(Guid id) =>
            deleteTagUseCase.Execute(new() { Id = id });
    }
}
