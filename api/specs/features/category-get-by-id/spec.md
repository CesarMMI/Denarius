# category-get-by-id

Permite que um usuário autenticado consulte os dados de uma categoria específica que possui, a partir do identificador da categoria.

> Entidade de referência: `specs/entities/category.md`

## Entrada
- `CategoryId`

> `UserId` vem do token JWT do usuário autenticado, não é informado explicitamente pelo chamador.

## Happy path
- Categoria com o `CategoryId` informado existe e pertence ao usuário autenticado
- Resposta: dados da categoria (`Id`, `Name`, `Color`, `Type`, `CreatedAt`, `UpdatedAt`)

## Erros
- Categoria não encontrada
- Categoria existe mas pertence a outro usuário (tratado como não encontrada — ver `plan.md`)

## Fora de escopo
- Retornar dados de transações ou de outras entidades relacionadas à categoria — a resposta contém apenas os campos da própria categoria
- Qualquer distinção de resposta entre "categoria não existe" e "categoria pertence a outro usuário" — do ponto de vista do chamador, ambos os casos são idênticos
