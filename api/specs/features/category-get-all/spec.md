# category-get-all

Permite que um usuário autenticado liste suas categorias, com filtro opcional por tipo (`Income` ou `Expense`).

> Entidade de referência: `specs/entities/category.md`

## Entrada
- `UserId` — vem do token JWT do usuário autenticado, não é informado pelo cliente
- `Type` *(opcional, query string)* — filtra por `Income` ou `Expense`

## Happy path
- Sem `Type`: retorna todas as categorias do usuário
- Com `Type`: retorna apenas as categorias do usuário cujo tipo corresponde ao filtro
- Usuário sem nenhuma categoria cadastrada recebe uma lista vazia

## Erros
- Nenhum erro específico deste use case — apenas o caso genérico de ausência/invalidez do token JWT (401), tratado antes do use case ser executado.

## Fora de escopo
- Paginação, ordenação customizável ou filtros além de `Type` (ex: por nome, por cor)
- Retornar contagem de transações vinculadas a cada categoria
