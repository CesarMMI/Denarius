using Denarius.Application.Inputs.Auth;
using Denarius.Application.Outputs.Auth;

namespace Denarius.Application.Interfaces.UseCases.Auth;

public interface IRegisterUserUseCase : IUseCase<RegisterUserInput, Task<UserOutput>>;
