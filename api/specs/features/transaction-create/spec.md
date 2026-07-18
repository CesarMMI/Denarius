# transaction-create

Permite que um usuário autenticado registre uma transação de `Income` (receita) ou `Expense` (despesa) em uma de suas contas, atualizando o saldo correspondente automaticamente.

> Entidade de referência: `specs/entities/transaction.md`

## Entrada
- `AccountId`
- `CategoryId`
- `Type` (`Income` ou `Expense`)
- `Amount`
- `Description`
- `Date`

> `UserId` vem do token JWT do usuário autenticado, não é informado explicitamente pelo chamador.

## Happy path
- Conta com o `AccountId` informado existe, pertence ao usuário autenticado e está ativa
- Categoria com o `CategoryId` informado existe e pertence ao usuário autenticado
- Categoria é compatível com o `Type` da transação (`Income` só aceita categoria `Income`, `Expense` só aceita categoria `Expense`)
- `Amount` é positivo
- `Description` não é vazia
- Transação criada
- Saldo da conta é atualizado: somado em `Income`, subtraído em `Expense`
- Resposta: dados completos da transação criada (`Id`, `AccountId`, `CategoryId`, `TransferPeerId`, `Type`, `Amount`, `Description`, `Date`, `IsIncomingTransfer`, `CreatedAt`, `UpdatedAt`)

## Erros
- Conta não encontrada
- Conta existe mas pertence a outro usuário (tratado como não encontrada — ver `plan.md`)
- Conta está inativa
- Categoria não encontrada
- Categoria existe mas pertence a outro usuário (tratado como não encontrada — ver `plan.md`)
- Categoria incompatível com o tipo da transação (ex: categoria `Expense` usada em transação `Income`)
- `Amount` zero ou negativo
- `Description` vazia ou nula

## Fora de escopo
- Transações do tipo `Transfer` — coberto por uma feature própria (`CreateTransfer`), que não usa este fluxo
- Alterar `Type`, `Amount`, `Description` ou `Category` após a criação — coberto por `UpdateTransaction`
- Exclusão da transação e reversão do saldo — coberto por `DeleteTransaction`
- Definir `Date` fora de um intervalo permitido — datas passadas ou futuras são igualmente aceitas
- Qualquer distinção de resposta entre "conta/categoria não existe" e "conta/categoria pertence a outro usuário" — do ponto de vista do chamador, ambos os casos são idênticos
