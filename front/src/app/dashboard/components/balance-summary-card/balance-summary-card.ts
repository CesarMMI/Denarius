import { Component, computed, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { formatMoney } from '../../../shared/utils/format-currency';

@Component({
	selector: 'app-balance-summary-card',
	templateUrl: './balance-summary-card.html',
	styleUrl: './balance-summary-card.scss',
	imports: [MatIconModule],
})
export class BalanceSummaryCard {
	readonly periodLabel = input.required<string>();
	readonly net = input.required<number>();
	readonly totalIn = input.required<number>();
	readonly totalOut = input.required<number>();

	protected readonly formattedNet = computed(() => formatMoney(this.net()));
	protected readonly formattedIn = computed(() => formatMoney(this.totalIn()));
	protected readonly formattedOut = computed(() => formatMoney(this.totalOut()));
}
