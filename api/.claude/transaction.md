# Transaction

Representa uma movimentação financeira em uma conta.
Pode ser uma entrada de valor (receita), uma saída (despesa), ou uma transferência entre contas do mesmo usuário.

## Propriedades

| Propriedade      | Descrição                                                                 |
|------------------|---------------------------------------------------------------------------|
| `Id`             | Identificador único da transação                                          |
| `UserId`         | Identificador do usuário dono da transação                                |
| `AccountId`      | Conta à qual a transação pertence                                         |
| `CategoryId`     | Categoria da transação — obrigatória para `Income` e `Expense`, ausente em `Transfer` |
| `TransferPeerId` | Identificador da transação espelho — presente apenas em `Transfer`        |
| `Type`           | Tipo da transação: `Income`, `Expense` ou `Transfer`                      |
| `Amount`         | Valor da transação — sempre positivo                                      |
| `Description`    | Descrição ou observação sobre a transação                                 |
| `Date`           | Data da transação — pode ser passada ou futura                            |
| `CreatedAt`      | Data e hora de criação do registro                                        |
| `UpdatedAt`      | Data e hora da última atualização                                         |

## Tipos (`TransactionType`)

| Valor      | Descrição                                                   |
|------------|-------------------------------------------------------------|
| `Income`   | Entrada de valor na conta (ex: salário, freelance)          |
| `Expense`  | Saída de valor da conta (ex: compra, conta)                 |
| `Transfer` | Movimentação entre duas contas do mesmo usuário             |

## Regras de negócio

### Criação

- `Amount` deve ser positivo
- `Description` não pode ser vazia
- Data futura é permitida — o usuário pode lançar transações agendadas
- Transações do tipo `Income` e `Expense` devem ter uma `Category` compatível com seu tipo
- Transações do tipo `Transfer` não têm categoria
- Uma transferência deve sempre ser criada em par — a transação de saída na conta de origem e a transação de entrada na conta de destino existem juntas ou não existem
- A conta de origem e a conta de destino de uma transferência devem ser diferentes
- A conta de origem e a conta de destino de uma transferência devem ter o mesmo `CurrencyCode` — transferências entre moedas diferentes não são suportadas

### Alteração

- `Type` nunca pode ser alterado após a criação
- `Amount` e `Description` podem ser alterados
- A `Category` pode ser trocada, desde que a nova categoria seja compatível com o tipo da transação
- Ao editar o `Amount` de uma transferência, o par (`TransferPeer`) deve ter seu valor atualizado automaticamente para o mesmo valor

### Exclusão

- Ao excluir uma transação do tipo `Transfer`, a transação par (`TransferPeer`) deve ser excluída automaticamente
- A exclusão de qualquer transação deve reverter seu efeito no saldo da conta correspondente

### Efeito no saldo

- `Income` soma o valor ao saldo da conta
- `Expense` subtrai o valor do saldo da conta
- `Transfer` subtrai o valor da conta de origem e soma na conta de destino

## Relacionamentos

- Pertence a um `User`
- Pertence a uma `Account`
- Pertence a uma `Category` (exceto quando `Transfer`)
- Pode referenciar outra `Transaction` como par de transferência (`TransferPeer`)
