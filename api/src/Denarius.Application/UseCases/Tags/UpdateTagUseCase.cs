using Denarius.Application.Commands.Tags;
using Denarius.Application.Exceptions;
using Denarius.Application.Interfaces.Tags;
using Denarius.Application.Results;
using Denarius.Domain.Interfaces;
using Denarius.Domain.ValueObjects;

namespace Denarius.Application.UseCases.Tags;

internal class UpdateTagUseCase(ITagRepository tagRepository) : IUpdateTagUseCase
{
    public async Task<TagResult> Execute(UpdateTagCommand command)
    {
        var id = new Identifier(command.Id);
        var tag = await tagRepository.FindByIdAsync(id);
        
        if (tag is null)
            throw new NotFoundException("Tag not found.");

        tag.Rename(new Name(command.Name ?? string.Empty, "tag name"));
        tag.ChangeColor(new Color(command.Color ?? string.Empty));

        tagRepository.Update(tag);

        return new TagResult(tag);
    }
}