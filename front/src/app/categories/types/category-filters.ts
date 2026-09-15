export interface CategoryFilters {
	name: string;
	withTransaction: boolean | null;
}

export const DEFAULT_CATEGORY_FILTERS: CategoryFilters = {
	name: '',
	withTransaction: null,
};

export const CATEGORY_TRANSACTION_FILTER = {
	All: null,
	With: true,
	Without: false,
} as const;

export type CategoryTransactionFilter = (typeof CATEGORY_TRANSACTION_FILTER)[keyof typeof CATEGORY_TRANSACTION_FILTER];

export const CATEGORY_TRANSACTION_FILTER_OPTIONS = [
	{ label: 'Todas', value: CATEGORY_TRANSACTION_FILTER.All },
	{ label: 'Com transações', value: CATEGORY_TRANSACTION_FILTER.With },
	{ label: 'Sem transações', value: CATEGORY_TRANSACTION_FILTER.Without },
];
