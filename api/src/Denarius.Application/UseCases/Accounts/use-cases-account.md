# Account — Use Cases

## CreateAccount

Cria uma nova conta para o usuário.

### Entrada
- `UserId`
- `Name`
- `CurrencyCode`
- `Color`

### Happy path
- Conta criada com `Balance = 0` e `IsActive = true`

### Erros
- `Name` vazio ou nulo
- `CurrencyCode` inválido (fora do padrão ISO 4217)
- `Color` vazia ou nula

---

## GetAccountById

Busca uma conta pelo seu identificador.

### Entrada
- `UserId`
- `AccountId`

### Happy path
- Retorna a conta correspondente

### Erros
- Conta não encontrada
- Conta não pertence ao usuário

---

## ListAccounts

Lista todas as contas do usuário.

### Entrada
- `UserId`

### Happy path
- Retorna a lista de contas do usuário, incluindo contas inativas

---

## UpdateAccount

Atualiza os dados editáveis de uma conta.

### Campos editáveis
- `Name`
- `Color`

> `CurrencyCode` não pode ser alterado após a criação da conta.

### Entrada
- `UserId`
- `AccountId`
- `Name`
- `Color`

### Happy path
- Conta atualizada com os novos valores

### Erros
- Conta não encontrada
- Conta não pertence ao usuário
- `Name` vazio ou nulo
- `Color` vazia ou nula

---

## DeactivateAccount

Desativa uma conta. Contas com transações vinculadas não podem ser excluídas, apenas desativadas.

### Entrada
- `UserId`
- `AccountId`

### Happy path
- Conta marcada como inativa
- Se a conta já estiver inativa, não tem efeito

### Erros
- Conta não encontrada
- Conta não pertence ao usuário
