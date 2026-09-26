import { fromApiDate, toApiDate, toDateKey } from './api-date';

describe('api-date', () => {
	it('toApiDate should send the local calendar day as UTC midnight', () => {
		expect(toApiDate(new Date(2026, 8, 24, 23, 59))).toBe('2026-09-24T00:00:00.000Z');
		expect(toApiDate(new Date(2027, 0, 5))).toBe('2027-01-05T00:00:00.000Z');
	});

	it.each(['2026-09-24T00:00:00Z', '2026-09-24T00:00:00.000Z', '2026-09-24T00:00:00', '2026-09-24'])(
		'fromApiDate should read %s as the local 24 September 2026',
		(value) => {
			expect(fromApiDate(value)).toEqual(new Date(2026, 8, 24));
		},
	);

	it('should round-trip a day', () => {
		const day = new Date(2026, 1, 28);
		expect(fromApiDate(toApiDate(day))).toEqual(day);
	});

	it('toDateKey should zero-pad month and day', () => {
		expect(toDateKey(new Date(2026, 0, 3))).toBe('2026-01-03');
	});
});
