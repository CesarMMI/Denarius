using Denarius.Application.Transactions.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Transactions.Commands.Delete;

public interface IDeleteTransactionCommand : ICommand<DeleteTransactionQuery, TransactionResult>
{
}
