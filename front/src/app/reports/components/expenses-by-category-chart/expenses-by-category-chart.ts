import { Component, computed, inject, input, output, Resource } from '@angular/core';
import { ChartData, ChartOptions } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { money, percentage } from '../../chart-formats';
import { ChartThemeService } from '../../services/chart-theme.service';
import { CategoryExpense, ExpensesByCategory } from '../../types/report';
import { ReportCard } from '../report-card/report-card';

/** Where the month's money went: a doughnut in the categories' colors. */
@Component({
	selector: 'app-expenses-by-category-chart',
	templateUrl: './expenses-by-category-chart.html',
	styleUrl: './expenses-by-category-chart.scss',
	imports: [BaseChartDirective, ReportCard],
})
export class ExpensesByCategoryChart {
	private static readonly slices = 5;

	private readonly theme = inject(ChartThemeService);

	readonly expenses = input.required<Resource<ExpensesByCategory | undefined>>();
	readonly retry = output<void>();

	protected readonly loaded = computed(() => {
		const expenses = this.expenses();
		return expenses.hasValue() ? expenses.value() : undefined;
	});
	/**
	 * At most five slices: past five categories, the four largest and the rest added up as "Outras". The API sorts the
	 * categories largest first and folds its own "Outras" past eight, always last, so it joins the rest.
	 */
	protected readonly items = computed<CategoryExpense[]>(() => {
		const expenses = this.loaded();
		if (!expenses || expenses.items.length <= ExpensesByCategoryChart.slices) return expenses?.items ?? [];
		const shown = expenses.items.slice(0, ExpensesByCategoryChart.slices - 1);
		const amount = expenses.items.slice(shown.length).reduce((sum, item) => sum + item.amount, 0);
		const percentage = (amount * 100) / expenses.total;
		return [...shown, { categoryId: null, categoryName: 'Outras', color: null, amount, percentage }];
	});
	protected readonly empty = computed(() => this.loaded()?.items.length === 0);

	protected readonly data = computed<ChartData<'doughnut', number[], string>>(() => {
		const colors = this.theme.colors();
		return {
			// The legend shows each category's share; the tooltip adds the amount.
			labels: this.items().map((item) => `${item.categoryName} (${percentage(item.percentage)})`),
			datasets: [
				{
					data: this.items().map((item) => item.amount),
					// "Outras" has no color of its own, so it takes the neutral one.
					backgroundColor: this.items().map((item) => item.color ?? colors.neutral),
					// A gap in the card's color separates the slices.
					borderColor: colors.surface,
					borderWidth: 4,
					hoverOffset: 4,
				},
			],
		};
	});

	protected readonly options = computed<ChartOptions<'doughnut'>>(() => {
		const items = this.items();
		return {
			maintainAspectRatio: false,
			cutout: '62%',
			plugins: {
				legend: {
					position: 'bottom',
					labels: { color: this.theme.colors().text, usePointStyle: true, pointStyle: 'circle', boxWidth: 8 },
				},
				tooltip: {
					callbacks: {
						label: (item) => `${item.label}: ${money.format(items[item.dataIndex].amount)}`,
					},
				},
			},
		};
	});

	protected readonly description = computed(
		() =>
			`Despesas por categoria: ${this.items()
				.map((item) => `${item.categoryName}, ${money.format(item.amount)} (${percentage(item.percentage)})`)
				.join('; ')}.`,
	);
}
