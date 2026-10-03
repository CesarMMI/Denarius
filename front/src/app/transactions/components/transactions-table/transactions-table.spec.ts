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
		expect(column('date')).toEqual(['24/09/2026', '05/09/2026']);
		expect(column('description')).toEqual(['Feira', 'Salário']);
		expect(column('category')).toEqual(['Mercado', 'Salário']);
		expect(
			fixture.debugElement.queryAll(By.directive(MatChipColor)).map((chip) => chip.injector.get(MatChipColor).color()),
		).toEqual(['#43A047', '#1E88E5']);
	});

	it('should mark the negative values and total them', () => {
		const [expense, income] = Array.from(element.querySelectorAll('tr[mat-row] .mat-column-value'));
		expect(expense.textContent).toContain('186,42');
		expect(expense.classList).toContain('negative');
		expect(income.textContent).toContain('8.600,00');
		expect(income.classList).not.toContain('negative');

		const total = element.querySelector('tr[mat-footer-row] .mat-column-value')!;
		expect(total.textContent).toContain('8.413,58');
		expect(total.classList).not.toContain('negative');
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

	it('should mark the column sorted and take the sort chosen', async () => {
		expect(element.querySelector('th.mat-column-date')!.getAttribute('aria-sort')).toBe('descending');

		element.querySelector<HTMLElement>('th.mat-column-value')!.click();
		await fixture.whenStable();

		expect(fixture.componentInstance.sort()).toEqual({ active: 'Value', direction: 'asc' });
		expect(element.querySelector('th.mat-column-value')!.getAttribute('aria-sort')).toBe('ascending');
	});
});
