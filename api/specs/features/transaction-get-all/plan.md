# transaction-get-all — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
GET /api/transactions?accountId={id}&categoryId={id}&type={Income|Expense|Transfer}&startDate={date}&endDate={date}
  → TransactionEndpoints.MapTransactionEndpoints (Denarius.Api/Endpoints/TransactionEndpoints.cs)
  → accountId, categoryId, type, startDate, endDate vinculados do query string,
    UserId extraído do ClaimsPrincipal (ClaimsPrincipalExtensions.GetUserId())
    → ListTransactionsInput(UserId, AccountId, CategoryId, Type, StartDate, EndDate)
  → IListTransactionsUseCase → ListTransactionsUseCase (Denarius.Application/UseCases/Transactions/ListTransactionsUseCase.cs)
      1. Valida StartDate <= EndDate (quando ambos informados) — lança InvalidDateRangeException caso contrário
      2. ITransactionRepository.ListByUserAsync(userId, accountId, categoryId, type, startDate, endDate)
         — busca já filtrada por dono e pelos filtros opcionais informados
      3. transactions.Select(TransactionOutput.FromEntity)
  → Results.Ok(result) — 200, IEnumerable<TransactionOutput>
```

## Decisões locais

- `ITransactionRepository.ListByUserAsync(Guid userId, Guid? accountId, Guid? categoryId, TransactionType? type, DateTime? startDate, DateTime? endDate)`
  (`Denarius.Infrastructure/Persistence/Repositories/TransactionRepository.cs`) filtra por `UserId` diretamente na
  query (`Where(t => t.UserId == userId)`) e aplica cada filtro opcional apenas quando informado
  (`accountId.HasValue`, `categoryId.HasValue`, `type.HasValue`, `startDate.HasValue`, `endDate.HasValue`) — tudo
  resolvido na query do banco, não em memória.
- `startDate`/`endDate` usam comparação inclusiva: `Date >= startDate.Value` e `Date <= endDate.Value`.
- Ordenação: `OrderByDescending(t => t.Date).ThenByDescending(t => t.CreatedAt)` — mais recentes primeiro, com
  desempate por data de criação. Decisão do repositório, não exposta como opção configurável pelo use case ou
  endpoint (mesmo padrão de `account-get-all` e `category-get-all`, que também fixam a ordenação no repositório).
- A validação `StartDate > EndDate` acontece no use case (`ListTransactionsUseCase.Execute`), antes de chamar o
  repositório — `StartDate` e `EndDate` isolados (sem o outro) nunca disparam erro, só a combinação inválida dos
  dois.
- Sem paginação: a query retorna todas as transações do usuário (filtradas) de uma vez, mesmo padrão de
  `account-get-all`/`category-get-all`.

## Edge cases e como são tratados

- Usuário sem nenhuma transação (ou nenhuma correspondente aos filtros) → `ListByUserAsync` retorna lista vazia →
  endpoint responde 200 com `[]` (não é um erro).
- Apenas `StartDate` informado (sem `EndDate`), ou vice-versa → nenhuma validação de intervalo é disparada, o
  filtro correspondente é aplicado isoladamente.
- `StartDate == EndDate` → válido, não lança `InvalidDateRangeException` (a checagem é estritamente `>`).
- `StartDate > EndDate` → `InvalidDateRangeException` (herda `AppException` → 400 via `ExceptionMiddleware`),
  e o repositório não chega a ser consultado.
- Filtros ausentes na query string chegam como `null` aos parâmetros nullable do endpoint (`Guid?`,
  `TransactionType?`, `DateTime?`) → nenhum filtro correspondente é aplicado, comportamento idêntico a "listar
  todas".
- Requisição sem token JWT → 401, antes mesmo do use case ser executado (`RequireAuthorization()` no grupo de
  rotas).

## Alternativas descartadas

Nenhuma decisão de projeto alternativa registrada — implementação segue diretamente o padrão de use case de
listagem já estabelecido para os demais aggregates (Account, Category), com a adição dos filtros opcionais por
`AccountId`, `CategoryId`, `Type` e período (`StartDate`/`EndDate`) e a validação de intervalo de datas.
