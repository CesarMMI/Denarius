using Denarius.Application.Domain.Commands.Transactions;
using Denarius.Application.Domain.Queries.Transactions;
using Denarius.Application.Domain.Results.Transactions;
using Denarius.Application.Exceptions;
using Denarius.Application.Extensions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Commands.Transactions;

internal class GetAllTransactionsCommand(ITransactionRepository transactionRepository) : Command<GetAllTransactionsQuery, IEnumerable<TransactionResult>>, IGetAllTransactionsCommand
{
    protected override async Task<IEnumerable<TransactionResult>> Handle(GetAllTransactionsQuery query)
    {
        var transactions = await transactionRepository.FindManyAsync(t => t.UserId == query.UserId);
        return transactions.Select(a => a.ToResult());
    }

    protected override void Validate(GetAllTransactionsQuery query)
    {
        if (!query.UserId.IsValidId()) throw new BadRequestException("User id is required");
    }
}
