import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, input, model, output, Resource } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Category } from '../../../categories/types/category';
import { MatChipColor } from '../../../shared/mat-chip-color/mat-chip-color';
import { Transaction } from '../../types/transaction';

@Component({
	selector: 'app-transactions-table',
	templateUrl: './transactions-table.html',
	styleUrl: './transactions-table.scss',
	imports: [
		CurrencyPipe,
		DatePipe,
		MatButtonModule,
		MatChipsModule,
		MatIconModule,
		MatProgressSpinnerModule,
		MatSortModule,
		MatTableModule,
		MatTooltipModule,
		MatChipColor,
	],
})
export class TransactionsTable {
	readonly transactions = input.required<Resource<Transaction[] | undefined>>();
	/** Give the rows their names and colors. */
	readonly categories = input.required<Resource<Category[] | undefined>>();
	readonly sort = model<Sort>({ active: 'Date', direction: 'desc' });
	readonly edit = output<Transaction>();
	readonly delete = output<Transaction>();

	protected readonly columns = ['date', 'description', 'category', 'value', 'actions'];

	/** Waits for both lists. `value()` throws while a resource is in error, `hasValue()` does not. */
	protected readonly rows = computed(() => {
		const transactions = this.transactions();
		return transactions.hasValue() && this.categories().hasValue() ? transactions.value() : [];
	});
	protected readonly total = computed(() => this.rows().reduce((sum, t) => sum + t.value, 0));

	protected readonly categoryById = computed(() => {
		const categories = this.categories();
		return new Map((categories.hasValue() ? categories.value() : []).map((c) => [c.id, c]));
	});
}
