export interface MonthRef {
	month: number;
	year: number;
}

export function monthRefToDate(ref: MonthRef): Date {
	return new Date(ref.year, ref.month, 1);
}

/** The first day of the month as the API's `dateRef` query param (`YYYY-MM-01`). */
export function monthRefToDateRef(ref: MonthRef): string {
	return `${ref.year}-${String(ref.month + 1).padStart(2, '0')}-01`;
}

export function isSameMonthRef(a: MonthRef | null, b: MonthRef | null): boolean {
	if (!a || !b) return a === b;
	return a.month === b.month && a.year === b.year;
}
