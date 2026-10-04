// The reports API (back/specs/003-financial-reports/contracts/reports-api.yaml): months are `YYYY-MM`, money out
// is a positive amount, and percentages are on a 0–100 scale, null where they don't apply.

export interface MonthlySummary {
	month: string;
	totalIncome: number;
	totalExpense: number;
	balance: number;
	/** The balance over the income; null without income. */
	savingsRate: number | null;
	projectedExpense: number;
	projectedBalance: number;
	previousMonth: PreviousMonthSummary;
}

/** The month before, and the change from each of its values; null when the previous value is zero. */
export interface PreviousMonthSummary {
	totalIncome: number;
	totalExpense: number;
	balance: number;
	totalIncomeChange: number | null;
	totalExpenseChange: number | null;
	balanceChange: number | null;
}

export interface ExpensesByCategory {
	total: number;
	/** Largest first; past eight categories, the seven largest and then "Outras". */
	items: CategoryExpense[];
}

/** "Outras" has no id or color. */
export interface CategoryExpense {
	categoryId: string | null;
	categoryName: string;
	color: string | null;
	amount: number;
	percentage: number;
}

export interface IncomeVsExpense {
	month: string;
	income: number;
	expense: number;
	balance: number;
}

/** A series stops at today in the current month and has no days in a month that hasn't started. */
export interface CumulativeExpenseComparison {
	currentMonth: AccumulatedExpense[];
	previousMonth: AccumulatedExpense[];
	daysInCurrentMonth: number;
	daysInPreviousMonth: number;
}

export interface AccumulatedExpense {
	day: number;
	accumulated: number;
}

export interface MonthlyTransaction {
	id: string;
	date: string;
	description: string | null;
	categoryName: string;
	type: 'in' | 'out';
	amount: number;
}
