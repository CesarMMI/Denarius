/** A filter left out or empty is not applied. */
export interface TransactionFilters {
	description?: string;
	type?: 'In' | 'Out' | '';
	categoryId?: string;
	/** The first day of the month. */
	month?: Date | null;
}
