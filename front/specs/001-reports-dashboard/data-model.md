# Modelo de dados: Painel de relatórios

O front não armazena nada. O seu modelo são os formatos dos cinco relatórios que ele recebe, tipados em
`src/app/reports/types/report.ts` para espelhar `back/specs/003-financial-reports/contracts/reports-api.yaml`
(dinheiro é `number` aqui — a API envia decimais como números JSON; os meses são strings `YYYY-MM`; as datas são
strings ISO à meia-noite UTC).

## Tipos

```ts
interface MonthlySummary {
	month: string; // 'YYYY-MM'
	totalIncome: number;
	totalExpense: number; // positive
	balance: number;
	savingsRate: number | null; // 0–100 scale; null without income
	projectedExpense: number;
	projectedBalance: number;
	previousMonth: PreviousMonthSummary;
}

interface PreviousMonthSummary {
	totalIncome: number;
	totalExpense: number;
	balance: number;
	totalIncomeChange: number | null; // 0–100 scale; null when the previous value is 0
	totalExpenseChange: number | null;
	balanceChange: number | null;
}

interface ExpensesByCategory {
	total: number;
	items: CategoryExpense[]; // largest first, "Outras" last
}

interface CategoryExpense {
	categoryId: string | null; // null for "Outras"
	categoryName: string;
	color: string | null; // '#RRGGBB'; null for "Outras"
	amount: number;
	percentage: number; // 0–100 scale
}

interface IncomeVsExpense {
	// the endpoint returns IncomeVsExpense[]
	month: string;
	income: number;
	expense: number;
	balance: number;
}

interface CumulativeExpenseComparison {
	currentMonth: AccumulatedExpense[];
	previousMonth: AccumulatedExpense[];
	daysInCurrentMonth: number;
	daysInPreviousMonth: number;
}

interface AccumulatedExpense {
	day: number;
	accumulated: number;
}

interface MonthlyTransaction {
	// the endpoint returns MonthlyTransaction[]
	id: string;
	date: string;
	description: string | null;
	categoryName: string;
	type: 'in' | 'out';
	amount: number; // positive; type says the sign
}
```

## Estado da página (`ReportsPage`)

| Estado                                                                                   | Tipo                           | Regra                                                                                                                                                                     |
| ---------------------------------------------------------------------------------------- | ------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `month`                                                                                  | `WritableSignal<Date \| null>` | Dia 1 do mês selecionado, no horário local; começa no mês atual em São Paulo. Tem binding bidirecional com o campo de mês; "Mês anterior" / "Próximo mês" o movem um mês. |
| `summary`, `expensesByCategory`, `incomeVsExpense`, `cumulativeExpenses`, `transactions` | `HttpResourceRef<…>`           | Um por bloco, cada um do `ReportsService` com `month()`; um mês novo recarrega os cinco, e o `retry` de um bloco recarrega só o dele.                                     |

## Estado derivado da visão (por bloco)

| Bloco                  | Vazio quando                                      | Mostra                                                                                                                                                                                       |
| ---------------------- | ------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Cards de resumo        | receita e despesa são ambas 0                     | 5 cards, despesas como valores negativos sem cor; variações como ↑/↓ + percentual + "vs. {mês anterior}", cor boa/ruim por métrica                                                           |
| Despesas por categoria | `items` está vazio                                | rosca, com no máximo cinco fatias (com mais de cinco itens, as quatro maiores e "Outras" com o resto, a sua participação recalculada a partir do `total`), cor da categoria, "Outras" neutra |
| Receitas vs. despesas  | todos os meses têm receita e despesa 0            | colunas agrupadas por mês, receita verde, despesa vermelha                                                                                                                                   |
| Comparação acumulada   | todos os valores acumulados das duas séries são 0 | duas linhas sobre os dias 1…max(dias), atual em vermelho, anterior neutra                                                                                                                    |
| Lista de transações    | a lista está vazia                                | título com a contagem do mês; tabela das dez primeiras (mais recentes): dd/MM, descrição + categoria, valor com sinal (vermelho quando `out`); "Ver todas" → `/transactions?month=YYYY-MM`   |
