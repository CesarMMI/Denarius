using Denarius.Application.Commands;
using Denarius.Application.Results;

namespace Denarius.Application.Interfaces.Transactions;

public interface IGetAllTransactionsUseCase : IUseCase<Command, IEnumerable<TransactionResult>>
{
}
