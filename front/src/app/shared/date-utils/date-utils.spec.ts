import { DateUtils } from './date-utils';

describe('DateUtils', () => {
	it('toApiDate should send the local calendar day as UTC midnight', () => {
		expect(DateUtils.toApiDate(new Date(2026, 8, 24, 23, 59))).toBe('2026-09-24T00:00:00.000Z');
		expect(DateUtils.toApiDate(new Date(2027, 0, 5))).toBe('2027-01-05T00:00:00.000Z');
	});

	it.each(['2026-09-24T00:00:00Z', '2026-09-24T00:00:00.000Z', '2026-09-24T00:00:00', '2026-09-24'])(
		'fromApiDate should read %s as the local 24 September 2026',
		(value) => {
			expect(DateUtils.fromApiDate(value)).toEqual(new Date(2026, 8, 24));
		},
	);

	it('should round-trip a day', () => {
		const day = new Date(2026, 1, 28);
		expect(DateUtils.fromApiDate(DateUtils.toApiDate(day))).toEqual(day);
	});

	it('toDateKey should zero-pad month and day', () => {
		expect(DateUtils.toDateKey(new Date(2026, 0, 3))).toBe('2026-01-03');
	});

	it('toMonthKey should keep the year and the zero-padded month', () => {
		expect(DateUtils.toMonthKey(new Date(2026, 8, 30))).toBe('2026-09');
		expect(DateUtils.toMonthKey(new Date(2027, 0, 1))).toBe('2027-01');
	});

	it.each([
		['2026-09', new Date(2026, 8, 1)],
		['2027-01', new Date(2027, 0, 1)],
		['2026-12', new Date(2026, 11, 1)],
	])('fromMonthKey should read %s as the first day of the month', (key, month) => {
		expect(DateUtils.fromMonthKey(key)).toEqual(month);
		expect(DateUtils.toMonthKey(DateUtils.fromMonthKey(key)!)).toBe(key);
	});

	it.each([null, '', '2026-13', '2026-00', '2026-9', '2026-09-01', 'setembro'])(
		'fromMonthKey should read %j as no month',
		(key) => {
			expect(DateUtils.fromMonthKey(key)).toBeNull();
		},
	);

	it.each([
		[new Date(2026, 9, 1), new Date(2026, 8, 1), new Date(2026, 10, 1)],
		[new Date(2026, 0, 1), new Date(2025, 11, 1), new Date(2026, 1, 1)],
		[new Date(2026, 11, 1), new Date(2026, 10, 1), new Date(2027, 0, 1)],
		[new Date(2026, 2, 31), new Date(2026, 1, 1), new Date(2026, 3, 1)],
	])('previousMonth and nextMonth of %s should be the first days of the months around it', (date, previous, next) => {
		expect(DateUtils.previousMonth(date)).toEqual(previous);
		expect(DateUtils.nextMonth(date)).toEqual(next);
	});

	it('previousMonth and nextMonth should keep no month as no month', () => {
		expect(DateUtils.previousMonth(null)).toBeNull();
		expect(DateUtils.nextMonth(null)).toBeNull();
	});

	describe('currentMonth', () => {
		afterEach(() => vi.useRealTimers());

		it.each([
			['2026-10-01T02:00:00Z', new Date(2026, 8, 1)],
			['2026-10-01T03:00:00Z', new Date(2026, 9, 1)],
			['2026-01-01T02:59:00Z', new Date(2025, 11, 1)],
		])('should be the month in São Paulo at %s', (now, month) => {
			vi.useFakeTimers({ now: new Date(now) });

			expect(DateUtils.currentMonth()).toEqual(month);
		});
	});
});
