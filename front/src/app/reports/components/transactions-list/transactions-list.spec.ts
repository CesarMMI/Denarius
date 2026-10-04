import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { LOCALE_ID, resourceFromSnapshots, ResourceSnapshot, signal, WritableSignal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
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
		type: 'out',
		amount: 70,
	});
	const salario = buildMonthlyTransaction({
		id: 't2',
		date: '2026-09-05T00:00:00Z',
		description: null,
		categoryName: 'Salário',
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
		expect(column('description').map((cell) => Array.from(cell.querySelectorAll('div')).map(text))).toEqual([
			['Presente', 'Presentes'],
			['Salário'],
		]);
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
