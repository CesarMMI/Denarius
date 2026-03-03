using Denarius.Application.Commands.Transactions;
using Denarius.Application.Results;

namespace Denarius.Application.Interfaces.Transactions;

public interface IUpdateTransactionUseCase : IUseCase<UpdateTransactionCommand, Task<TransactionResult>>
{
}