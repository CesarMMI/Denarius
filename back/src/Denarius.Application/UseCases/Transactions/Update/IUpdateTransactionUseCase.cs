using Denarius.Application.IO.Transactions;

namespace Denarius.Application.UseCases.Transactions.Update;

public interface IUpdateTransactionUseCase : IUseCase<(Guid Id, UpdateTransactionInput Input), Task<TransactionOutput>>
{
}
