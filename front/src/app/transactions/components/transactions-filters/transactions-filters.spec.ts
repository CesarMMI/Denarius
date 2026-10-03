import { HarnessLoader } from '@angular/cdk/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatButtonHarness } from '@angular/material/button/testing';
import { MatFormFieldHarness } from '@angular/material/form-field/testing';
import { MatInputHarness } from '@angular/material/input/testing';
import { MatSelectHarness } from '@angular/material/select/testing';
import { By } from '@angular/platform-browser';
import { buildCategory } from '../../../categories/testing/category-fixture';
import { MonthField } from '../../../shared/month-field/month-field';
import { TransactionFilters } from '../../types/transaction-filters';
import { TransactionsFilters } from './transactions-filters';

@Component({
	imports: [TransactionsFilters],
	template: `<app-transactions-filters [(filters)]="filters" [categories]="categories" />`,
})
class Host {
	readonly filters = signal<TransactionFilters>({ description: '', type: '', categoryId: '', month: null });
	readonly categories = [
		buildCategory({ id: 'mercado', name: 'Mercado' }),
		buildCategory({ id: 'salario', name: 'Salário' }),
	];
}

describe('TransactionsFilters', () => {
	let fixture: ComponentFixture<Host>;
	let host: Host;
	let loader: HarnessLoader;

	beforeEach(async () => {
		fixture = TestBed.createComponent(Host);
		host = fixture.componentInstance;
		loader = TestbedHarnessEnvironment.loader(fixture);
		await fixture.whenStable();
	});

	function field(label: string) {
		return loader.getHarness(MatFormFieldHarness.with({ floatingLabelText: label }));
	}

	async function select(label: string) {
		return (await (await field(label)).getControl(MatSelectHarness))!;
	}

	async function input(label: string) {
		return (await (await field(label)).getControl(MatInputHarness))!;
	}

	function clearButton(label: string) {
		return loader.getHarnessOrNull(MatButtonHarness.with({ selector: `[aria-label="${label}"]` }));
	}

	it('should offer every category', async () => {
		const category = await select('Categoria');
		await category.open();

		const options = await category.getOptions();
		expect(await Promise.all(options.map((option) => option.getText()))).toEqual(['Todas', 'Mercado', 'Salário']);
	});

	it('should filter by the type and the category chosen and the month picked', async () => {
		await (await select('Tipo')).clickOptions({ text: 'Saídas' });
		await (await select('Categoria')).clickOptions({ text: 'Mercado' });
		fixture.debugElement.query(By.directive(MonthField)).componentInstance.value.set(new Date(2026, 8, 1));
		await fixture.whenStable();

		expect(host.filters()).toEqual({
			description: '',
			type: 'Out',
			categoryId: 'mercado',
			month: new Date(2026, 8, 1),
		});
	});

	it('should filter by the description after a pause in the typing', async () => {
		await (await input('Descrição')).setValue('feira');
		expect(host.filters().description).toBe('');

		await new Promise((resolve) => setTimeout(resolve, 300));
		expect(host.filters().description).toBe('feira');
	});

	it('should filter by the description at once when leaving the field', async () => {
		const description = await input('Descrição');
		await description.setValue('feira');
		await description.blur();

		expect(host.filters().description).toBe('feira');
	});

	it.each([
		['Limpar descrição', 'description'],
		['Limpar tipo', 'type'],
		['Limpar categoria', 'categoryId'],
	] as const)('should only offer "%s" while the filter is set, and empty it', async (label, filter) => {
		expect(await clearButton(label)).toBeNull();

		const filters: TransactionFilters = { description: 'feira', type: 'Out', categoryId: 'mercado', month: null };
		host.filters.set(filters);
		await fixture.whenStable();
		await (await clearButton(label))!.click();

		expect(host.filters()).toEqual({ ...filters, [filter]: '' });
		expect(await clearButton(label)).toBeNull();
	});

	it('should not open the select whose filter is cleared', async () => {
		host.filters.update((filters) => ({ ...filters, type: 'Out' }));
		await fixture.whenStable();

		await (await clearButton('Limpar tipo'))!.click();

		expect(await (await select('Tipo')).isOpen()).toBe(false);
	});
});
