import {
	CategoryExpense,
	CumulativeExpenseComparison,
	ExpensesByCategory,
	IncomeVsExpense,
	MonthlySummary,
	MonthlyTransaction,
} from '../types/report';

export function buildMonthlySummary(overrides: Partial<MonthlySummary> = {}): MonthlySummary {
	return {
		month: '2026-09',
		totalIncome: 8000,
		totalExpense: 5200,
		balance: 2800,
		savingsRate: 35,
		projectedExpense: 5200,
		projectedBalance: 2800,
		previousMonth: {
			totalIncome: 8000,
			totalExpense: 4000,
			balance: 4000,
			totalIncomeChange: 0,
			totalExpenseChange: 30,
			balanceChange: -30,
		},
		...overrides,
	};
}

export function buildCategoryExpense(overrides: Partial<CategoryExpense> = {}): CategoryExpense {
	return {
		categoryId: '3f2a1c4e-0000-4000-8000-000000000001',
		categoryName: 'Mercado',
		color: '#43A047',
		amount: 1200,
		percentage: 23.08,
		...overrides,
	};
}

export function buildExpensesByCategory(overrides: Partial<ExpensesByCategory> = {}): ExpensesByCategory {
	return {
		total: 5200,
		items: [
			buildCategoryExpense({
				categoryId: 'mercado',
				categoryName: 'Mercado',
				color: '#43A047',
				amount: 2950,
				percentage: 56.73,
			}),
			buildCategoryExpense({
				categoryId: 'aluguel',
				categoryName: 'Aluguel',
				color: '#8E24AA',
				amount: 2000,
				percentage: 38.46,
			}),
			buildCategoryExpense({ categoryId: null, categoryName: 'Outras', color: null, amount: 250, percentage: 4.81 }),
		],
		...overrides,
	};
}

export function buildIncomeVsExpense(overrides: Partial<IncomeVsExpense> = {}): IncomeVsExpense {
	return { month: '2026-09', income: 8000, expense: 5200, balance: 2800, ...overrides };
}

export function buildCumulativeExpenseComparison(
	overrides: Partial<CumulativeExpenseComparison> = {},
): CumulativeExpenseComparison {
	return {
		currentMonth: [
			{ day: 1, accumulated: 150 },
			{ day: 2, accumulated: 210 },
			{ day: 3, accumulated: 210 },
		],
		previousMonth: [
			{ day: 1, accumulated: 100 },
			{ day: 2, accumulated: 100 },
			{ day: 3, accumulated: 800 },
			{ day: 4, accumulated: 800 },
		],
		daysInCurrentMonth: 31,
		daysInPreviousMonth: 30,
		...overrides,
	};
}

export function buildMonthlyTransaction(overrides: Partial<MonthlyTransaction> = {}): MonthlyTransaction {
	return {
		id: '7b1d2e3f-0000-4000-8000-000000000001',
		date: '2026-09-20T00:00:00Z',
		description: 'Feira',
		categoryName: 'Mercado',
		type: 'out',
		amount: 500,
		...overrides,
	};
}
