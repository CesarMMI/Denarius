# transaction-update

Permite que um usuário autenticado atualize o valor, a descrição e a categoria de uma transação existente, mantendo o saldo da conta (e, em transferências, da conta par) consistente com o novo valor.

> Entidade(s) de referência: `specs/entities/transaction.md`, `specs/entities/account.md`, `specs/entities/category.md`

## Entrada
- `TransactionId`
- `Amount`
- `Description`
- `CategoryId` (opcional — ausente/nulo em transações do tipo `Transfer`)

> `UserId` vem do token JWT do usuário autenticado, não é informado explicitamente pelo chamador.

## Campos editáveis
- `Amount`
- `Description`
- `CategoryId`

`Type` não pode ser alterado após a criação da transação — não faz parte da entrada desta feature.

## Happy path
- Transação com o `TransactionId` informado existe e pertence ao usuário autenticado
- `Amount` é positivo e `Description` não é vazia/nula
- Transação atualizada com os novos valores de `Amount`, `Description` e `CategoryId`
- Se `Amount` for alterado, o saldo da conta é recalculado pela diferença entre o valor anterior e o novo valor (não é uma simples soma do novo valor)
- Se a transação for do tipo `Transfer` e `Amount` for alterado, a transação par (`TransferPeer`) tem seu valor atualizado automaticamente para o mesmo novo valor, e o saldo da conta da perna par também é recalculado
- Transações `Income`/`Expense` exigem uma categoria válida e compatível com o tipo da transação; a categoria pode ser trocada por outra do mesmo usuário, desde que compatível
- Resposta: dados completos da transação já atualizada (`Id`, `AccountId`, `CategoryId`, `TransferPeerId`, `Type`, `Amount`, `Description`, `Date`, `IsIncomingTransfer`, `CreatedAt`, `UpdatedAt`)

## Erros
- Transação não encontrada
- Transação existe mas pertence a outro usuário (tratado como não encontrada — ver `plan.md`)
- `Amount` zero ou negativo
- `Description` vazia ou nula
- Nova categoria não encontrada
- Nova categoria existe mas pertence a outro usuário (tratado como não encontrada — ver `plan.md`)
- Nova categoria incompatível com o tipo da transação (ex.: categoria `Expense` em transação `Income`)
- Tentativa de definir uma categoria em uma transação do tipo `Transfer`
- Tentativa de remover a categoria de uma transação `Income` ou `Expense` (`CategoryId` nulo)

## Fora de escopo
- Alterar `Type` — imutável após a criação da transação
- Alterar `AccountId` — mover uma transação para outra conta não é suportado por esta feature
- Alterar `Date` — não faz parte dos campos editáveis desta feature
- Criar ou excluir a transação par de uma transferência — apenas o `Amount` do par é sincronizado; criação e exclusão são cobertas por outras features
- Qualquer distinção de resposta entre "transação não existe" e "transação pertence a outro usuário", ou entre "categoria não existe" e "categoria pertence a outro usuário" — do ponto de vista do chamador, cada par de casos é idêntico
