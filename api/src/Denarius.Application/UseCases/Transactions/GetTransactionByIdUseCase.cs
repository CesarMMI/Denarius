using Denarius.Application.Exceptions.Transactions;
using Denarius.Application.Inputs.Transactions;
using Denarius.Application.Interfaces.UseCases.Transactions;
using Denarius.Application.Outputs.Transactions;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.Transactions;

public class GetTransactionByIdUseCase(ITransactionRepository transactionRepository)
    : IGetTransactionByIdUseCase
{
    public async Task<TransactionOutput> Execute(GetTransactionByIdInput input)
    {
        var transaction = await transactionRepository.GetByIdAsync(input.TransactionId, input.UserId);

        if (transaction is null)
            throw new TransactionNotFoundException(input.TransactionId);

        return TransactionOutput.FromEntity(transaction);
    }
}
