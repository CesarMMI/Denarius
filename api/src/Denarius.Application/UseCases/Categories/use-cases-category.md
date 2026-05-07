# Category — Use Cases

## CreateCategory

Cria uma nova categoria para o usuário.

### Entrada
- `UserId`
- `Name`
- `Color`
- `Type` (`Income` ou `Expense`)

### Happy path
- Categoria criada com os dados informados

### Erros
- `Name` vazio ou nulo
- `Color` vazia ou nula
- `Type` não informado

---

## GetCategoryById

Busca uma categoria pelo seu identificador.

### Entrada
- `UserId`
- `CategoryId`

### Happy path
- Retorna a categoria correspondente

### Erros
- Categoria não encontrada
- Categoria não pertence ao usuário

---

## ListCategories

Lista as categorias do usuário, com filtro opcional por tipo.

### Entrada
- `UserId`
- `Type` *(opcional)* — filtra por `Income` ou `Expense`

### Happy path
- Retorna todas as categorias do usuário
- Se `Type` for informado, retorna apenas as categorias do tipo correspondente

---

## UpdateCategory

Atualiza os dados editáveis de uma categoria.

### Campos editáveis
- `Name`
- `Color`

> `Type` não pode ser alterado após a criação.

### Entrada
- `UserId`
- `CategoryId`
- `Name`
- `Color`

### Happy path
- Categoria atualizada com os novos valores

### Erros
- Categoria não encontrada
- Categoria não pertence ao usuário
- `Name` vazio ou nulo
- `Color` vazia ou nula

---

## DeleteCategory

Exclui uma categoria. Transações vinculadas a ela terão `CategoryId` definido como nulo.

### Entrada
- `UserId`
- `CategoryId`

### Happy path
- Categoria excluída
- Transações vinculadas são atualizadas com `CategoryId = null`

### Erros
- Categoria não encontrada
- Categoria não pertence ao usuário
