/** A filter left out or empty is not applied. */
export interface CategoryFilters {
	name?: string;
	/** Keeps the categories with (`true`) or without (`false`) transactions in the period. */
	withTransaction?: boolean | '';
	/** The first day of the month. */
	month?: Date | null;
}
