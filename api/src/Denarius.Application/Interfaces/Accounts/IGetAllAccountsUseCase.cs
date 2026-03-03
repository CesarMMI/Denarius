using Denarius.Application.Commands;
using Denarius.Application.Results;

namespace Denarius.Application.Interfaces.Accounts;

public interface IGetAllAccountsUseCase : IUseCase<Command, IEnumerable<AccountResult>>
{
}
