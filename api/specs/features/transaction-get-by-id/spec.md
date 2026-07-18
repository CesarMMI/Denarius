# transaction-get-by-id

Permite que um usuário autenticado consulte os dados de uma transação específica que possui, a partir do identificador da transação.

> Entidade de referência: `specs/entities/transaction.md`

## Entrada
- `TransactionId`

> `UserId` vem do token JWT do usuário autenticado, não é informado explicitamente pelo chamador.

## Happy path
- Transação com o `TransactionId` informado existe e pertence ao usuário autenticado
- Resposta: dados da transação (`Id`, `AccountId`, `CategoryId`, `TransferPeerId`, `Type`, `Amount`, `Description`, `Date`, `IsIncomingTransfer`, `CreatedAt`, `UpdatedAt`)
- Transações de qualquer tipo (`Income`, `Expense`, `Transfer`) podem ser consultadas normalmente, incluindo ambas as pernas de uma transferência

## Erros
- Transação não encontrada
- Transação existe mas pertence a outro usuário (tratado como não encontrada — ver `plan.md`)

## Fora de escopo
- Retornar dados da conta, categoria ou transação par (`TransferPeer`) associadas — a resposta contém apenas os campos da própria transação
- Qualquer distinção de resposta entre "transação não existe" e "transação pertence a outro usuário" — do ponto de vista do chamador, ambos os casos são idênticos
