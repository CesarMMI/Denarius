using Denarius.Application.Transactions.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Transactions.Commands.GetById;

public interface IGetTransactionByIdCommand : ICommand<GetTransactionByIdQuery, TransactionResult>
{
}
