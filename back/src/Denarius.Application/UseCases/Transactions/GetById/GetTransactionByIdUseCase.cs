using Denarius.Application.Exceptions;
using Denarius.Application.IO.Transactions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.UseCases.Transactions.GetById;

internal class GetTransactionByIdUseCase(ITransactionRepository transactionRepository) : IGetTransactionByIdUseCase
{
    public async Task<TransactionOutput> Execute(Guid input)
    {
        var transaction = await transactionRepository.GetByIdAsync(input)
            ?? throw new NotFoundException("Transação não encontrada.");

        return new TransactionOutput(transaction);
    }
}
