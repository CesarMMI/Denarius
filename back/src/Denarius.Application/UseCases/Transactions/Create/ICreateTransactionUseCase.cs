using Denarius.Application.IO.Transactions;

namespace Denarius.Application.UseCases.Transactions.Create;

public interface ICreateTransactionUseCase : IUseCase<CreateTransactionInput, Task<TransactionOutput>>
{
}
