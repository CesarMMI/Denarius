import { compactMoney, money, monthName, percentage } from './chart-formats';

/** Intl separates the currency with a non-breaking space. */
function plain(text: string) {
	return text.replace(/\s/g, ' ');
}

describe('chart formats', () => {
	it('money should write BRL in pt-BR', () => {
		expect(plain(money.format(1234.5))).toBe('R$ 1.234,50');
		expect(plain(money.format(-50))).toBe('-R$ 50,00');
	});

	it('compactMoney should shorten values for an axis', () => {
		expect(plain(compactMoney.format(1200))).toBe('R$ 1,2 mil');
		expect(plain(compactMoney.format(0))).toBe('R$ 0');
	});

	it.each([
		['2026-09', 'long', 'setembro de 2026'],
		['2026-09', 'short', 'set.'],
		['2025-12', 'long', 'dezembro de 2025'],
	] as const)('monthName should name %s (%s) as "%s"', (month, style, name) => {
		expect(monthName(month, style)).toBe(name);
	});

	it('monthName should also take a date', () => {
		expect(monthName(new Date(2026, 0, 1))).toBe('janeiro de 2026');
	});

	it('percentage should write a 0–100 value with up to two decimals', () => {
		expect(percentage(23.08)).toBe('23,08%');
		expect(percentage(50)).toBe('50%');
	});
});
