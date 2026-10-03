import { HarnessLoader } from '@angular/cdk/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatButtonHarness } from '@angular/material/button/testing';
import { MatFormFieldHarness } from '@angular/material/form-field/testing';
import { MatInputHarness } from '@angular/material/input/testing';
import { MatSelectHarness } from '@angular/material/select/testing';
import { By } from '@angular/platform-browser';
import { MonthField } from '../../../shared/month-field/month-field';
import { CategoryFilters } from '../../types/category-filters';
import { CategoriesFilters } from './categories-filters';

@Component({
	imports: [CategoriesFilters],
	template: `<app-categories-filters [(filters)]="filters" />`,
})
class Host {
	readonly filters = signal<CategoryFilters>({ name: '', withTransaction: '', month: null });
}

describe('CategoriesFilters', () => {
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

	function clearButton(label: string) {
		return loader.getHarnessOrNull(MatButtonHarness.with({ selector: `[aria-label="${label}"]` }));
	}

	async function name() {
		return (await (await field('Nome')).getControl(MatInputHarness))!;
	}

	it('should filter by the transactions chosen and the month picked', async () => {
		await (await (await field('Transações no período')).getControl(MatSelectHarness))!.clickOptions({
			text: 'Sem transações',
		});
		fixture.debugElement.query(By.directive(MonthField)).componentInstance.value.set(new Date(2026, 8, 1));
		await fixture.whenStable();

		expect(host.filters()).toEqual({ name: '', withTransaction: false, month: new Date(2026, 8, 1) });
	});

	it('should filter by the name after a pause in the typing', async () => {
		await (await name()).setValue('mer');
		expect(host.filters().name).toBe('');

		await new Promise((resolve) => setTimeout(resolve, 300));
		expect(host.filters().name).toBe('mer');
	});

	it('should filter by the name at once when leaving the field', async () => {
		const input = await name();
		await input.setValue('mer');
		await input.blur();

		expect(host.filters().name).toBe('mer');
	});

	it.each([
		['Limpar nome', 'name'],
		['Limpar transações no período', 'withTransaction'],
	] as const)('should only offer "%s" while the filter is set, and empty it', async (label, filter) => {
		expect(await clearButton(label)).toBeNull();

		const filters: CategoryFilters = { name: 'mer', withTransaction: false, month: null };
		host.filters.set(filters);
		await fixture.whenStable();
		await (await clearButton(label))!.click();

		expect(host.filters()).toEqual({ ...filters, [filter]: '' });
		expect(await clearButton(label)).toBeNull();
	});
});
