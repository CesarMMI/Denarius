# category-delete

Permite que um usuário autenticado exclua uma categoria que possui, sem deixar transações órfãs de forma inconsistente.

> Entidade de referência: `specs/entities/category.md`

## Entrada
- `CategoryId`

> `UserId` vem do token JWT do usuário autenticado, não é informado explicitamente pelo chamador.

## Happy path
- Categoria com o `CategoryId` informado existe e pertence ao usuário autenticado
- Transações vinculadas a essa categoria têm `CategoryId` definido como nulo
- Categoria é excluída
- Resposta: sem conteúdo (exclusão confirmada)

## Erros
- Categoria não encontrada
- Categoria existe mas pertence a outro usuário (tratado como não encontrada — ver `plan.md`)

## Fora de escopo
- Excluir em cascata as transações vinculadas à categoria — elas permanecem, apenas perdem a referência (`CategoryId = null`)
- Qualquer distinção de resposta entre "categoria não existe" e "categoria pertence a outro usuário" — do ponto de vista do chamador, ambos os casos são idênticos
- Confirmação prévia ou soft-delete — a exclusão é imediata e definitiva
