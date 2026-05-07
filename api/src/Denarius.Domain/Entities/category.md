# Category

Representa uma categoria usada para classificar transações financeiras.
Permite ao usuário organizar suas receitas e despesas por natureza (ex: Alimentação, Salário, Transporte).

## Propriedades

| Propriedade | Descrição                                        |
|-------------|--------------------------------------------------|
| `Id`        | Identificador único da categoria                 |
| `UserId`    | Identificador do usuário dono da categoria       |
| `Name`      | Nome da categoria (ex: "Alimentação", "Salário") |
| `Color`     | Cor de identificação visual da categoria         |
| `Type`      | Tipo da categoria: `Income` ou `Expense`         |
| `CreatedAt` | Data e hora de criação                           |
| `UpdatedAt` | Data e hora da última atualização                |

## Tipos (`CategoryType`)

| Valor     | Descrição                                              |
|-----------|--------------------------------------------------------|
| `Income`  | Categoria exclusiva para transações de entrada         |
| `Expense` | Categoria exclusiva para transações de saída           |

## Regras de negócio

### Criação

- `Name` não pode ser vazio
- `Color` deve ser informada
- `Type` deve ser informado na criação

### Alteração

- `Name` e `Color` podem ser alterados via `Update(name, color)`
- `Name` não pode ser vazio
- `Color` deve ser informada
- `Type` nunca pode ser alterado após a criação — mudar o tipo invalidaria o histórico de transações já classificadas com essa categoria

### Compatibilidade com transações

- Uma categoria do tipo `Income` só pode classificar transações do tipo `Income`
- Uma categoria do tipo `Expense` só pode classificar transações do tipo `Expense`
- Transações do tipo `Transfer` não possuem categoria

## Relacionamentos

- Pertence a um `User`
- Classifica zero ou mais `Transaction`
