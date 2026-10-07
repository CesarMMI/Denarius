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

const MERCADO_ID = '3f2a1c4e-0000-4000-8000-0000000000a1';
const SALARIO_ID = '3f2a1c4e-0000-4000-8000-0000000000a2';
const FEIRA_ID = '7b1d2e3f-0000-4000-8000-0000000000b1';
const PAGAMENTO_ID = '7b1d2e3f-0000-4000-8000-0000000000b2';
const PADARIA_ID = '7b1d2e3f-0000-4000-8000-0000000000b3';

describe('TransactionsTable', () => {
	const mercado = buildCategory({ id: MERCADO_ID, name: 'Mercado', color: '#43A047' });
	const salario = buildCategory({ id: SALARIO_ID, name: 'Salário', color: '#1E88E5' });
	const feira = buildTransaction({
		id: FEIRA_ID,
		description: 'Feira',
		categoryId: MERCADO_ID,
		value: -186.42,
		date: '2026-09-24T00:00:00Z',
	});
	const pagamento = buildTransaction({
		id: PAGAMENTO_ID,
		description: 'Salário',
		categoryId: SALARIO_ID,
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

	it('should mark the negative values', () => {
		const [expense, income] = Array.from(element.querySelectorAll('tr[mat-row] .mat-column-value'));
		expect(expense.textContent).toContain('186,42');
		expect(expense.classList).toContain('negative');
		expect(income.textContent).toContain('8.600,00');
		expect(income.classList).not.toContain('negative');
	});

	it('should mark the transactions followed by another of the same date', async () => {
		const padaria = buildTransaction({
			id: PADARIA_ID,
			description: 'Padaria',
			value: -13.58,
			date: '2026-09-24T00:00:00Z',
		});
		transactions.set({ status: 'resolved', value: [padaria, feira, pagamento] });
		await fixture.whenStable();

		const rows = Array.from(element.querySelectorAll('tr[mat-row]'));
		expect(rows.map((row) => row.classList.contains('same-date'))).toEqual([true, false, false]);
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

	describe('when reloading', () => {
		function spinner() {
			return element.querySelector('td[colspan] mat-progress-spinner');
		}

		it('should keep the rows in sight, without a spinner', async () => {
			transactions.set({ status: 'reloading', value: [feira, pagamento] });
			await fixture.whenStable();

			expect(column('description')).toEqual(['Feira', 'Salário']);
			expect(spinner()).toBeNull();
		});

		it('an empty list, should show a spinner instead of the empty message', async () => {
			transactions.set({ status: 'resolved', value: [] });
			await fixture.whenStable();
			transactions.set({ status: 'reloading', value: [] });
			await fixture.whenStable();

			expect(spinner()).not.toBeNull();
			expect(noDataRow()).toBe('');
		});

		it('after an error, should show a spinner instead of the error message', async () => {
			transactions.set(failed);
			await fixture.whenStable();
			transactions.set({ status: 'reloading', value: undefined });
			await fixture.whenStable();

			expect(spinner()).not.toBeNull();
			expect(noDataRow()).toBe('');
		});
	});

	it('should show the description and the category name as plain text', async () => {
		const markup = buildCategory({ id: MERCADO_ID, name: '<b>teste</b>' });
		transactions.set({ status: 'resolved', value: [{ ...feira, description: '<b>teste</b>' }] });
		categories.set({ status: 'resolved', value: [markup] });
		await fixture.whenStable();

		const description = element.querySelector('tr[mat-row] .mat-column-description')!;
		const chip = element.querySelector('tr[mat-row] .mat-column-category mat-chip')!;
		expect(description.textContent).toContain('<b>teste</b>');
		expect(chip.textContent).toContain('<b>teste</b>');
		expect(description.querySelector('b')).toBeNull();
		expect(chip.querySelector('b')).toBeNull();
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

	describe('while a transaction is being deleted', () => {
		beforeEach(async () => {
			fixture.componentRef.setInput('deleting', new Set([feira.id]));
			await fixture.whenStable();
		});

		function spinnerIn(button: HTMLButtonElement) {
			return button.querySelector('mat-progress-spinner');
		}

		it('should disable its "Excluir" and "Editar", show a spinner in "Excluir" and emit nothing', () => {
			const edit = vi.fn();
			const remove = vi.fn();
			fixture.componentInstance.edit.subscribe(edit);
			fixture.componentInstance.delete.subscribe(remove);

			rowButton(0, 'Excluir').click();
			rowButton(0, 'Editar').click();

			expect(rowButton(0, 'Excluir').getAttribute('aria-disabled')).toBe('true');
			expect(rowButton(0, 'Editar').getAttribute('aria-disabled')).toBe('true');
			expect(spinnerIn(rowButton(0, 'Excluir'))).not.toBeNull();
			expect(remove).not.toHaveBeenCalled();
			expect(edit).not.toHaveBeenCalled();
		});

		it('should leave the other rows as they are', () => {
			const edit = vi.fn();
			const remove = vi.fn();
			fixture.componentInstance.edit.subscribe(edit);
			fixture.componentInstance.delete.subscribe(remove);

			expect(rowButton(1, 'Excluir').getAttribute('aria-disabled')).toBeNull();
			expect(rowButton(1, 'Editar').getAttribute('aria-disabled')).toBeNull();
			expect(spinnerIn(rowButton(1, 'Excluir'))).toBeNull();

			rowButton(1, 'Excluir').click();
			rowButton(1, 'Editar').click();

			expect(remove).toHaveBeenCalledWith(pagamento);
			expect(edit).toHaveBeenCalledWith(pagamento);
		});
	});
});
