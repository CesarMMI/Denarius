import { Component, computed, inject, input, output, Resource } from '@angular/core';
import { ChartData, ChartOptions } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { compactMoney, money, monthName } from '../../chart-formats';
import { ChartThemeService } from '../../services/chart-theme.service';
import { AccumulatedExpense, CumulativeExpenseComparison } from '../../types/report';
import { ReportCard } from '../report-card/report-card';

/** The month's spending pace: its expenses added up day by day, against the month before. */
@Component({
	selector: 'app-cumulative-comparison-chart',
	templateUrl: './cumulative-comparison-chart.html',
	styleUrl: './cumulative-comparison-chart.scss',
	imports: [BaseChartDirective, ReportCard],
})
export class CumulativeComparisonChart {
	/** Thin lines; a day's points only show under the pointer. */
	private static readonly lines = {
		borderWidth: 2,
		pointRadius: 0,
		pointHoverRadius: 4,
		pointHitRadius: 12,
		tension: 0.2,
	} as const;

	private readonly theme = inject(ChartThemeService);

	readonly comparison = input.required<Resource<CumulativeExpenseComparison | undefined>>();
	/** The month compared, which names the lines. */
	readonly month = input.required<Date | null>();
	readonly retry = output<void>();

	protected readonly loaded = computed(() => {
		const comparison = this.comparison();
		return comparison.hasValue() ? comparison.value() : undefined;
	});
	protected readonly empty = computed(() => {
		const comparison = this.loaded();
		return !!comparison && [...comparison.currentMonth, ...comparison.previousMonth].every((day) => !day.accumulated);
	});
	private readonly monthNames = computed(() => {
		const month = this.month();
		return month
			? { current: monthName(month), previous: monthName(new Date(month.getFullYear(), month.getMonth() - 1, 1)) }
			: { current: 'Este mês', previous: 'Mês anterior' };
	});

	protected readonly data = computed<ChartData<'line', (number | null)[], number>>(() => {
		const comparison = this.loaded();
		const colors = this.theme.colors();
		const days = Array.from(
			{ length: Math.max(comparison?.daysInCurrentMonth ?? 0, comparison?.daysInPreviousMonth ?? 0) },
			(_, index) => index + 1,
		);
		// A day the month doesn't have, or hasn't reached, has no value: the line stops there.
		const line = (series: AccumulatedExpense[] = []) =>
			days.map((day) => series.find((point) => point.day === day)?.accumulated ?? null);
		return {
			labels: days,
			datasets: [
				{
					label: this.monthNames().current,
					data: line(comparison?.currentMonth),
					borderColor: colors.primary,
					backgroundColor: colors.primary,
					...CumulativeComparisonChart.lines,
				},
				{
					label: this.monthNames().previous,
					data: line(comparison?.previousMonth),
					borderColor: colors.primaryVariant,
					backgroundColor: colors.primaryVariant,
					...CumulativeComparisonChart.lines,
				},
			],
		};
	});

	protected readonly options = computed<ChartOptions<'line'>>(() => {
		const colors = this.theme.colors();
		return {
			maintainAspectRatio: false,
			// One tooltip per day, with both months.
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
					labels: { color: colors.text, usePointStyle: true, pointStyle: 'line' },
				},
				tooltip: {
					filter: (item) => item.parsed.y !== null,
					callbacks: {
						title: (items) => `Dia ${items[0].label}`,
						label: (item) => `${item.dataset.label}: ${money.format(item.parsed.y ?? 0)}`,
					},
				},
			},
		};
	});

	protected readonly description = computed(() => {
		const comparison = this.loaded();
		const last = (series: AccumulatedExpense[] = []) => series[series.length - 1];
		const current = last(comparison?.currentMonth);
		const previous = last(comparison?.previousMonth);
		const names = this.monthNames();
		return (
			`Despesas acumuladas: ${names.current}, ${money.format(current?.accumulated ?? 0)} até o dia ${current?.day ?? 0}; ` +
			`${names.previous}, ${money.format(previous?.accumulated ?? 0)} até o dia ${previous?.day ?? 0}.`
		);
	});
}
