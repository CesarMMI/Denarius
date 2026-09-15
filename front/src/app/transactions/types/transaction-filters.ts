export const TRANSACTION_TYPE = {
	All: 'all',
	In: 'in',
	Out: 'out',
} as const;

export type TransactionType = (typeof TRANSACTION_TYPE)[keyof typeof TRANSACTION_TYPE];

export const TRANSACTION_TYPE_OPTIONS = [
	{ label: 'Todas', value: TRANSACTION_TYPE.All },
	{ label: 'Entradas', value: TRANSACTION_TYPE.In },
	{ label: 'Saídas', value: TRANSACTION_TYPE.Out },
];

export interface TransactionFilters {
	description: string;
	type: TransactionType;
	categoryId: string | 'all';
}

export const DEFAULT_TRANSACTION_FILTERS: TransactionFilters = {
	description: '',
	type: TRANSACTION_TYPE.All,
	categoryId: 'all',
};
