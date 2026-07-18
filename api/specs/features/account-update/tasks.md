# account-update — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Input/Output/Interface do use case (`UpdateAccountInput`, `AccountOutput`, `IUpdateAccountUseCase`)
      Verificação: `dotnet build` compila sem erros.
- [x] `Account.Update(name, color)` valida `Name`/`Color` e não aceita `currencyCode` como parâmetro
      Verificação: `tests/Denarius.Domain.Tests/Entities/AccountTests.cs` cobre update válido e
      lançamento de `InvalidNameException`/`InvalidColorException` para valores vazios/nulos.
- [x] `UpdateAccountUseCase` busca a conta via `IAccountRepository.GetByIdAsync(accountId, userId)`,
      lança `AccountNotFoundException` quando `null`, chama `account.Update()`, `UpdateAsync()` e
      `CommitAsync()` uma única vez
      Verificação: `tests/Denarius.Application.Tests/UseCases/Accounts/UpdateAccountUseCaseTests.cs`
      — happy path, moeda não alterada, `UpdateAsync`/`CommitAsync` chamados uma vez, conta
      inexistente, conta de outro usuário, nome/cor inválidos.
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      Verificação: `dotnet build` resolve `IUpdateAccountUseCase` sem erro em runtime.
- [x] Endpoint `PUT /api/accounts/{id}` mapeado em `AccountEndpoints.MapAccountEndpoints`, recebe
      `UpdateAccountRequest(Name, Color)`, `UserId` extraído do `ClaimsPrincipal`, requer autenticação
      Verificação: `tests/Denarius.Api.Tests/Endpoints/AccountEndpointsTests.cs` — retorna 200 com
      os dados atualizados da conta no happy path, chama o use case com `UserId`/`AccountId`/`Name`/`Color`
      corretos.
- [x] Mapeamento de exceção → HTTP via `ExceptionMiddleware`
      Verificação: `AccountNotFoundException` (herda `NotFoundException`) retorna 404;
      `InvalidNameException`/`InvalidColorException` (herdam `DomainException`) retornam 400.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes
      de qualquer nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
