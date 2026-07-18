# category-create

Permite que um usuário autenticado crie uma nova categoria (`Income` ou `Expense`) para classificar suas transações financeiras.

> Entidade de referência: `specs/entities/category.md`

## Entrada
- `Name`
- `Color`
- `Type` (`Income` ou `Expense`)

> `UserId` vem do token JWT do usuário autenticado, não é informado explicitamente pelo chamador.

## Happy path
- `Name` não é vazio
- `Color` não é vazia
- `Type` é `Income` ou `Expense`
- Categoria é criada com os dados informados
- Resposta: dados da categoria criada (`Id`, `Name`, `Color`, `Type`, `CreatedAt`, `UpdatedAt`)

## Erros
- `Name` vazio ou nulo
- `Color` vazia ou nula

## Fora de escopo
- Impedir a criação de categorias com `Name` duplicado para o mesmo usuário — não há verificação de unicidade
- Validação de que `Color` seja um valor hexadecimal ou de uma paleta específica — qualquer string não vazia é aceita
- Alterar `Type` após a criação (ver `UpdateCategory`, que só permite alterar `Name` e `Color`)
- Limite de quantidade de categorias por usuário
