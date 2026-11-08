# Modelo de dados: Cor da categoria nas transações do mês

Nenhuma entidade, coluna ou migration nova: `Category.Color` já existe (`varchar(7)`, obrigatório).

**MonthlyTransactionOutput** (resposta, `Application/IO/Reports`)

| Campo | Tipo | Mudança |
| --- | --- | --- |
| `id`, `date`, `description`, `categoryName`, `type`, `amount` | como em 003 | nenhuma |
| `color` | string, `^#[0-9A-F]{6}$`, não nulo | **novo**: `Category.Color.HexCode` da transação, lido a cada consulta |

**Repositório**: `ITransactionRepository.GetWithCategoryAsync(from, to)` → `(Transaction, Category)` (antes `(Transaction, string CategoryName)`); filtro por data e `AsNoTracking()` como hoje.
