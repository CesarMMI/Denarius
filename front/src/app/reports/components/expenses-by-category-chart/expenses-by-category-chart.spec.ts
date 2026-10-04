import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { LOCALE_ID, resourceFromSnapshots, ResourceSnapshot, signal, WritableSignal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { TooltipItem, TooltipModel } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { ChartColors, ChartThemeService } from '../../services/chart-theme.service';
import { FakeChart } from '../../testing/fake-chart';
import { buildCategoryExpense, buildExpensesByCategory } from '../../testing/report-fixtures';
import { ExpensesByCategory } from '../../types/report';
import { ExpensesByCategoryChart } from './expenses-by-category-chart';

registerLocaleData(localePt);

describe('ExpensesByCategoryChart', () => {
	const colors: ChartColors = {
		expense: 'red',
		income: 'green',
		neutral: 'gray',
		primary: 'amber',
		primaryVariant: 'brown',
		text: 'ink',
		grid: 'line',
		surface: 'card',
	};

	let fixture: ComponentFixture<ExpensesByCategoryChart>;
	let element: HTMLElement;
	let expenses: WritableSignal<ResourceSnapshot<ExpensesByCategory | undefined>>;

	beforeEach(async () => {
		TestBed.configureTestingModule({
			providers: [
				{ provide: LOCALE_ID, useValue: 'pt-BR' },
				{ provide: ChartThemeService, useValue: { colors: signal(colors) } },
			],
		});
		TestBed.overrideComponent(ExpensesByCategoryChart, {
			remove: { imports: [BaseChartDirective] },
			add: { imports: [FakeChart] },
		});
		expenses = signal({ status: 'resolved', value: buildExpensesByCategory() });

		fixture = TestBed.createComponent(ExpensesByCategoryChart);
		element = fixture.nativeElement;
		fixture.componentRef.setInput('expenses', resourceFromSnapshots(expenses));
		await fixture.whenStable();
	});

	function chart() {
		return fixture.debugElement.query(By.directive(FakeChart))?.injector.get(FakeChart);
	}

	function text(node: Element | null) {
		return node?.textContent?.replace(/\s+/g, ' ').trim();
	}

	it('should draw a slice per category, largest first, in its color, separated by the color of the card', () => {
		const data = chart()!.data()!;

		expect(chart()!.type()).toBe('doughnut');
		expect(data.datasets[0].data).toEqual([2950, 2000, 250]);
		expect(data.datasets[0].backgroundColor).toEqual(['#43A047', '#8E24AA', 'gray']);
		expect(data.datasets[0]).toEqual(expect.objectContaining({ borderColor: 'card', borderWidth: 4 }));
	});

	describe('past five categories', () => {
		// What the API sends past eight categories: the seven largest and its own "Outras".
		const categories = [
			buildCategoryExpense({
				categoryId: 'mercado',
				categoryName: 'Mercado',
				color: '#43A047',
				amount: 2000,
				percentage: 40,
			}),
			buildCategoryExpense({
				categoryId: 'aluguel',
				categoryName: 'Aluguel',
				color: '#8E24AA',
				amount: 1500,
				percentage: 30,
			}),
			buildCategoryExpense({ categoryId: 'lazer', categoryName: 'Lazer', color: '#FB8C00', amount: 600, percentage: 12 }),
			buildCategoryExpense({ categoryId: 'saude', categoryName: 'Saúde', color: '#E53935', amount: 400, percentage: 8 }),
			buildCategoryExpense({
				categoryId: 'transporte',
				categoryName: 'Transporte',
				color: '#1E88E5',
				amount: 200,
				percentage: 4,
			}),
			buildCategoryExpense({
				categoryId: 'educacao',
				categoryName: 'Educação',
				color: '#3949AB',
				amount: 150,
				percentage: 3,
			}),
			buildCategoryExpense({ categoryId: 'pets', categoryName: 'Pets', color: '#6D4C41', amount: 100, percentage: 2 }),
			buildCategoryExpense({ categoryId: null, categoryName: 'Outras', color: null, amount: 50, percentage: 1 }),
		];

		it('should keep the four largest and add up the rest as "Outras", in the neutral color', async () => {
			expenses.set({ status: 'resolved', value: buildExpensesByCategory({ total: 5000, items: categories }) });
			await fixture.whenStable();

			const data = chart()!.data()!;
			const label = chart()!.options()!.plugins!.tooltip!.callbacks!.label!;
			expect(data.labels).toEqual(['Mercado (40%)', 'Aluguel (30%)', 'Lazer (12%)', 'Saúde (8%)', 'Outras (10%)']);
			expect(data.datasets[0].data).toEqual([2000, 1500, 600, 400, 500]);
			expect(data.datasets[0].backgroundColor).toEqual(['#43A047', '#8E24AA', '#FB8C00', '#E53935', 'gray']);
			expect(
				String(
					label.call({} as TooltipModel<'doughnut'>, { dataIndex: 4, label: 'Outras (10%)' } as TooltipItem<'doughnut'>),
				).replace(/\s/g, ' '),
			).toBe('Outras (10%): R$ 500,00');
		});

		it('should show five categories as they are', async () => {
			expenses.set({ status: 'resolved', value: buildExpensesByCategory({ total: 4700, items: categories.slice(0, 5) }) });
			await fixture.whenStable();

			expect(chart()!.data()!.labels).toEqual([
				'Mercado (40%)',
				'Aluguel (30%)',
				'Lazer (12%)',
				'Saúde (8%)',
				'Transporte (4%)',
			]);
		});
	});

	it("should name each slice with its share in the chart's legend, below it, in the text color", () => {
		const legend = chart()!.options()!.plugins!.legend!;

		expect(chart()!.data()!.labels).toEqual(['Mercado (56,73%)', 'Aluguel (38,46%)', 'Outras (4,81%)']);
		expect(legend.display).not.toBe(false);
		expect(legend.position).toBe('bottom');
		expect(legend.labels).toEqual(expect.objectContaining({ color: 'ink', usePointStyle: true }));
		expect(chart()!.options()!.maintainAspectRatio).toBe(false);
	});

	it('should describe the chart', () => {
		expect(element.querySelector('canvas')!.getAttribute('aria-label')?.replace(/\s/g, ' ')).toBe(
			'Despesas por categoria: Mercado, R$ 2.950,00 (56,73%); Aluguel, R$ 2.000,00 (38,46%); Outras, R$ 250,00 (4,81%).',
		);
	});

	it('should add the amount in BRL to the tooltip of a slice', () => {
		const label = chart()!.options()!.plugins!.tooltip!.callbacks!.label!;

		const tooltip = label.call(
			{} as TooltipModel<'doughnut'>,
			{ dataIndex: 1, label: 'Aluguel (38,46%)' } as TooltipItem<'doughnut'>,
		);

		expect(String(tooltip).replace(/\s/g, ' ')).toBe('Aluguel (38,46%): R$ 2.000,00');
	});

	it('should say when the month has no expenses', async () => {
		expenses.set({ status: 'resolved', value: buildExpensesByCategory({ total: 0, items: [] }) });
		await fixture.whenStable();

		expect(chart()).toBeUndefined();
		expect(text(element.querySelector('mat-card-content p'))).toBe('Sem despesas neste mês.');
	});

	it('should show a spinner instead of the chart while loading', async () => {
		expenses.set({ status: 'loading', value: undefined });
		await fixture.whenStable();

		expect(chart()).toBeUndefined();
		expect(element.querySelector('mat-progress-spinner')).not.toBeNull();
	});

	it('should say when the expenses fail to load, and retry them', async () => {
		const retry = vi.fn();
		fixture.componentInstance.retry.subscribe(retry);
		expenses.set({ status: 'error', error: new Error('Server Error') });
		await fixture.whenStable();

		expect(chart()).toBeUndefined();
		expect(text(element.querySelector('mat-card-content p'))).toBe(
			'Não foi possível carregar as despesas por categoria.',
		);
		Array.from(element.querySelectorAll('button'))
			.find((b) => b.textContent?.includes('Tentar novamente'))!
			.click();
		expect(retry).toHaveBeenCalledOnce();
	});
});
