import { SortOption } from '../../shared/types/sort';

export const TRANSACTION_SORT_FIELD = {
	Date: 'Date',
	Description: 'Description',
	Value: 'Value',
	CategoryName: 'CategoryName',
} as const;

export type TransactionSortField = (typeof TRANSACTION_SORT_FIELD)[keyof typeof TRANSACTION_SORT_FIELD];

export const TRANSACTION_SORT_OPTIONS: SortOption<TransactionSortField>[] = [
	{ label: 'Mais recentes', orderBy: TRANSACTION_SORT_FIELD.Date, ascending: false },
	{ label: 'Mais antigas', orderBy: TRANSACTION_SORT_FIELD.Date, ascending: true },
	{ label: 'Maior valor', orderBy: TRANSACTION_SORT_FIELD.Value, ascending: false },
	{ label: 'Menor valor', orderBy: TRANSACTION_SORT_FIELD.Value, ascending: true },
	{ label: 'Descrição [A-Z]', orderBy: TRANSACTION_SORT_FIELD.Description, ascending: true },
	{ label: 'Descrição [Z-A]', orderBy: TRANSACTION_SORT_FIELD.Description, ascending: false },
	{ label: 'Categoria [A-Z]', orderBy: TRANSACTION_SORT_FIELD.CategoryName, ascending: true },
	{ label: 'Categoria [Z-A]', orderBy: TRANSACTION_SORT_FIELD.CategoryName, ascending: false },
];
