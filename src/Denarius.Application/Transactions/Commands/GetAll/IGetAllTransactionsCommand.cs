using Denarius.Application.Transactions.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Transactions.Commands.GetAll;

public interface IGetAllTransactionsCommand : ICommand<GetAllTransactionsQuery, IList<TransactionResult>>
{
}
