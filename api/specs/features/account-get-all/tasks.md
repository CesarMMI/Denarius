# account-get-all — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Input/Output/Interface do use case (`ListAccountsInput`, `AccountOutput`, `IListAccountsUseCase`)
      Verificação: `dotnet build` compila sem erros.
- [x] `IAccountRepository.ListByUserAsync(Guid userId)` filtra a query por dono da conta e ordena por `Name`
      Verificação: `tests/Denarius.Infrastructure.Tests/Repositories/AccountRepositoryTests.cs` cobre
      listagem de contas do usuário e ausência de contas de outros usuários no resultado.
- [x] `ListAccountsUseCase` retorna todas as contas do usuário, incluindo inativas, ou lista vazia
      quando o usuário não tem nenhuma conta
      Verificação: `tests/Denarius.Application.Tests/UseCases/Accounts/ListAccountsUseCaseTests.cs`
      — múltiplas contas, contas inativas incluídas, lista vazia.
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      Verificação: `dotnet build` resolve `IListAccountsUseCase` sem erro em runtime.
- [x] Endpoint `GET /api/accounts` mapeado em `AccountEndpoints.MapAccountEndpoints`,
      `UserId` extraído do `ClaimsPrincipal`, requer autenticação
      Verificação: `tests/Denarius.Api.Tests/Endpoints/AccountEndpointsTests.cs` — retorna 200 com
      a lista de contas, chama o use case com o `UserId` correto.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes
      de qualquer nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
