# account-update

Permite que um usuário autenticado atualize o nome e a cor de identificação de uma conta financeira que possui.

> Entidade de referência: `specs/entities/account.md`

## Entrada
- `AccountId`
- `Name`
- `Color`

> `UserId` vem do token JWT do usuário autenticado, não é informado explicitamente pelo chamador.

## Campos editáveis
- `Name`
- `Color`

`CurrencyCode` não pode ser alterado após a criação da conta — não faz parte da entrada desta feature.

## Happy path
- Conta com o `AccountId` informado existe e pertence ao usuário autenticado
- `Name` e `Color` são válidos (não vazios/nulos)
- Conta atualizada com os novos valores de `Name` e `Color`
- Resposta: dados completos da conta já atualizada (`Id`, `Name`, `CurrencyCode`, `Balance`, `Color`, `IsActive`, `CreatedAt`, `UpdatedAt`)
- Contas inativas também podem ser atualizadas normalmente

## Erros
- Conta não encontrada
- Conta existe mas pertence a outro usuário (tratado como não encontrada — ver `plan.md`)
- `Name` vazio ou nulo
- `Color` vazia ou nula

## Fora de escopo
- Alterar `CurrencyCode` — imutável após a criação da conta
- Alterar `Balance` — só muda via transações confirmadas
- Alterar `IsActive` — feito exclusivamente pela feature de desativação de conta
- Qualquer distinção de resposta entre "conta não existe" e "conta pertence a outro usuário" — do ponto de vista do chamador, ambos os casos são idênticos
