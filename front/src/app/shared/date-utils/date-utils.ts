/**
 * Transaction dates are calendar days. The API stores them as UTC timestamps and filters months in UTC,
 * so a day is sent as its UTC midnight and read back from the date part, independent of the local time zone.
 */
export abstract class DateUtils {
	static toApiDate(date: Date): string {
		return `${DateUtils.toDateKey(date)}T00:00:00.000Z`;
	}

	static fromApiDate(value: string): Date {
		const [year, month, day] = value.slice(0, 10).split('-').map(Number);
		return new Date(year, month - 1, day);
	}

	/** The `YYYY-MM-DD` key of a local date. */
	static toDateKey(date: Date): string {
		return `${date.getFullYear()}-${DateUtils.pad(date.getMonth() + 1)}-${DateUtils.pad(date.getDate())}`;
	}

	private static pad(value: number) {
		return String(value).padStart(2, '0');
	}
}
