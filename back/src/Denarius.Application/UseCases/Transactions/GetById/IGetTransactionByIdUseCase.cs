using Denarius.Application.IO.Transactions;

namespace Denarius.Application.UseCases.Transactions.GetById;

public interface IGetTransactionByIdUseCase : IUseCase<Guid, Task<TransactionOutput>>
{
}
