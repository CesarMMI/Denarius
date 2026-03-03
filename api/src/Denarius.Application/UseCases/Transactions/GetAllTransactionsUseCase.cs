using Denarius.Application.Commands;
using Denarius.Application.Interfaces.Transactions;
using Denarius.Application.Results;
using Denarius.Domain.Interfaces;

namespace Denarius.Application.UseCases.Transactions;

internal class GetAllTransactionsUseCase(ITransactionRepository transactionRepository) : IGetAllTransactionsUseCase
{
    public IEnumerable<TransactionResult> Execute(Command command)
    {
        return transactionRepository.Find()
            .ToList()
            .Select(t => new TransactionResult(t));
    }
}