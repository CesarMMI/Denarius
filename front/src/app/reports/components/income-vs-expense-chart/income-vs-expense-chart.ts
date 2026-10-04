import { Component, computed, inject, input, output, Resource } from '@angular/core';
import { ChartData, ChartOptions } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { compactMoney, money, monthName } from '../../chart-formats';
import { ChartThemeService } from '../../services/chart-theme.service';
import { IncomeVsExpense } from '../../types/report';
import { ReportCard } from '../report-card/report-card';

/** Income and expenses side by side, month by month, up to the selected month. */
@Component({
	selector: 'app-income-vs-expense-chart',
	templateUrl: './income-vs-expense-chart.html',
	styleUrl: './income-vs-expense-chart.scss',
	imports: [BaseChartDirective, ReportCard],
})
export class IncomeVsExpenseChart {
	/** Thin bars with rounded tops, square on the baseline. */
	private static readonly bars = { maxBarThickness: 24, borderRadius: 4, borderSkipped: 'start' } as const;

	private readonly theme = inject(ChartThemeService);

	readonly series = input.required<Resource<IncomeVsExpense[] | undefined>>();
	readonly retry = output<void>();

	protected readonly months = computed(() => {
		const series = this.series();
		return series.hasValue() ? series.value() : [];
	});
	/** Only a period without any movement is empty: a quiet month still has its place on the axis. */
	protected readonly empty = computed(
		() => this.series().hasValue() && this.months().every((month) => month.income === 0 && month.expense === 0),
	);

	protected readonly data = computed<ChartData<'bar', number[], string | string[]>>(() => {
		const colors = this.theme.colors();
		const months = this.months();
		return {
			// "nov.", with the year below the first month and every January, so the axis needs no tilt.
			labels: months.map(({ month }, index) =>
				index === 0 || month.endsWith('-01') ? [monthName(month, 'short'), month.slice(0, 4)] : monthName(month, 'short'),
			),
			datasets: [
				{
					label: 'Receitas',
					data: months.map((month) => month.income),
					backgroundColor: colors.income,
					...IncomeVsExpenseChart.bars,
				},
				{
					label: 'Despesas',
					data: months.map((month) => month.expense),
					backgroundColor: colors.expense,
					...IncomeVsExpenseChart.bars,
				},
			],
		};
	});

	protected readonly options = computed<ChartOptions<'bar'>>(() => {
		const colors = this.theme.colors();
		const months = this.months();
		return {
			maintainAspectRatio: false,
			// One tooltip per month, with both values.
			interaction: { mode: 'index', intersect: false },
			scales: {
				x: { grid: { display: false }, border: { color: colors.grid }, ticks: { color: colors.text, maxRotation: 0 } },
				y: {
					beginAtZero: true,
					grid: { color: colors.grid },
					border: { display: false },
					ticks: { color: colors.text, callback: (value) => compactMoney.format(Number(value)), count: 5 },
				},
			},
			plugins: {
				legend: {
					position: 'bottom',
					labels: { color: colors.text, usePointStyle: true, pointStyle: 'rectRounded', boxWidth: 8 },
				},
				tooltip: {
					callbacks: {
						title: (items) => monthName(months[items[0].dataIndex].month),
						label: (item) => `${item.dataset.label}: ${money.format(item.parsed.y ?? 0)}`,
					},
				},
			},
		};
	});

	protected readonly description = computed(
		() =>
			`Receitas e despesas por mês: ${this.months()
				.map(
					(month) =>
						`${monthName(month.month)}, receitas ${money.format(month.income)}, despesas ${money.format(month.expense)}`,
				)
				.join('; ')}.`,
	);
}
