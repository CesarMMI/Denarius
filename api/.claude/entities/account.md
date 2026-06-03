# Account

Representa uma conta financeira do usuário, como conta corrente, poupança ou carteira física.
É o ponto central do sistema — toda transação está vinculada a uma conta.

## Propriedades

| Propriedade    | Descrição                                          |
|----------------|----------------------------------------------------|
| `Id`           | Identificador único da conta                       |
| `UserId`       | Identificador do usuário dono da conta             |
| `Name`         | Nome da conta (ex: "Nubank", "Carteira")           |
| `CurrencyCode` | Código da moeda no padrão ISO 4217 (ex: BRL, USD)  |
| `Balance`      | Saldo atual da conta                               |
| `Color`        | Cor de identificação visual da conta               |
| `IsActive`     | Indica se a conta está ativa                       |
| `CreatedAt`    | Data e hora de criação                             |
| `UpdatedAt`    | Data e hora da última atualização                  |

## Regras de negócio

### Criação

- `Name` não pode ser vazio
- `CurrencyCode` deve seguir o padrão ISO 4217 (3 letras maiúsculas: BRL, USD, EUR...)
- `Color` deve ser informada
- `Balance` começa sempre em zero — o saldo é construído a partir das transações

### Atualização de saldo

- O saldo é atualizado a cada transação confirmada, somando ou subtraindo o valor da operação
- Saldo negativo é permitido e representa uma conta no vermelho
- Uma atualização com delta zero não é uma operação válida

### Desativação

- Uma conta com transações vinculadas não pode ser excluída — apenas desativada
- Desativar uma conta já inativa não tem efeito

## Relacionamentos

- Pertence a um `User`
- Possui zero ou mais `Transaction`
