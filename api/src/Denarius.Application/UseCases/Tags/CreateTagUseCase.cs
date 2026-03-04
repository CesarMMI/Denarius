using Denarius.Application.Commands.Tags;
using Denarius.Application.Interfaces.Tags;
using Denarius.Application.Results;
using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces;
using Denarius.Domain.ValueObjects;

namespace Denarius.Application.UseCases.Tags;

internal class CreateTagUseCase(ITagRepository tagRepository) : ICreateTagUseCase
{
    public async Task<TagResult> Execute(CreateTagCommand command)
    {
        var tag = Tag.New(
            Name.New(command.Name, "tag name"),
            Color.New(command.Color));

        await tagRepository.AddAsync(tag);
        await tagRepository.SaveAsync();

        return new TagResult(tag);
    }
}
