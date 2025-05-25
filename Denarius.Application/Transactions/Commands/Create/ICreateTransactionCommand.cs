using Denarius.Application.Transactions.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Transactions.Commands.Create;

public interface ICreateTransactionCommand : ICommand<CreateTransactionQuery, TransactionResult>
{
}
