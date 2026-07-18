# account-deactivate — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Input/Interface do use case (`DeactivateAccountInput`, `IDeactivateAccountUseCase : IUseCase<DeactivateAccountInput, Task>`)
      Verificação: `dotnet build` compila sem erros.
- [x] `Account.Deactivate()` marca `IsActive = false` e é idempotente (early-return se já inativa)
      Verificação: `tests/Denarius.Domain.Tests/Entities/AccountTests.cs` cobre desativação de conta
      ativa e desativação repetida de conta já inativa.
- [x] `DeactivateAccountUseCase` busca a conta via `IAccountRepository.GetByIdAsync(accountId, userId)`,
      lança `AccountNotFoundException` quando `null`, chama `account.Deactivate()`, `UpdateAsync()` e
      `CommitAsync()` uma única vez
      Verificação: `tests/Denarius.Application.Tests/UseCases/Accounts/DeactivateAccountUseCaseTests.cs`
      — conta ativa é desativada, `UpdateAsync`/`CommitAsync` chamados uma vez, conta já inativa é
      idempotente (sem exceção), conta inexistente lança `AccountNotFoundException`, conta de outro
      usuário lança `AccountNotFoundException`, conta inexistente não chama `UpdateAsync`.
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      Verificação: `dotnet build` resolve `IDeactivateAccountUseCase` sem erro em runtime.
- [x] Endpoint `DELETE /api/accounts/{id}` mapeado em `AccountEndpoints.MapAccountEndpoints`, `UserId`
      extraído do `ClaimsPrincipal`, requer autenticação, responde `204 No Content` sem body
      Verificação: `tests/Denarius.Api.Tests/Endpoints/AccountEndpointsTests.cs` —
      `DeactivateAccount_Returns204NoContent` chama o use case com `UserId`/`AccountId` corretos.
- [x] Mapeamento de exceção → HTTP via `ExceptionMiddleware`
      Verificação: `AccountNotFoundException` (herda `NotFoundException`) retorna 404.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes
      de qualquer nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
