using Denarius.Application.Domain.Results.Transactions;
using Denarius.Application.Domain.Queries.Transactions;

namespace Denarius.Application.Domain.Commands.Transactions;

public interface IUpdateTransactionCommand : ICommand<UpdateTransactionQuery, TransactionResult>
{
}
