# auth-register — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Entidade `User` valida `Email`, `PasswordHash` e `Name` no construtor
      Verificação: `tests/Denarius.Domain.Tests` cobre e-mail inválido, hash vazio e nome curto/vazio.
- [x] Input/Output/Interface do use case (`RegisterUserInput`, `UserOutput`, `IRegisterUserUseCase`)
      Verificação: `dotnet build` compila sem erros; `UserOutput.FromEntity` nunca expõe `PasswordHash`.
- [x] `RegisterUserUseCase` implementa checagem de e-mail duplicado, validação de senha,
      hash da senha, criação e persistência do usuário
      Verificação: `tests/Denarius.Application.Tests/UseCases/Auth/RegisterUserUseCaseTests.cs`
      — happy path, e-mail duplicado, senha curta, e-mail/nome inválidos, `AddAsync`/`CommitAsync`
      chamados exatamente uma vez.
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      Verificação: `dotnet build` resolve `IRegisterUserUseCase` sem erro em runtime.
- [x] Endpoint `POST /api/auth/register` mapeado em `AuthEndpoints.MapAuthEndpoints`
      Verificação: `tests/Denarius.Api.Tests/Endpoints/AuthEndpointsTests.cs` — retorna 201 com
      `Location: /api/users/{id}` no happy path e 400 nos cenários de erro (via `ExceptionMiddleware`).
- [x] Mapeamento de exceções → HTTP via `ExceptionMiddleware`
      Verificação: `EmailAlreadyInUseException`, `InvalidPasswordException` (ambas `AppException`)
      e `InvalidEmailException`/`InvalidNameException` (`DomainException`) retornam 400.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes
      de qualquer nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
