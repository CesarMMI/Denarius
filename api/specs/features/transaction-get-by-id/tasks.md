# transaction-get-by-id — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Input/Output/Interface do use case (`GetTransactionByIdInput`, `TransactionOutput`, `IGetTransactionByIdUseCase`)
      Verificação: `dotnet build` compila sem erros.
- [x] `ITransactionRepository.GetByIdAsync(Guid id, Guid userId)` filtra a query por dono da transação
      Verificação: `tests/Denarius.Infrastructure.Tests/Repositories/TransactionRepositoryTests.cs` cobre
      busca de transação existente do usuário e ausência de retorno para transação de outro usuário.
- [x] `GetTransactionByIdUseCase` retorna a transação encontrada ou lança `TransactionNotFoundException`
      quando o repositório retorna `null` (transação inexistente ou de outro usuário)
      Verificação: `tests/Denarius.Application.Tests/UseCases/Transactions/GetTransactionByIdUseCaseTests.cs`
      — happy path, transação inexistente, transação pertencente a outro usuário (mesmo comportamento).
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      Verificação: `dotnet build` resolve `IGetTransactionByIdUseCase` sem erro em runtime.
- [x] Endpoint `GET /api/transactions/{id}` mapeado em `TransactionEndpoints.MapTransactionEndpoints`,
      `UserId` extraído do `ClaimsPrincipal`, requer autenticação
      Verificação: `tests/Denarius.Api.Tests/Endpoints/TransactionEndpointsTests.cs` — retorna 200 com
      os dados da transação no happy path, chama o use case com `UserId`/`TransactionId` corretos.
- [x] Mapeamento de exceção → HTTP via `ExceptionMiddleware`
      Verificação: `TransactionNotFoundException` (herda `NotFoundException`) retorna 404.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes
      de qualquer nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
