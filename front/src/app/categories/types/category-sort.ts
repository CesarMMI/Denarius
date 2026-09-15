import { SortOption } from '../../shared/types/sort';

export const CATEGORY_SORT_FIELD = {
	Name: 'Name',
	TransactionCount: 'TransactionCount',
	Balance: 'Balance',
} as const;

export type CategorySortField = (typeof CATEGORY_SORT_FIELD)[keyof typeof CATEGORY_SORT_FIELD];

export const CATEGORY_SORT_OPTIONS: SortOption<CategorySortField>[] = [
	{ label: 'Nome [A-Z]', orderBy: CATEGORY_SORT_FIELD.Name, ascending: true },
	{ label: 'Nome [Z-A]', orderBy: CATEGORY_SORT_FIELD.Name, ascending: false },
	{ label: 'Maior Saldo', orderBy: CATEGORY_SORT_FIELD.Balance, ascending: false },
	{ label: 'Menor Saldo', orderBy: CATEGORY_SORT_FIELD.Balance, ascending: true },
	{ label: 'Mais Transações', orderBy: CATEGORY_SORT_FIELD.TransactionCount, ascending: false },
	{ label: 'Menos Transações', orderBy: CATEGORY_SORT_FIELD.TransactionCount, ascending: true },
];
