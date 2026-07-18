# transaction-delete

Permite que um usuário autenticado exclua uma transação que possui, revertendo automaticamente seu efeito no saldo da conta e, quando aplicável, excluindo também a transação par de uma transferência.

> Entidade de referência: `specs/entities/transaction.md`

## Entrada
- `TransactionId`

> `UserId` vem do token JWT do usuário autenticado, não é informado explicitamente pelo chamador.

## Happy path
- Transação com o `TransactionId` informado existe e pertence ao usuário autenticado
- Transação é excluída
- Saldo da conta vinculada é revertido com base no valor e tipo da transação excluída (`Income` desfaz a soma, `Expense` desfaz a subtração)
- Se a transação for do tipo `Transfer`, a transação par (`TransferPeer`) também é excluída automaticamente, e o saldo da conta correspondente à perna par também é revertido
- Resposta: sem conteúdo (204)

## Erros
- Transação não encontrada
- Transação existe mas pertence a outro usuário (tratado como não encontrada — ver `plan.md`)

## Fora de escopo
- Excluir apenas uma perna de uma transferência — sempre as duas transações do par são excluídas juntas
- Confirmação prévia ou soft-delete — a exclusão é imediata e definitiva
- Reverter/desfazer a exclusão (não existe operação de "restaurar" uma transação excluída)
- Qualquer distinção de resposta entre "transação não existe" e "transação pertence a outro usuário" — do ponto de vista do chamador, ambos os casos são idênticos
