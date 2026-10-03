import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { LOCALE_ID, resourceFromSnapshots, ResourceSnapshot, signal, WritableSignal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { buildCategory } from '../../../categories/testing/category-fixture';
import { Category } from '../../../categories/types/category';
import { MatChipColor } from '../../../shared/mat-chip-color/mat-chip-color';
import { buildTransaction } from '../../testing/transaction-fixture';
import { Transaction } from '../../types/transaction';
import { TransactionsTable } from './transactions-table';

registerLocaleData(localePt);

describe('TransactionsTable', () => {
	const mercado = buildCategory({ id: 'mercado', name: 'Mercado', color: '#43A047' });
	const salario = buildCategory({ id: 'salario', name: 'Salário', color: '#1E88E5' });
	const feira = buildTransaction({
		id: 't1',
		description: 'Feira',
		categoryId: 'mercado',
		value: -186.42,
		date: '2026-09-24T00:00:00Z',
	});
	const pagamento = buildTransaction({
		id: 't2',
		description: 'Salário',
		categoryId: 'salario',
		value: 8600,
		date: '2026-09-05T00:00:00Z',
	});

	const loading = { status: 'loading', value: undefined } as const;
	const failed = { status: 'error', error: new Error('Server Error') } as const;

	let fixture: ComponentFixture<TransactionsTable>;
	let element: HTMLElement;
	let transactions: WritableSignal<ResourceSnapshot<Transaction[] | undefined>>;
	let categories: WritableSignal<ResourceSnapshot<Category[] | undefined>>;

	beforeEach(async () => {
		TestBed.configureTestingModule({ providers: [{ provide: LOCALE_ID, useValue: 'pt-BR' }] });
		transactions = signal({ status: 'resolved', value: [feira, pagamento] });
		categories = signal({ status: 'resolved', value: [mercado, salario] });

		fixture = TestBed.createComponent(TransactionsTable);
		element = fixture.nativeElement;
		fixture.componentRef.setInput('transactions', resourceFromSnapshots(transactions));
		fixture.componentRef.setInput('categories', resourceFromSnapshots(categories));
		await fixture.whenStable();
	});

	function column(name: string) {
		return Array.from(element.querySelectorAll(`tr[mat-row] .mat-column-${name}`)).map((cell) =>
			cell.textContent?.trim(),
		);
	}

	function noDataRow() {
		return element.querySelector('td[colspan]')?.textContent?.trim();
	}

	function rowButton(row: number, label: 'Editar' | 'Excluir') {
		return element.querySelectorAll<HTMLButtonElement>(`tr[mat-row] button[aria-label="${label}"]`)[row];
	}

	it('should list the transactions with a chip in the color of their category', () => {
		expect(column('description')).toEqual(['Feira', 'Salário']);
		expect(column('category')).toEqual(['Mercado', 'Salário']);
		expect(
			fixture.debugElement.queryAll(By.directive(MatChipColor)).map((chip) => chip.injector.get(MatChipColor).color()),
		).toEqual(['#43A047', '#1E88E5']);
	});

	it('should open each day with its date and total, keeping the order of the API', async () => {
		const padaria = buildTransaction({ id: 't3', description: 'Padaria', value: -13.58, date: '2026-09-24T00:00:00Z' });
		transactions.set({ status: 'resolved', value: [padaria, feira, pagamento] });
		await fixture.whenStable();

		const rows = Array.from(element.querySelectorAll('tr[mat-row]'));
		expect(rows.map((row) => row.classList.contains('day'))).toEqual([true, false, false, true, false]);
		expect(column('dayDate')).toEqual(['quinta-feira, 24 de setembro de 2026', 'sábado, 5 de setembro de 2026']);
		const [expenses, income] = Array.from(element.querySelectorAll('tr.day .mat-column-dayTotal'));
		expect(expenses.textContent).toContain('Total:');
		expect(expenses.textContent).toContain('-R$');
		expect(expenses.textContent).toContain('200,00');
		expect(expenses.classList).toContain('negative');
		expect(income.textContent).toContain('8.600,00');
		expect(income.classList).not.toContain('negative');
		expect(column('description')).toEqual(['Padaria', 'Feira', 'Salário']);
	});

	it('should collapse and expand the transactions of a day, keeping its total', async () => {
		const toggle = () => element.querySelector<HTMLButtonElement>('tr.day .mat-column-dayToggle button')!;
		expect(toggle().getAttribute('aria-label')).toBe('Recolher');
		expect(toggle().getAttribute('aria-expanded')).toBe('true');

		toggle().click();
		await fixture.whenStable();

		expect(column('description')).toEqual(['Salário']);
		expect(column('dayDate')).toEqual(['quinta-feira, 24 de setembro de 2026', 'sábado, 5 de setembro de 2026']);
		expect(element.querySelector('tr.day .mat-column-dayTotal')!.textContent).toContain('186,42');
		expect(toggle().getAttribute('aria-label')).toBe('Expandir');
		expect(toggle().getAttribute('aria-expanded')).toBe('false');

		toggle().click();
		await fixture.whenStable();

		expect(column('description')).toEqual(['Feira', 'Salário']);
	});

	it('should keep the rows of the other days when one collapses', async () => {
		const [, day] = Array.from(element.querySelectorAll('tr.day'));
		const transaction = Array.from(element.querySelectorAll('tr[mat-row]:not(.day)'))[1];

		element.querySelector<HTMLButtonElement>('tr.day .mat-column-dayToggle button')!.click();
		await fixture.whenStable();

		expect(element.querySelectorAll('tr.day')[1]).toBe(day);
		expect(element.querySelector('tr[mat-row]:not(.day)')).toBe(transaction);
	});

	it('should collapse every day from the header, and expand them all once they are', async () => {
		const toggleAll = () => element.querySelector<HTMLButtonElement>('th.mat-column-toggle button')!;
		const toggleDay = () => element.querySelector<HTMLButtonElement>('tr.day .mat-column-dayToggle button')!;
		expect(toggleAll().getAttribute('aria-label')).toBe('Recolher todos');

		toggleDay().click();
		await fixture.whenStable();
		expect(toggleAll().getAttribute('aria-label')).toBe('Recolher todos');

		toggleAll().click();
		await fixture.whenStable();

		expect(column('description')).toEqual([]);
		expect(column('dayDate').length).toBe(2);
		expect(toggleAll().getAttribute('aria-label')).toBe('Expandir todos');
		expect(toggleAll().getAttribute('aria-expanded')).toBe('false');

		toggleAll().click();
		await fixture.whenStable();

		expect(column('description')).toEqual(['Feira', 'Salário']);
		expect(toggleAll().getAttribute('aria-label')).toBe('Recolher todos');
	});

	it('should drop the actions column only while every day is collapsed', async () => {
		const actions = () => element.querySelectorAll('.mat-column-actions, .mat-column-dayActions').length;
		const toggleDays = () => element.querySelectorAll<HTMLButtonElement>('tr.day .mat-column-dayToggle button');
		expect(actions()).toBeGreaterThan(0);

		toggleDays()[0].click();
		await fixture.whenStable();
		expect(actions()).toBeGreaterThan(0);

		toggleDays()[1].click();
		await fixture.whenStable();
		expect(actions()).toBe(0);
		expect(element.querySelector('tr.day td:last-child')!.classList).toContain('mat-column-dayTotal');
	});

	it('should disable the toggle in the header without transactions', async () => {
		transactions.set({ status: 'resolved', value: [] });
		await fixture.whenStable();

		expect(element.querySelector<HTMLButtonElement>('th.mat-column-toggle button')!.disabled).toBe(true);
	});

	it('should mark the negative values', () => {
		const [expense, income] = Array.from(element.querySelectorAll('tr[mat-row] .mat-column-value'));
		expect(expense.textContent).toContain('186,42');
		expect(expense.classList).toContain('negative');
		expect(income.textContent).toContain('8.600,00');
		expect(income.classList).not.toContain('negative');
	});

	it('should say when there are no transactions', async () => {
		transactions.set({ status: 'resolved', value: [] });
		await fixture.whenStable();

		expect(noDataRow()).toBe('Nenhuma transação encontrada.');
	});

	describe.each([
		['the transactions', () => transactions],
		['the categories', () => categories],
	] as const)('when %s', (_, list) => {
		it('load, should show a spinner instead of the rows', async () => {
			list().set(loading);
			await fixture.whenStable();

			expect(column('description')).toEqual([]);
			expect(element.querySelector('td[colspan] mat-progress-spinner')).not.toBeNull();
		});

		it('fail, should say so instead of the rows', async () => {
			list().set(failed);
			await fixture.whenStable();

			expect(column('description')).toEqual([]);
			expect(noDataRow()).toBe('Não foi possível carregar as transações.');
		});
	});

	it('should emit the transaction to edit or delete', () => {
		const edit = vi.fn();
		const remove = vi.fn();
		fixture.componentInstance.edit.subscribe(edit);
		fixture.componentInstance.delete.subscribe(remove);

		rowButton(1, 'Editar').click();
		rowButton(0, 'Excluir').click();

		expect(edit).toHaveBeenCalledWith(pagamento);
		expect(remove).toHaveBeenCalledWith(feira);
	});
});
