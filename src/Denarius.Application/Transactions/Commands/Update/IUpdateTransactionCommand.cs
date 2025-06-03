using Denarius.Application.Transactions.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Transactions.Commands.Update;

public interface IUpdateTransactionCommand : ICommand<UpdateTransactionQuery, TransactionResult>
{
}
