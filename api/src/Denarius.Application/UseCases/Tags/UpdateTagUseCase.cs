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
        var tag = await tagRepository.FindByIdAsync(command.Id);

        if (tag is null) throw new NotFoundException("Tag not found.");

        tag.Rename(Name.New(command.Name, "tag name"));
        tag.ChangeColor(Color.New(command.Color));

        tagRepository.Update(tag);
        await tagRepository.SaveAsync();

        return new TagResult(tag);
    }
}