# transaction-get-by-id — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
GET /api/transactions/{id}
  → TransactionEndpoints.MapTransactionEndpoints (Denarius.Api/Endpoints/TransactionEndpoints.cs)
  → { id } da rota + UserId extraído do ClaimsPrincipal (ClaimsPrincipalExtensions.GetUserId())
    → GetTransactionByIdInput(UserId, TransactionId)
  → IGetTransactionByIdUseCase → GetTransactionByIdUseCase (Denarius.Application/UseCases/Transactions/GetTransactionByIdUseCase.cs)
      1. ITransactionRepository.GetByIdAsync(transactionId, userId) — busca já filtrada por dono
      2. transaction is null → lança TransactionNotFoundException(transactionId)
  → TransactionOutput.FromEntity(transaction)
  → Results.Ok(result) — 200
```

## Decisões locais

- `ITransactionRepository.GetByIdAsync(Guid id, Guid userId)` (`Denarius.Infrastructure/Persistence/Repositories/TransactionRepository.cs`)
  já recebe `userId` como parâmetro e filtra no próprio `FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId)`.
  Isso significa que "transação não existe" e "transação pertence a outro usuário" chegam ao use case como o
  mesmo resultado (`null`) — o use case não distingue os dois casos, ambos viram `TransactionNotFoundException`.
  Efeito colateral desejado: evita vazar para o chamador se um `TransactionId` existe mas pertence a
  outro usuário (mesmo princípio de não-enumeração já usado em `Login`, `account-get-by-id` e
  `category-get-by-id`, ver `specs/ARCHITECTURE.md`).
- `TransactionNotFoundException` herda `NotFoundException` → mapeada para HTTP 404 pelo `ExceptionMiddleware`.
- O output (`TransactionOutput`) não expõe dados da `Account` ou `Category` relacionadas, apenas os
  respectivos ids (`AccountId`, `CategoryId`) — nenhum `include`/join é feito na consulta.
- A perna de destino de uma transferência (`IsIncomingTransfer = true`) é consultável pelo seu próprio
  `Id` como qualquer outra transação; não há lógica especial que redirecione para a perna de origem.

## Edge cases e como são tratados

- `TransactionId` que nunca existiu → `TransactionNotFoundException` → 404.
- `TransactionId` existente mas de outro usuário → mesmo resultado do caso acima, `TransactionNotFoundException` → 404
  (repositório já filtra por `UserId` na query, não há checagem de posse separada no use case).
- `TransactionId` de uma transação do tipo `Transfer` → retorna normalmente, incluindo `TransferPeerId`
  e `IsIncomingTransfer`; a transação par não é buscada nem incluída na resposta.
- Requisição sem token JWT → 401, antes mesmo do use case ser executado (`RequireAuthorization()` no grupo de rotas).

## Alternativas descartadas

Nenhuma decisão de projeto alternativa registrada — implementação segue diretamente o padrão de
use case de leitura por id já estabelecido para os demais aggregates (Account, Category).
