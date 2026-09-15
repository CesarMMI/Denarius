import { Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TransactionRow } from '../../../shared/transaction-row/transaction-row';
import { softBackground } from '../../../shared/utils/soft-color';

export interface RecentTransactionRow {
	id: string;
	icon: string;
	description: string;
	descriptionMuted: boolean;
	meta: string;
	categoryColor: string;
	value: number;
}

@Component({
	selector: 'app-recent-transactions-card',
	templateUrl: './recent-transactions-card.html',
	styleUrl: './recent-transactions-card.scss',
	imports: [MatButtonModule, MatIconModule, TransactionRow],
})
export class RecentTransactionsCard {
	readonly periodLabel = input.required<string>();
	readonly transactions = input.required<RecentTransactionRow[]>();

	readonly viewAll = output<void>();
	readonly openTransaction = output<string>();

	protected readonly softBackground = softBackground;
}
