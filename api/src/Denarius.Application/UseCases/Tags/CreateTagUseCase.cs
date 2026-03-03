using Denarius.Application.Commands.Tags;
using Denarius.Application.Interfaces.Tags;
using Denarius.Application.Results;
using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces;
using Denarius.Domain.ValueObjects;

namespace Denarius.Application.UseCases.Tags;

public class CreateTagUseCase(ITagRepository tagRepository) : ICreateTagUseCase
{
    public async Task<TagResult> Execute(CreateTagCommand command)
    {
        var tag = new Tag(
            Identifier.New(),
            new Name(command.Name ?? string.Empty, "tag name"),
            new Color(command.Color ?? string.Empty));

        await tagRepository.AddAsync(tag);

        return new TagResult(tag);
    }
}
