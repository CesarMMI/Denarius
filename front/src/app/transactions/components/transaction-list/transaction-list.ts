import { Component, input, output } from '@angular/core';
import { TransactionRow } from '../../../shared/transaction-row/transaction-row';
import { softBackground } from '../../../shared/utils/soft-color';

export interface TransactionListRow {
	id: string;
	icon: string;
	description: string;
	descriptionMuted: boolean;
	categoryName: string;
	categoryColor: string;
	value: number;
}

export interface TransactionListGroup {
	label: string;
	total: string;
	items: TransactionListRow[];
}

@Component({
	selector: 'app-transaction-list',
	templateUrl: './transaction-list.html',
	styleUrl: './transaction-list.scss',
	imports: [TransactionRow],
})
export class TransactionList {
	readonly groups = input.required<TransactionListGroup[]>();
	readonly openTransaction = output<string>();

	protected readonly softBackground = softBackground;
}
