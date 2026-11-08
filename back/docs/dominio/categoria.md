# Categoria

Rótulo definido pelo usuário para agrupar transações, com um nome e uma cor. Pode existir sem nenhuma transação. Código: `Denarius.Domain/Entities/Category.cs`.

## Campos

| Campo | Regra |
| --- | --- |
| `Id` | `Guid`, gerado na criação |
| `Name` | Obrigatório. Sem espaços nas pontas. No máximo 100 caracteres (`NameMaxLength`) |
| `Color` | Value object `Color`, ver abaixo |
| `CreatedAt`, `UpdatedAt` | UTC. `UpdatedAt` muda a cada `Update` |

## Cor (`Color`)

- Aceita `#RGB` ou `#RRGGBB`, com ou sem `#`, em maiúsculas ou minúsculas.
- Normaliza para `#RRGGBB` em maiúsculas (`abc` vira `#AABBCC`). É esse o formato que a API devolve.
- Vazia ou fora do formato: `DomainException`.

## Regras

- Nome vazio, só com espaços ou acima de 100 caracteres é recusado (`DomainException`).
- **Exclusão:** uma categoria com ao menos uma transação, em qualquer mês, não pode ser excluída (`DomainException`: "Não é possível excluir uma categoria que possui transações associadas."). Categoria inexistente: `NotFoundException`.
- **Derivados na listagem:** `TransactionCount` e `Balance` (soma dos valores) saem das transações da categoria no mês pedido, ou em todo o histórico sem mês. `CanDelete` reflete a regra de exclusão e por isso ignora o mês pedido (`ListCategoriesUseCase.cs:13`).
- A busca por nome na listagem ignora maiúsculas e acentos, e é feita no banco (`unaccent`).

## Relações

Uma categoria tem muitas [transações](transacao.md); cada transação tem exatamente uma categoria.

## Origem

Specs [001-category-management](../features/001-category-management/spec.md) e [004-cor-categoria-transacoes-mes](../features/004-cor-categoria-transacoes-mes/spec.md).
