using Denarius.Application.Commands;
using Denarius.Application.Interfaces.Tags;
using Denarius.Application.Results;
using Denarius.Domain.Interfaces;

namespace Denarius.Application.UseCases.Tags;

public class GetAllTagsUseCase(ITagRepository tagRepository) : IGetAllTagsUseCase
{
    public IEnumerable<TagResult> Execute(Command command)
    {
        return tagRepository.Find()
            .ToList()
            .Select(t => new TagResult(t));
    }
}
