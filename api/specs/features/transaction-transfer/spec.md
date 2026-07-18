# transaction-transfer

Permite que um usuário autenticado transfira valor entre duas contas próprias, gerando um par de transações vinculadas (saída na conta de origem, entrada na conta de destino) e ajustando o saldo de ambas.

> Entidade(s) de referência: `specs/entities/transaction.md`, `specs/entities/account.md`

## Entrada
- `SourceAccountId`
- `DestinationAccountId`
- `Amount`
- `Description`
- `Date`

> `UserId` vem do token JWT do usuário autenticado, não é informado explicitamente pelo chamador.

## Happy path
- Conta de origem e conta de destino existem e pertencem ao usuário autenticado
- Ambas as contas estão ativas
- Conta de origem e conta de destino são diferentes
- Conta de origem e conta de destino têm o mesmo `CurrencyCode`
- `Amount` é positivo
- `Description` não é vazia
- Duas transações são criadas — uma de saída (`AccountId = SourceAccountId`) e uma de entrada (`AccountId = DestinationAccountId`) — vinculadas entre si via `TransferPeerId`
- A transação de entrada é marcada como `IsIncomingTransfer = true`
- Saldo da conta de origem é subtraído em `Amount`
- Saldo da conta de destino é somado em `Amount`
- Resposta: dados das duas transações criadas (saída e entrada)

## Erros
- Conta de origem não encontrada
- Conta de origem pertence a outro usuário (tratado como não encontrada — ver `plan.md`)
- Conta de origem está inativa
- Conta de destino não encontrada
- Conta de destino pertence a outro usuário (tratado como não encontrada — ver `plan.md`)
- Conta de destino está inativa
- Conta de origem e conta de destino são a mesma conta
- Conta de origem e conta de destino têm `CurrencyCode` diferentes
- `Amount` zero ou negativo
- `Description` vazia ou nula

## Fora de escopo
- Transferências entre contas de usuários diferentes — origem e destino devem pertencer ao mesmo usuário autenticado
- Conversão de moeda em transferências entre `CurrencyCode` diferentes — não suportado
- Definir manualmente `TransferPeerId` ou `IsIncomingTransfer` pelo chamador — controlados inteiramente pelo servidor
- Categoria em transações de transferência — `Transfer` nunca tem `CategoryId` (ver `specs/entities/transaction.md`)
- Alterar contas/valores de uma transferência já criada — coberto por `UpdateTransaction`/`DeleteTransaction`, não por esta feature
