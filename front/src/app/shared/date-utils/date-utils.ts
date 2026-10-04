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

	/** The `YYYY-MM` key of a local date's month. */
	static toMonthKey(date: Date): string {
		return DateUtils.toDateKey(date).slice(0, 7);
	}

	/** The first day of a `YYYY-MM` month as a local date; null for anything else. */
	static fromMonthKey(key: string | null): Date | null {
		const match = key?.match(/^(\d{4})-(0[1-9]|1[0-2])$/);
		return match ? new Date(Number(match[1]), Number(match[2]) - 1, 1) : null;
	}

	/** The first day of the current month in São Paulo, the API's time zone, as a local date. */
	static currentMonth(): Date {
		const parts = new Intl.DateTimeFormat('en-US', {
			timeZone: 'America/Sao_Paulo',
			year: 'numeric',
			month: 'numeric',
		}).formatToParts(new Date());
		const part = (type: 'year' | 'month') => Number(parts.find((p) => p.type === type)?.value);
		return new Date(part('year'), part('month') - 1, 1);
	}

	static previousMonth(date: Date | null): Date | null {
		if (!date) return null;
		return new Date(date.getFullYear(), date.getMonth() - 1, 1);
	}

	static nextMonth(date: Date | null): Date | null {
		if (!date) return null;
		return new Date(date.getFullYear(), date.getMonth() + 1, 1);
	}

	private static pad(value: number) {
		return String(value).padStart(2, '0');
	}
}
