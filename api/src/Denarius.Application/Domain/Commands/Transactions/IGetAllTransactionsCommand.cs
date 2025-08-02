using Denarius.Application.Domain.Results.Transactions;
using Denarius.Application.Domain.Queries.Transactions;

namespace Denarius.Application.Domain.Commands.Transactions;

public interface IGetAllTransactionsCommand : ICommand<GetAllTransactionsQuery, IEnumerable<TransactionResult>>
{
}
