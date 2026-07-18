# transaction-update — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
PUT /api/transactions/{id}
  → TransactionEndpoints.MapTransactionEndpoints (Denarius.Api/Endpoints/TransactionEndpoints.cs)
  → { id } da rota + UpdateTransactionRequest(Amount, Description, CategoryId) + UserId extraído do
    ClaimsPrincipal (ClaimsPrincipalExtensions.GetUserId())
    → UpdateTransactionInput(UserId, TransactionId, Amount, Description, CategoryId)
  → IUpdateTransactionUseCase → UpdateTransactionUseCase (Denarius.Application/UseCases/Transactions/UpdateTransactionUseCase.cs)
      1. ITransactionRepository.GetByIdAsync(transactionId, userId) — busca já filtrada por dono
      2. transaction is null → lança TransactionNotFoundException(transactionId)
      3. IAccountRepository.GetByIdAsync(transaction.AccountId, userId) — busca a conta da transação
         (não checada por null; ver "Decisões locais")
      4. transaction.UpdateAmount(input.Amount) — valida Amount > 0 (Denarius.Domain/Entities/Transaction.cs)
      5. Se o novo Amount difere do valor anterior:
         a. calcula `correction = AccountDelta(transaction, novoAmount) - AccountDelta(transaction, amountAntigo)`
            e aplica via `account.ApplyDelta(correction)`
         b. se `transaction.IsTransfer`, busca a transação par (`ITransactionRepository.GetByIdAsync(TransferPeerId, userId)`)
            e a conta dela, aplica a mesma correção de saldo na conta par, e chama `peer.UpdateAmount(novoAmount)`
            — o par recebe o mesmo valor novo (não uma correção proporcional)
      6. transaction.UpdateDescription(input.Description) — valida Description não vazia/nula
      7. Lógica de categoria:
         - Se `!transaction.IsTransfer`: exige `input.CategoryId.HasValue` (senão `InvalidCategoryException`),
           busca a categoria (`ICategoryRepository.GetByIdAsync`), lança `CategoryNotFoundException` se null,
           valida `category.AcceptsTransactionType(transaction.Type)` (senão `InvalidCategoryException`),
           então chama `transaction.UpdateCategory(input.CategoryId)`
         - Se `transaction.IsTransfer` e `input.CategoryId.HasValue`: chama `transaction.UpdateCategory(input.CategoryId)`
           diretamente, que sempre lança `InvalidCategoryException` porque `Transaction.UpdateCategory` rejeita
           qualquer categoria quando `IsTransfer` é true (ver "Decisões locais")
         - Se `transaction.IsTransfer` e `input.CategoryId` é null: nenhuma ação — CategoryId permanece null
      8. ITransactionRepository.UpdateAsync(transaction) + (se par existir) UpdateAsync(peer)
      9. IAccountRepository.UpdateAsync(account) + (se par existir) UpdateAsync(peerAccount)
      10. IUnitOfWork.CommitAsync() — única chamada de commit do use case, após todas as validações passarem
  → TransactionOutput.FromEntity(transaction)
  → Results.Ok(result) — 200
