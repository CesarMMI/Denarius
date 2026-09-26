/**
 * Transaction dates are calendar days. The API stores them as UTC timestamps and filters months in UTC,
 * so a day is sent as its UTC midnight and read back from the date part, independent of the local time zone.
 */
export function toApiDate(date: Date): string {
	return `${toDateKey(date)}T00:00:00.000Z`;
}

export function fromApiDate(value: string): Date {
	const [year, month, day] = value.slice(0, 10).split('-').map(Number);
	return new Date(year, month - 1, day);
}

/** The `YYYY-MM-DD` key of a local date. */
export function toDateKey(date: Date): string {
	return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

function pad(value: number) {
	return String(value).padStart(2, '0');
}
