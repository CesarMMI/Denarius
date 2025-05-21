using Denarius.Application.Accounts.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Accounts.Commands.GetAll;

public interface IGetAllAccountsCommand : ICommand<GetAllAccountsQuery, IList<AccountResult>>
{
}
