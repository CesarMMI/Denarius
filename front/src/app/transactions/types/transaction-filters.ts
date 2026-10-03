/** A filter left out or empty is not applied. */
export interface TransactionFilters {
	description?: string;
	type?: 'in' | 'out' | '';
	categoryId?: string;
	/** The first day of the month. */
	month?: Date | null;
}