```

## Decisões locais

- `ITransactionRepository.GetByIdAsync(Guid id, Guid userId)` já filtra a query por dono, mesmo padrão
  usado em `account-update`/`category-update`. "Transação não existe" e "transação pertence a outro
  usuário" chegam ao use case como o mesmo `null` → ambos os casos viram `TransactionNotFoundException`
  (mesmo princípio de não-enumeração usado em `Login`, ver `specs/ARCHITECTURE.md`). O mesmo vale para
  `ICategoryRepository.GetByIdAsync(categoryId, userId)` ao trocar a categoria.
- O ajuste de saldo nunca é "somar o novo valor" — é sempre `AccountDelta(novoAmount) - AccountDelta(amountAntigo)`,
  onde `AccountDelta` (helper privado do use case) devolve o delta assinado que aquele tipo/perna de transação
  aplica à conta: `+amount` para `Income` e para a perna de entrada de uma transferência
  (`IsIncomingTransfer == true`), `-amount` para `Expense` e para a perna de saída. Isso garante que o saldo
  final reflita corretamente a diferença, independente de quanto a conta já tinha sido movimentada por outras
  transações.
- Ao mudar o `Amount` de uma transação `Transfer`, a perna par recebe exatamente o **mesmo novo valor**
  (`peer.UpdateAmount(input.Amount)`), não uma correção proporcional — as duas pernas de uma transferência
  sempre têm o mesmo `Amount` por construção (ver `specs/entities/transaction.md`).
- `account` e (quando aplicável) `peerAccount`/`peer` retornados pelos repositórios não são checados por null
  no use case (`account!`, `peerAccount!`, `peer!`) — a integridade referencial (uma transação sempre aponta
  para uma conta existente do mesmo usuário, e `TransferPeerId` sempre aponta para uma transação existente)
  é tratada como invariante garantida na criação, não revalidada aqui.
- `Transaction.UpdateCategory(categoryId)` (Denarius.Domain/Entities/Transaction.cs) tem duas regras próprias:
  lança `InvalidCategoryException` se `IsTransfer` for true (qualquer `categoryId`, mesmo não-nulo) e lança
  `InvalidCategoryException` se `categoryId` for null. Isso faz o use case não precisar duplicar essas checagens
  ao montar a chamada — só decide *se* chama `UpdateCategory`, a entidade decide se o valor é aceitável.
  Consequência observável: enviar `CategoryId` não-nulo em uma transação `Transfer` sempre resulta em erro,
  ainda que o valor "pareça" uma categoria válida do usuário — o use case nem chega a consultar
  `ICategoryRepository` nesse caso.
- Compatibilidade categoria↔tipo é verificada via `Category.AcceptsTransactionType(transaction.Type)`
  (Denarius.Domain/Entities/Category.cs) — mesma regra descrita em `specs/entities/category.md`
  ("Compatibilidade com transações").
- Todas as mutações de entidade (`UpdateAmount`, `ApplyDelta`, `UpdateDescription`, `UpdateCategory`) acontecem
  em memória antes de qualquer `UpdateAsync`/`CommitAsync` — como o use case só chama `CommitAsync()` uma vez,
  ao final, qualquer exceção lançada no meio do fluxo (ex.: categoria incompatível, descoberta só no passo 7,
  depois do `Amount` já ter sido validado/aplicado no passo 4-5) impede a persistência de qualquer mudança,
  já que nada foi salvo ainda.

## Edge cases e como são tratados

- `TransactionId` que nunca existiu, ou que existe mas é de outro usuário → mesmo resultado,
  `TransactionNotFoundException` → 404.
- `Amount` zero ou negativo → `InvalidAmountException` (`Denarius.Domain.Exceptions.Transactions`) → 400,
  lançada dentro de `transaction.UpdateAmount()`, antes de qualquer ajuste de saldo.
- `Description` vazia, só espaços ou nula → `InvalidDescriptionException` → 400.
- `CategoryId` novo que nunca existiu, ou que existe mas é de outro usuário → `CategoryNotFoundException` → 404
  (mesmo princípio de não-enumeração do passo de busca da transação).
- Categoria existente mas de tipo incompatível (`Expense` numa transação `Income`, por exemplo) →
  `InvalidCategoryException` → 400.
- `CategoryId` não-nulo enviado em uma transação `Transfer` → `InvalidCategoryException` → 400 (via
  `Transaction.UpdateCategory`, não há consulta ao repositório de categorias nesse caso).
- `CategoryId` nulo enviado em uma transação `Income`/`Expense` → `InvalidCategoryException` → 400, lançada
  pelo próprio use case antes de consultar `ICategoryRepository`.
- `Amount` alterado em uma transação `Transfer` → a perna par é buscada, seu saldo corrigido e seu `Amount`
  sincronizado, mesmo que o par pertença a uma conta diferente da transação editada (é sempre o caso, já que
  as duas pernas de uma transferência estão em contas distintas por construção).
- `Amount` enviado igual ao valor atual → nenhum ajuste de saldo é feito (bloco de correção só executa se
  `input.Amount != oldAmount`), mas `Description`/`CategoryId` ainda são processados normalmente.
- Requisição sem token JWT → 401, antes mesmo do use case ser executado (`RequireAuthorization()` no grupo
  de rotas).

## Alternativas descartadas

Nenhuma decisão de projeto alternativa registrada — implementação segue o mesmo padrão de use case de
escrita por id estabelecido em `specs/features/account-update/plan.md` e `specs/features/category-update/plan.md`,
estendido para lidar com o efeito colateral no saldo da conta e a sincronização da perna par de transferências,
que não existem nesses dois outros aggregates.
