# category-update

Permite que um usuário autenticado atualize o nome e a cor de identificação de uma categoria que possui.

> Entidade de referência: `specs/entities/category.md`

## Entrada
- `CategoryId`
- `Name`
- `Color`

> `UserId` vem do token JWT do usuário autenticado, não é informado explicitamente pelo chamador.

## Campos editáveis
- `Name`
- `Color`

`Type` não pode ser alterado após a criação da categoria — não faz parte da entrada desta feature.

## Happy path
- Categoria com o `CategoryId` informado existe e pertence ao usuário autenticado
- `Name` e `Color` são válidos (não vazios/nulos)
- Categoria atualizada com os novos valores de `Name` e `Color`
- Resposta: dados completos da categoria já atualizada (`Id`, `Name`, `Color`, `Type`, `CreatedAt`, `UpdatedAt`)

## Erros
- Categoria não encontrada
- Categoria existe mas pertence a outro usuário (tratado como não encontrada — ver `plan.md`)
- `Name` vazio ou nulo
- `Color` vazia ou nula

## Fora de escopo
- Alterar `Type` — imutável após a criação da categoria (mudar o tipo invalidaria o histórico de transações já classificadas com essa categoria)
- Validação de que `Color` seja um valor hexadecimal ou de uma paleta específica — qualquer string não vazia é aceita
- Qualquer distinção de resposta entre "categoria não existe" e "categoria pertence a outro usuário" — do ponto de vista do chamador, ambos os casos são idênticos
