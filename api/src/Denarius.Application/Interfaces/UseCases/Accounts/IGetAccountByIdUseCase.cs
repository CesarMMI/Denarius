using Denarius.Application.Inputs.Accounts;
using Denarius.Application.Outputs.Accounts;

namespace Denarius.Application.Interfaces.UseCases.Accounts;

public interface IGetAccountByIdUseCase : IUseCase<GetAccountByIdInput, Task<AccountOutput>>;
