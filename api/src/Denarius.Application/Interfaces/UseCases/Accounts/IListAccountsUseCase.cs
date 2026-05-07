using Denarius.Application.Inputs.Accounts;
using Denarius.Application.Outputs.Accounts;

namespace Denarius.Application.Interfaces.UseCases.Accounts;

public interface IListAccountsUseCase : IUseCase<ListAccountsInput, Task<IEnumerable<AccountOutput>>>;
