# account-create

Permite que um usuário autenticado crie uma nova conta financeira (ex: conta corrente, poupança, carteira) para começar a registrar transações nela.

> Entidade de referência: `specs/entities/account.md`

## Entrada
- `Name`
- `CurrencyCode`
- `Color`

> `UserId` vem do token JWT do usuário autenticado, não é informado explicitamente pelo chamador.

## Happy path
- `Name` não é vazio
- `CurrencyCode` segue o padrão ISO 4217 (3 letras maiúsculas: BRL, USD, EUR...)
- `Color` não é vazia
- Conta é criada com `Balance = 0` e `IsActive = true`
- Resposta: dados da conta criada (`Id`, `Name`, `CurrencyCode`, `Balance`, `Color`, `IsActive`, `CreatedAt`, `UpdatedAt`)

## Erros
- `Name` vazio ou nulo
- `CurrencyCode` inválido (fora do padrão ISO 4217)
- `Color` vazia ou nula

## Fora de escopo
- Alterar `CurrencyCode` após a criação — não pode ser alterado por nenhum fluxo (ver `UpdateAccount`)
- Definir um `Balance` inicial diferente de zero na criação — o saldo só é construído a partir de transações
- Limite de quantidade de contas por usuário
- Validação de que `Color` seja um valor hexadecimal ou de uma paleta específica — qualquer string não vazia é aceita
