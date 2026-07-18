# account-get-by-id

Permite que um usuário autenticado consulte os dados de uma conta financeira específica que possui, a partir do identificador da conta.

> Entidade de referência: `specs/entities/account.md`

## Entrada
- `AccountId`

> `UserId` vem do token JWT do usuário autenticado, não é informado explicitamente pelo chamador.

## Happy path
- Conta com o `AccountId` informado existe e pertence ao usuário autenticado
- Resposta: dados da conta (`Id`, `Name`, `CurrencyCode`, `Balance`, `Color`, `IsActive`, `CreatedAt`, `UpdatedAt`)
- Contas inativas também podem ser consultadas normalmente

## Erros
- Conta não encontrada
- Conta existe mas pertence a outro usuário (tratado como não encontrada — ver `plan.md`)

## Fora de escopo
- Retornar dados de transações ou de outras entidades relacionadas à conta — a resposta contém apenas os campos da própria conta
- Qualquer distinção de resposta entre "conta não existe" e "conta pertence a outro usuário" — do ponto de vista do chamador, ambos os casos são idênticos
