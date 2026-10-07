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

	it('should show a name with markup as plain text in the chip', async () => {
		categories.set({ status: 'resolved', value: [buildCategory({ id: 'markup', name: '<b>teste</b>' })] });
		await fixture.whenStable();

		const chip = element.querySelector('tr[mat-row] mat-chip')!;
		expect(chip.textContent?.trim()).toBe('<b>teste</b>');
		expect(chip.querySelector('b')).toBeNull();
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

	it('should show "Editar" as the tooltip of the edit button', () => {
		expect(tooltip(rowButton(0, 'Editar'))).toBe('Editar');
	});

	describe('while a category is being deleted', () => {
		const pets = buildCategory({ id: 'pets', name: 'Pets', canDelete: true });

		beforeEach(async () => {
			categories.set({ status: 'resolved', value: [mercado, educacao, pets] });
			fixture.componentRef.setInput('deleting', new Set([educacao.id]));
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

			rowButton(1, 'Excluir').click();
			rowButton(1, 'Editar').click();

			expect(rowButton(1, 'Excluir').getAttribute('aria-disabled')).toBe('true');
			expect(rowButton(1, 'Editar').getAttribute('aria-disabled')).toBe('true');
			expect(spinnerIn(rowButton(1, 'Excluir'))).not.toBeNull();
			expect(remove).not.toHaveBeenCalled();
			expect(edit).not.toHaveBeenCalled();
		});

		it('should leave the other rows as they are', () => {
			const edit = vi.fn();
			const remove = vi.fn();
			fixture.componentInstance.edit.subscribe(edit);
			fixture.componentInstance.delete.subscribe(remove);

			expect(rowButton(2, 'Excluir').getAttribute('aria-disabled')).toBeNull();
			expect(rowButton(2, 'Editar').getAttribute('aria-disabled')).toBeNull();
			expect(spinnerIn(rowButton(2, 'Excluir'))).toBeNull();
			expect(spinnerIn(rowButton(0, 'Excluir'))).toBeNull();
			expect(rowButton(0, 'Excluir').getAttribute('aria-disabled')).toBe('true');
			expect(tooltip(rowButton(0, 'Excluir'))).toBe(
				'Não é possível excluir uma categoria com transações, mesmo que em outros meses.',
			);
			expect(tooltip(rowButton(1, 'Excluir'))).toBe('Excluir');
			expect(tooltip(rowButton(2, 'Excluir'))).toBe('Excluir');

			rowButton(2, 'Excluir').click();
			rowButton(0, 'Editar').click();

			expect(remove).toHaveBeenCalledWith(pets);
			expect(edit).toHaveBeenCalledWith(mercado);
		});
	});
});
