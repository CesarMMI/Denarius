import { Component, computed, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { formatSignedMoney } from '../utils/format-currency';

@Component({
	selector: 'app-transaction-row',
	templateUrl: './transaction-row.html',
	styleUrl: './transaction-row.scss',
	imports: [MatIconModule],
})
export class TransactionRow {
	readonly icon = input.required<string>();
	readonly iconColor = input('var(--mat-sys-on-surface-variant)');
	readonly iconBackground = input('var(--mat-sys-surface-container-high)');

	readonly description = input.required<string>();
	readonly descriptionMuted = input(false);

	readonly meta = input.required<string>();
	readonly metaDotColor = input<string | null>(null);

	readonly value = input.required<number>();
	readonly bordered = input(false);

	readonly rowClick = output<void>();

	protected readonly formattedValue = computed(() => formatSignedMoney(this.value()));
}
