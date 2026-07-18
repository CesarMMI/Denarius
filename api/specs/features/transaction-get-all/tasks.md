# transaction-get-all — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Input/Output/Interface do use case (`ListTransactionsInput`, `TransactionOutput`, `IListTransactionsUseCase`)
      Verificação: `dotnet build` compila sem erros.
- [x] Exceção `InvalidDateRangeException` (`Denarius.Application/Exceptions/Transactions/InvalidDateRangeException.cs`)
      lançada quando `StartDate` é posterior a `EndDate`
      Verificação: `tests/Denarius.Api.Tests/Middleware/ExceptionMiddlewareTests.cs` confirma mapeamento para 400.
- [x] `ITransactionRepository.ListByUserAsync(Guid userId, Guid? accountId, Guid? categoryId, TransactionType? type, DateTime? startDate, DateTime? endDate)`
      filtra a query por dono da transação, aplica cada filtro opcional apenas quando informado, e ordena por
      `Date` e `CreatedAt` (descendente)
      Verificação: `tests/Denarius.Infrastructure.Tests/Repositories/TransactionRepositoryTests.cs` cobre
      listagem sem filtros, filtro por `AccountId`, `CategoryId`, `Type`, `StartDate`, `EndDate`, ausência de
      transações de outros usuários e ordenação por data descendente.
- [x] `ListTransactionsUseCase` valida `StartDate <= EndDate`, retorna todas as transações do usuário aplicando
      os filtros informados, ou lista vazia quando não há transações correspondentes
      Verificação: `tests/Denarius.Application.Tests/UseCases/Transactions/ListTransactionsUseCaseTests.cs` —
      sem filtros, lista vazia, todos os filtros repassados ao repositório, apenas `StartDate`, apenas
      `EndDate`, `StartDate == EndDate`, e `StartDate > EndDate` lançando `InvalidDateRangeException` sem
      chamar o repositório.
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      Verificação: `dotnet build` resolve `IListTransactionsUseCase` sem erro em runtime.
- [x] Endpoint `GET /api/transactions` mapeado em `TransactionEndpoints.MapTransactionEndpoints`, aceita
      `accountId`, `categoryId`, `type`, `startDate` e `endDate` como query string opcionais, `UserId`
      extraído do `ClaimsPrincipal`, requer autenticação
      Verificação: `tests/Denarius.Api.Tests/Endpoints/TransactionEndpointsTests.cs` — retorna 200 com a lista
      de transações, chama o use case com o `UserId` correto.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes
      de qualquer nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
