import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { LOCALE_ID, resourceFromSnapshots, ResourceSnapshot, signal, WritableSignal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { MatChipColor } from '../../../shared/mat-chip-color/mat-chip-color';
import { buildMonthlyTransaction } from '../../testing/report-fixtures';
import { MonthlyTransaction } from '../../types/report';
import { TransactionsList } from './transactions-list';

registerLocaleData(localePt);

describe('TransactionsList', () => {
	const presente = buildMonthlyTransaction({
		id: 't1',
		date: '2026-09-28T00:00:00Z',
		description: 'Presente',
		categoryName: 'Presentes',
		color: '#D81B60',
		type: 'out',
		amount: 70,
	});
	const salario = buildMonthlyTransaction({
		id: 't2',
		date: '2026-09-05T00:00:00Z',
		description: null,
		categoryName: 'Salário',
		color: '#1E88E5',
		type: 'in',
		amount: 8000,
	});

	let fixture: ComponentFixture<TransactionsList>;
	let element: HTMLElement;
	let transactions: WritableSignal<ResourceSnapshot<MonthlyTransaction[] | undefined>>;

	beforeEach(async () => {
		TestBed.configureTestingModule({ providers: [{ provide: LOCALE_ID, useValue: 'pt-BR' }, provideRouter([])] });
		transactions = signal({ status: 'resolved', value: [presente, salario] });

		fixture = TestBed.createComponent(TransactionsList);
		element = fixture.nativeElement;
		fixture.componentRef.setInput('transactions', resourceFromSnapshots(transactions));
		fixture.componentRef.setInput('month', new Date(2026, 8, 1));
		await fixture.whenStable();
	});

	function text(node: Element | null) {
		return node?.textContent?.replace(/\s+/g, ' ').trim();
	}

	function column(name: string) {
		return Array.from(element.querySelectorAll(`tr[mat-row] .mat-column-${name}`));
	}

	function link() {
		return element.querySelector('a');
	}

	it('should list the transactions in the API order with their date and category', () => {
		expect(column('date').map(text)).toEqual(['28/09', '05/09']);
		expect(column('description').map(text)).toEqual(['Presente', 'Salário']);
	});

	it('should show each description in a chip, the category name when there is no description', () => {
		const chips = column('description').map((cell) => cell.querySelector('mat-chip'));

		expect(chips.map(text)).toEqual(['Presente', 'Salário']);
		expect(chips.every((chip) => chip !== null)).toBe(true);
	});

	it('should not repeat the category name under the description', () => {
		expect(element.querySelector('.category')).toBeNull();
		expect(column('description').map(text)).not.toContain('Presente Presentes');
	});

	it('should paint each chip with the color the API gave its category, the same for the same category', async () => {
		const feira = buildMonthlyTransaction({ id: 't3', description: 'Feira', categoryName: 'Mercado', color: '#43A047' });
		const padaria = buildMonthlyTransaction({
			id: 't4',
			description: 'Padaria',
			categoryName: 'Mercado',
			color: '#43A047',
		});
		transactions.set({ status: 'resolved', value: [presente, feira, salario, padaria] });
		await fixture.whenStable();

		const colors = fixture.debugElement
			.queryAll(By.directive(MatChipColor))
			.map((chip) => chip.injector.get(MatChipColor).color());

		expect(colors).toEqual(['#D81B60', '#43A047', '#1E88E5', '#43A047']);
	});

	it('should keep the chip text in its own label, the element that gets cut with an ellipsis', () => {
		const labels = column('description').map((cell) => cell.querySelector('mat-chip .label'));

		expect(labels.map(text)).toEqual(['Presente', 'Salário']);
	});

	it('should mark the transactions followed by another of the same day, whatever the time', async () => {
		const manha = buildMonthlyTransaction({ id: 'a', date: '2026-09-10T00:00:00Z' });
		const tarde = buildMonthlyTransaction({ id: 'b', date: '2026-09-10T15:30:00Z' });
		const ontem = buildMonthlyTransaction({ id: 'c', date: '2026-09-09T00:00:00Z' });
		transactions.set({ status: 'resolved', value: [manha, tarde, ontem] });
		await fixture.whenStable();

		const rows = Array.from(element.querySelectorAll('tr[mat-row]'));
		expect(rows.map((row) => row.classList.contains('same-date'))).toEqual([true, false, false]);
	});

	it('should not mark the rows of different days, nor a single row', async () => {
		expect(Array.from(element.querySelectorAll('tr[mat-row].same-date'))).toHaveLength(0);

		transactions.set({ status: 'resolved', value: [presente] });
		await fixture.whenStable();
		expect(Array.from(element.querySelectorAll('tr[mat-row].same-date'))).toHaveLength(0);
	});

	it('should look only at the ten rows shown when marking the same day', async () => {
		const days = Array.from({ length: 11 }, (_, index) =>
			buildMonthlyTransaction({ id: `d${index}`, date: `2026-09-${String(28 - index).padStart(2, '0')}T00:00:00Z` }),
		);
		days[10] = { ...days[10], date: days[9].date };
		transactions.set({ status: 'resolved', value: days });
		await fixture.whenStable();

		const rows = Array.from(element.querySelectorAll('tr[mat-row]'));
		expect(rows).toHaveLength(10);
		expect(rows.some((row) => row.classList.contains('same-date'))).toBe(false);
	});

	it('should sign the amounts and mark the expenses in red', () => {
		const [expense, income] = column('amount');

		expect(text(expense)).toBe('-R$ 70,00');
		expect(expense.classList).toContain('negative');
		expect(text(income)).toBe('R$ 8.000,00');
		expect(income.classList).not.toContain('negative');
	});

	it('should count the transactions in the title and keep the header in view while the list scrolls', () => {
		expect(text(element.querySelector('mat-card-title'))).toBe('Transações do mês (2)');
		expect(element.querySelector('tr[mat-header-row] th')!.classList).toContain('mat-mdc-table-sticky');
		expect(element.querySelector('.scroll table')).not.toBeNull();
	});

	it('should list only the ten most recent, still counting them all', async () => {
		const month = Array.from({ length: 12 }, (_, index) =>
			buildMonthlyTransaction({ id: `t${index}`, date: `2026-09-${String(28 - index).padStart(2, '0')}T00:00:00Z` }),
		);
		transactions.set({ status: 'resolved', value: month });
		await fixture.whenStable();

		expect(column('date').map(text)).toEqual(month.slice(0, 10).map(({ date }) => `${date.slice(8, 10)}/09`));
		expect(text(element.querySelector('mat-card-title'))).toBe('Transações do mês (12)');
	});

	it('should link to every transaction of the month on the transactions page', async () => {
		expect(text(link())).toBe('Ver todas');
		expect(link()!.getAttribute('href')).toBe('/transactions?month=2026-09');

		fixture.componentRef.setInput('month', new Date(2027, 0, 1));
		await fixture.whenStable();
		expect(link()!.getAttribute('href')).toBe('/transactions?month=2027-01');
	});

	it('should say when the month has no transactions', async () => {
		transactions.set({ status: 'resolved', value: [] });
		await fixture.whenStable();

		expect(element.querySelector('table')).toBeNull();
		expect(link()).toBeNull();
		expect(text(element.querySelector('mat-card-content p'))).toBe('Sem movimentações neste mês.');
	});

	it('should show a spinner while loading', async () => {
		transactions.set({ status: 'loading', value: undefined });
		await fixture.whenStable();

		expect(element.querySelector('table')).toBeNull();
		expect(element.querySelector('mat-progress-spinner')).not.toBeNull();
	});

	it('should say when the transactions fail to load, and retry them', async () => {
		const retry = vi.fn();
		fixture.componentInstance.retry.subscribe(retry);
		transactions.set({ status: 'error', error: new Error('Server Error') });
		await fixture.whenStable();

		expect(text(element.querySelector('mat-card-content p'))).toBe('Não foi possível carregar as transações do mês.');
		Array.from(element.querySelectorAll('button'))
			.find((b) => b.textContent?.includes('Tentar novamente'))!
			.click();
		expect(retry).toHaveBeenCalledOnce();
	});
});
