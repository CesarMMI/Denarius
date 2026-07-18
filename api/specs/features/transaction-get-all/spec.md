# transaction-get-all

Permite que um usuário autenticado liste suas transações, com filtros opcionais por conta, categoria, tipo e período.

> Entidade de referência: `specs/entities/transaction.md`

## Entrada
- `UserId` — vem do token JWT do usuário autenticado, não é informado pelo cliente
- `AccountId` *(opcional, query string)* — filtra transações de uma conta específica
- `CategoryId` *(opcional, query string)* — filtra transações de uma categoria específica
- `Type` *(opcional, query string)* — filtra por `Income`, `Expense` ou `Transfer`
- `StartDate` *(opcional, query string)* — data inicial do período
- `EndDate` *(opcional, query string)* — data final do período

## Happy path
- Retorna as transações do usuário aplicando todos os filtros informados
- Se nenhum filtro for informado, retorna todas as transações do usuário
- Usuário sem nenhuma transação (ou sem nenhuma correspondente aos filtros) recebe uma lista vazia

## Erros
- `StartDate` posterior a `EndDate`

## Fora de escopo
- Paginação ou ordenação customizável
- Filtros adicionais além de conta, categoria, tipo e período (ex: por descrição, por valor)
- Retornar totais ou agregações sobre as transações listadas
