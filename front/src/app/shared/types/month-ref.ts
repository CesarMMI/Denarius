export interface MonthRef {
	month: number;
	year: number;
}

export function monthRefToDate(ref: MonthRef): Date {
	return new Date(ref.year, ref.month, 1);
}

export function isSameMonthRef(a: MonthRef | null, b: MonthRef | null): boolean {
	if (!a || !b) return a === b;
	return a.month === b.month && a.year === b.year;
}
