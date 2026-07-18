# transaction-create — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
POST /api/transactions
  → TransactionEndpoints.MapTransactionEndpoints (Denarius.Api/Endpoints/TransactionEndpoints.cs)
  → CreateTransactionRequest(AccountId, CategoryId, Type, Amount, Description, Date) +
    UserId extraído do ClaimsPrincipal (ClaimsPrincipalExtensions.GetUserId())
    → CreateTransactionInput(UserId, AccountId, CategoryId, Type, Amount, Description, Date)
  → ICreateTransactionUseCase → CreateTransactionUseCase (Denarius.Application/UseCases/Transactions/CreateTransactionUseCase.cs)
      1. IAccountRepository.GetByIdAsync(accountId, userId) — busca já filtrada por dono
      2. account is null → lança AccountNotFoundException(accountId)
      3. !account.IsActive → lança InactiveAccountException(accountId)
      4. ICategoryRepository.GetByIdAsync(categoryId, userId) — busca já filtrada por dono
      5. category is null → lança CategoryNotFoundException(categoryId)
      6. !category.AcceptsTransactionType(type) → lança InvalidCategoryException (domínio)
      7. new Transaction(userId, accountId, categoryId, transferPeerId: null, type, amount,
         description, date) — entidade valida Amount e Description no construtor, lança
         InvalidAmountException / InvalidDescriptionException se inválidos
      8. account.ApplyDelta(transaction.ToDelta()) — ToDelta() retorna +Amount para Income,
         -Amount para Expense; ApplyDelta soma ao Balance da conta em memória
      9. ITransactionRepository.AddAsync(transaction)
      10. IAccountRepository.UpdateAsync(account)
      11. IUnitOfWork.CommitAsync() — única chamada de commit do use case
  → TransactionOutput.FromEntity(transaction)
  → Results.Created($"/api/transactions/{result.Id}", result) — 201
```

## Decisões locais

- `IAccountRepository.GetByIdAsync(Guid id, Guid userId)` e `ICategoryRepository.GetByIdAsync(Guid id, Guid userId)`
  já filtram a query por dono (`FirstOrDefaultAsync(... && x.UserId == userId)`), mesmo padrão usado em
  `account-update`. "Não existe" e "pertence a outro usuário" chegam ao use case como o mesmo `null` —
  ambos os casos viram `AccountNotFoundException`/`CategoryNotFoundException` (mesmo princípio de
  não-enumeração usado em `Login`, ver `specs/ARCHITECTURE.md`).
- A ordem de validação é fixa: conta (existência → ativa) antes de categoria (existência →
  compatibilidade) antes da entidade `Transaction` (Amount → Description). Um `AccountId` inexistente
  nunca chega a consultar a categoria.
- A incompatibilidade entre `Category.Type` e `Transaction.Type` é checada no use case
  (`category.AcceptsTransactionType(input.Type)`, `Denarius.Domain/Entities/Category.cs`), não dentro
  do construtor de `Transaction` — a entidade `Transaction` só sabe validar a *presença* de categoria
  para `Income`/`Expense`, não a compatibilidade de tipo, que depende de outra entidade.
- `InvalidCategoryException` é uma exceção de **domínio** (`Denarius.Domain.Exceptions.Transactions`,
  herda `DomainException`), lançada diretamente pelo use case (não pela entidade) quando a categoria é
  incompatível — mapeada para HTTP 400 pelo `ExceptionMiddleware` como qualquer `DomainException`.
- `Amount` e `Description` são validados inteiramente no construtor de `Transaction`
  (`Denarius.Domain/Entities/Transaction.cs`) — o use case não duplica essas regras.
- `Transaction.ToDelta()` centraliza o sinal do efeito no saldo: `+Amount` para `Income`, `-Amount`
  para `Expense` (e `-Amount` para `Transfer`, fora de escopo aqui). `Account.ApplyDelta(delta)` soma
  o delta ao `Balance` e lança `InvalidDeltaException` se `delta == 0` — não pode acontecer neste fluxo
  porque `Amount` já é garantidamente positivo antes de chegar aqui.
- `CreateTransactionInput`/`CreateTransactionRequest` não recebem `TransferPeerId` nem
  `IsIncomingTransfer` — esses campos só existem para o fluxo de `CreateTransfer`; aqui a entidade é
  sempre construída com `transferPeerId: null`.
- Resposta HTTP usa `Results.Created` com `Location: /api/transactions/{id}`, consistente com o
  endpoint `GET /api/transactions/{id}` (`GetTransactionById`).

## Edge cases e como são tratados

- `AccountId` que nunca existiu ou pertence a outro usuário → `AccountNotFoundException` → 404
  (repositório já filtra por `UserId`, não há checagem de posse separada no use case).
- Conta existente e do usuário certo, porém `IsActive == false` → `InactiveAccountException` → 422
  (`InactiveAccountException` tem mapeamento HTTP dedicado no `ExceptionMiddleware`, diferente das
  demais `AppException` que caem em 400).
- `CategoryId` que nunca existiu ou pertence a outro usuário → `CategoryNotFoundException` → 404.
  Só é avaliado depois que a conta passa nas checagens de existência/atividade.
- Categoria `Expense` usada em transação `Income` (ou vice-versa) → `InvalidCategoryException` → 400.
- `Amount` zero ou negativo → `InvalidAmountException` → 400 (checado no construtor de `Transaction`,
  depois que conta e categoria já foram validadas).
- `Description` vazia, só espaços em branco ou nula → `InvalidDescriptionException` → 400.
- Requisição sem token JWT → 401, antes mesmo do use case ser executado (`RequireAuthorization()` no
  grupo de rotas).
- `Date` no passado ou no futuro → aceito sem restrição; não há validação de intervalo neste fluxo
  (diferente de `ListTransactions`, que tem `InvalidDateRangeException` para filtros de período).

## Alternativas descartadas

Nenhuma decisão de projeto alternativa registrada — implementação segue diretamente o padrão de use
case de escrita já estabelecido para os demais aggregates (Account, Category), com a adição de uma
segunda entidade lida (`Account`) sendo mutada como efeito colateral (`ApplyDelta`) dentro do mesmo
use case que persiste a nova `Transaction`.
