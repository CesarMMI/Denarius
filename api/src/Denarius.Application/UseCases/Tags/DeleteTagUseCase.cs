using Denarius.Application.Commands;
using Denarius.Application.Exceptions;
using Denarius.Application.Interfaces.Tags;
using Denarius.Application.Results;
using Denarius.Domain.Interfaces;
using Denarius.Domain.ValueObjects;

namespace Denarius.Application.UseCases.Tags;

public class DeleteTagUseCase(
    ITagRepository tagRepository,
    ITransactionRepository transactionRepository) : IDeleteTagUseCase
{
    public async Task<TagResult> Execute(IdCommand command)
    {
        var id = new Identifier(command.Id);
        var tag = await tagRepository.FindByIdAsync(id);

        if (tag is null) throw new NotFoundException("Tag not found.");

        var transactions = (await transactionRepository.FindByTagAsync(tag.Id)).ToList();
        if (transactions.Count > 0)
        {
            foreach (var transaction in transactions) transaction.SetTag(null);
            await transactionRepository.UpdateBatchAsync(transactions);
        }

        await tagRepository.DeleteAsync(tag.Id);

        return new TagResult(tag);
    }
}