// How the charts write values in their tooltips, axes and labels, in pt-BR like the rest of the page.

/** Money as the currency pipe writes it: R$ 1.234,56. */
export const money = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });

/** Money for value axes: R$ 1,2 mil. */
export const compactMoney = new Intl.NumberFormat('pt-BR', {
	style: 'currency',
	currency: 'BRL',
	notation: 'compact',
	maximumFractionDigits: 1,
});

const longMonth = new Intl.DateTimeFormat('pt-BR', { month: 'long', year: 'numeric' });
const shortMonth = new Intl.DateTimeFormat('pt-BR', { month: 'short' });

/** A month spelled out, "setembro de 2026", or abbreviated for an axis, "set.". Takes `YYYY-MM` or a date. */
export function monthName(month: string | Date, style: 'long' | 'short' = 'long') {
	const date = typeof month === 'string' ? new Date(Number(month.slice(0, 4)), Number(month.slice(5, 7)) - 1, 1) : month;
	return (style === 'long' ? longMonth : shortMonth).format(date);
}

/** A percentage on a 0–100 scale: 23,08%. */
export function percentage(value: number) {
	return `${value.toLocaleString('pt-BR', { maximumFractionDigits: 2 })}%`;
}
