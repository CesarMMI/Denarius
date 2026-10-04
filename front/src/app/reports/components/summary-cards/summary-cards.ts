import { CurrencyPipe, DatePipe, NgTemplateOutlet, PercentPipe } from '@angular/common';
import { Component, computed, input, output, Resource } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MonthlySummary } from '../../types/report';
import { ReportCard } from '../report-card/report-card';

/** How a change from the previous month reads: its arrow, its size, and whether it is good news. */
interface Change {
	arrow: '↑' | '↓' | '';
	/** A fraction, for the percent pipe. */
	size: number;
	tone: 'good' | 'bad' | 'neutral';
}

/** The month at a glance: balance, income and expenses against the month before, savings rate and projection. */
@Component({
	selector: 'app-summary-cards',
	templateUrl: './summary-cards.html',
	styleUrl: './summary-cards.scss',
	imports: [CurrencyPipe, DatePipe, NgTemplateOutlet, PercentPipe, MatCardModule, ReportCard],
})
export class SummaryCards {
	readonly summary = input.required<Resource<MonthlySummary | undefined>>();
	readonly retry = output<void>();

	/** `value()` throws while the resource is in error, `hasValue()` does not. */
	protected readonly loaded = computed(() => {
		const summary = this.summary();
		return summary.hasValue() ? summary.value() : undefined;
	});
	protected readonly empty = computed(() => this.loaded()?.totalIncome === 0 && this.loaded()?.totalExpense === 0);

	protected readonly previousMonth = computed(() => {
		const [year, month] = (this.loaded()?.month ?? '').split('-').map(Number);
		return new Date(year, month - 2, 1);
	});

	/** Going up is good news for the balance and the income, and bad news for the expenses. */
	protected readonly changes = computed(() => {
		const previousMonth = this.loaded()?.previousMonth;
		return {
			balance: SummaryCards.change(previousMonth?.balanceChange ?? null, true),
			income: SummaryCards.change(previousMonth?.totalIncomeChange ?? null, true),
			expense: SummaryCards.change(previousMonth?.totalExpenseChange ?? null, false),
		};
	});

	/** A percentage on a 0–100 scale, or null when the previous month had nothing to compare with. */
	private static change(percentage: number | null, upIsGood: boolean): Change | null {
		if (percentage === null) return null;
		return {
			arrow: percentage > 0 ? '↑' : percentage < 0 ? '↓' : '',
			size: Math.abs(percentage) / 100,
			tone: percentage === 0 ? 'neutral' : percentage > 0 === upIsGood ? 'good' : 'bad',
		};
	}
}
