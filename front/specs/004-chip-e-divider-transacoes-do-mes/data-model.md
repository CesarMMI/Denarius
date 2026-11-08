# Modelo de dados: Chip colorido e divisores por dia em "Transações do mês"

Sem entidade nova nem armazenamento. Muda o tipo de resposta consumido:

**MonthlyTransaction** (`reports/types/report.ts`), espelho de `MonthlyTransactionOutput` do contrato do back (back 004):

| Campo | Tipo | Mudança |
| --- | --- | --- |
| `id`, `date`, `description`, `categoryName`, `type`, `amount` | como hoje | nenhuma |
| `color` | `string`, `#RRGGBB` em maiúsculas, não nulo | **novo** |

**Estado derivado** (`TransactionsList`): `sameDate`: `Set` das linhas exibidas cuja seguinte tem o mesmo `date.slice(0, 10)`; usado só para a classe `same-date` (sem divisor).
