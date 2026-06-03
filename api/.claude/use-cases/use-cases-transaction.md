# Transaction — Use Cases

## CreateTransaction

Cria uma transação do tipo `Income` ou `Expense` em uma conta.

### Entrada
- `UserId`
- `AccountId`
- `CategoryId`
- `Type` (`Income` ou `Expense`)
- `Amount`
- `Description`
- `Date`

### Happy path
- Transação criada
- Saldo da conta atualizado: somado em `Income`, subtraído em `Expense`

### Erros
- Conta não encontrada
- Conta não pertence ao usuário
- Conta está inativa
- Categoria não encontrada
- Categoria não pertence ao usuário
- Categoria incompatível com o tipo da transação
- `Amount` zero ou negativo
- `Description` vazia ou nula

---

## CreateTransfer

Cria uma transferência entre duas contas do mesmo usuário. Gera um par de transações: saída na conta de origem e entrada na conta de destino.

### Entrada
- `UserId`
- `SourceAccountId`
- `DestinationAccountId`
- `Amount`
- `Description`
- `Date`

### Happy path
- Duas transações criadas e vinculadas entre si via `TransferPeerId`
- Saldo da conta de origem subtraído
- Saldo da conta de destino somado

### Erros
- Conta de origem não encontrada
- Conta de origem não pertence ao usuário
- Conta de origem está inativa
- Conta de destino não encontrada
- Conta de destino não pertence ao usuário
- Conta de destino está inativa
- Conta de origem e conta de destino são a mesma conta
- Conta de origem e conta de destino possuem `CurrencyCode` diferentes
- `Amount` zero ou negativo
- `Description` vazia ou nula

---

## GetTransactionById

Busca uma transação pelo seu identificador.

### Entrada
- `UserId`
- `TransactionId`

### Happy path
- Retorna a transação correspondente

### Erros
- Transação não encontrada
- Transação não pertence ao usuário

---

## ListTransactions

Lista transações do usuário com filtros opcionais.

### Entrada
- `UserId`
- `AccountId` *(opcional)*
- `CategoryId` *(opcional)*
- `Type` *(opcional)* — `Income`, `Expense` ou `Transfer`
- `StartDate` *(opcional)*
- `EndDate` *(opcional)*

### Happy path
- Retorna as transações do usuário aplicando os filtros informados
- Se nenhum filtro for informado, retorna todas as transações do usuário

### Erros
- `StartDate` posterior a `EndDate`

---

## UpdateTransaction

Atualiza os dados editáveis de uma transação.

### Campos editáveis
- `Amount`
- `Description`
- `CategoryId`

> `Type` não pode ser alterado após a criação.

### Entrada
- `UserId`
- `TransactionId`
- `Amount`
- `Description`
- `CategoryId`

### Happy path
- Transação atualizada com os novos valores
- Se a transação for do tipo `Transfer` e `Amount` for alterado, o valor da transação par (`TransferPeer`) é atualizado automaticamente para o mesmo valor
- Se `Amount` for alterado, o saldo da conta é recalculado com base na diferença entre o valor anterior e o novo valor

### Erros
- Transação não encontrada
- Transação não pertence ao usuário
- `Amount` zero ou negativo
- `Description` vazia ou nula
- Nova categoria não encontrada
- Nova categoria não pertence ao usuário
- Nova categoria incompatível com o tipo da transação
- Tentativa de definir categoria em uma transação `Transfer`
- Tentativa de remover categoria de uma transação `Income` ou `Expense`

---

## DeleteTransaction

Exclui uma transação.

### Entrada
- `UserId`
- `TransactionId`

### Happy path
- Transação excluída
- Saldo da conta revertido com base no valor e tipo da transação excluída
- Se a transação for do tipo `Transfer`, a transação par (`TransferPeer`) é excluída automaticamente e o saldo da conta correspondente também é revertido

### Erros
- Transação não encontrada
- Transação não pertence ao usuário
