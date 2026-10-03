import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { LOCALE_ID, resourceFromSnapshots, ResourceSnapshot, signal, WritableSignal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatTooltip } from '@angular/material/tooltip';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { MatChipColor } from '../../../shared/mat-chip-color/mat-chip-color';
import { buildCategory } from '../../testing/category-fixture';
import { Category } from '../../types/category';
import { CategoriesTable } from './categories-table';

registerLocaleData(localePt);

describe('CategoriesTable', () => {
	const mercado = buildCategory({
		id: 'mercado',
		name: 'Mercado',
		color: '#43A047',
		transactionCount: 14,
		balance: -1842.55,
		canDelete: false,
	});
	const educacao = buildCategory({
		id: 'educacao',
		name: 'Educação',
		color: '#F6BF26',
		transactionCount: 0,
		balance: 0,
	});
	const salario = buildCategory({
		id: 'salario',
		name: 'Salário',
		color: '#1E88E5',
		transactionCount: 1,
		balance: 8600,
		canDelete: false,
	});

	let fixture: ComponentFixture<CategoriesTable>;
	let element: HTMLElement;
	let categories: WritableSignal<ResourceSnapshot<Category[] | undefined>>;

	beforeEach(async () => {
		TestBed.configureTestingModule({ providers: [{ provide: LOCALE_ID, useValue: 'pt-BR' }, provideRouter([])] });
		categories = signal({ status: 'resolved', value: [mercado, educacao, salario] });

		fixture = TestBed.createComponent(CategoriesTable);
		element = fixture.nativeElement;
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

	function tooltip(button: HTMLButtonElement) {
		return fixture.debugElement
			.queryAll(By.directive(MatTooltip))
			.find((el) => el.nativeElement === button)!
			.injector.get(MatTooltip).message;
	}

	function rowButton(row: number, label: 'Editar' | 'Excluir') {
		return element.querySelectorAll<HTMLButtonElement>(`tr[mat-row] button[aria-label="${label}"]`)[row];
	}

	it('should list the categories as chips in their colors, with their counts and balances', () => {
		expect(column('name')).toEqual(['Mercado', 'Educação', 'Salário']);
		expect(
			fixture.debugElement.queryAll(By.directive(MatChipColor)).map((chip) => chip.injector.get(MatChipColor).color()),
		).toEqual(['#43A047', '#F6BF26', '#1E88E5']);
		expect(column('transactionCount')).toEqual(['14', '0', '1']);

		const [negative, zero, positive] = Array.from(element.querySelectorAll('tr[mat-row] .mat-column-balance'));
		expect(negative.textContent).toContain('1.842,55');
		expect(negative.classList).toContain('negative');
		expect(zero.classList).not.toContain('negative');
		expect(positive.classList).not.toContain('negative');
	});

	it('should link the count to the transactions of the category', () => {
		expect(element.querySelector('tr[mat-row] a')!.getAttribute('href')).toBe('/transactions?categoryId=mercado');
	});

	it('should show a spinner while loading', async () => {
		categories.set({ status: 'loading', value: undefined });
		await fixture.whenStable();

		expect(element.querySelector('td[colspan] mat-progress-spinner')).not.toBeNull();
	});

	it('should say when the categories fail to load', async () => {
		categories.set({ status: 'error', error: new Error('Server Error') });
		await fixture.whenStable();

		expect(column('name')).toEqual([]);
		expect(noDataRow()).toBe('Não foi possível carregar as categorias.');
	});

	it('should say when there are no categories', async () => {
		categories.set({ status: 'resolved', value: [] });
		await fixture.whenStable();

		expect(noDataRow()).toBe('Nenhuma categoria encontrada.');
	});

	it('should emit the category to edit or delete', () => {
		const edit = vi.fn();
		const remove = vi.fn();
		fixture.componentInstance.edit.subscribe(edit);
		fixture.componentInstance.delete.subscribe(remove);

		rowButton(2, 'Editar').click();
		rowButton(1, 'Excluir').click();

		expect(edit).toHaveBeenCalledWith(salario);
		expect(remove).toHaveBeenCalledWith(educacao);
	});

	it('should disable deleting a category with transactions and say why', () => {
		const remove = vi.fn();
		fixture.componentInstance.delete.subscribe(remove);
		const [withTransactions, withoutTransactions] = [rowButton(0, 'Excluir'), rowButton(1, 'Excluir')];

		withTransactions.click();

		expect(remove).not.toHaveBeenCalled();
		expect(withTransactions.getAttribute('aria-disabled')).toBe('true');
		expect(withoutTransactions.getAttribute('aria-disabled')).toBeNull();
		expect(tooltip(withTransactions)).toBe(
			'Não é possível excluir uma categoria com transações, mesmo que em outros meses.',
		);
		expect(tooltip(withoutTransactions)).toBe('Excluir');
	});

	it('should mark the column sorted and take the sort chosen', async () => {
		expect(element.querySelector('th.mat-column-name')!.getAttribute('aria-sort')).toBe('ascending');

		element.querySelector<HTMLElement>('th.mat-column-balance')!.click();
		await fixture.whenStable();

		expect(fixture.componentInstance.sort()).toEqual({ active: 'balance', direction: 'asc' });
		expect(element.querySelector('th.mat-column-balance')!.getAttribute('aria-sort')).toBe('ascending');
	});
});
