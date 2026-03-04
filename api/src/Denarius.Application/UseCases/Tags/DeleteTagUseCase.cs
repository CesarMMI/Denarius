using Denarius.Application.Commands;
using Denarius.Application.Exceptions;
using Denarius.Application.Interfaces.Tags;
using Denarius.Application.Results;
using Denarius.Domain.Interfaces;

namespace Denarius.Application.UseCases.Tags;

internal class DeleteTagUseCase(
    ITagRepository tagRepository,
    ITransactionRepository transactionRepository) : IDeleteTagUseCase
{
    public async Task<TagResult> Execute(IdCommand command)
    {
        var tag = await tagRepository.FindByIdAsync(command.Id);

        if (tag is null) throw new NotFoundException("Tag not found.");

        var transactions = (await transactionRepository.FindByTagAsync(tag.Id)).ToList();

        if (transactions.Count > 0)
        {
            foreach (var transaction in transactions) transaction.SetTag(null);
            transactionRepository.UpdateBatch(transactions);
            await transactionRepository.SaveAsync();
        }

        tagRepository.Delete(tag);
        await tagRepository.SaveAsync();

        return new TagResult(tag);
    }
}