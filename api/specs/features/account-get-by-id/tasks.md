# account-get-by-id — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Input/Output/Interface do use case (`GetAccountByIdInput`, `AccountOutput`, `IGetAccountByIdUseCase`)
      Verificação: `dotnet build` compila sem erros.
- [x] `IAccountRepository.GetByIdAsync(Guid id, Guid userId)` filtra a query por dono da conta
      Verificação: `tests/Denarius.Infrastructure.Tests/Repositories/AccountRepositoryTests.cs` cobre
      busca de conta existente do usuário e ausência de retorno para conta de outro usuário.
- [x] `GetAccountByIdUseCase` retorna a conta encontrada ou lança `AccountNotFoundException`
      quando o repositório retorna `null` (conta inexistente ou de outro usuário)
      Verificação: `tests/Denarius.Application.Tests/UseCases/Accounts/GetAccountByIdUseCaseTests.cs`
      — happy path, conta inexistente, conta pertencente a outro usuário (mesmo comportamento).
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      Verificação: `dotnet build` resolve `IGetAccountByIdUseCase` sem erro em runtime.
- [x] Endpoint `GET /api/accounts/{id}` mapeado em `AccountEndpoints.MapAccountEndpoints`,
      `UserId` extraído do `ClaimsPrincipal`, requer autenticação
      Verificação: `tests/Denarius.Api.Tests/Endpoints/AccountEndpointsTests.cs` — retorna 200 com
      os dados da conta no happy path, chama o use case com `UserId`/`AccountId` corretos.
- [x] Mapeamento de exceção → HTTP via `ExceptionMiddleware`
      Verificação: `AccountNotFoundException` (herda `NotFoundException`) retorna 404.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes
      de qualquer nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
