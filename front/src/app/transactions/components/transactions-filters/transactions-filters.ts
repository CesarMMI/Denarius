import { Component, input, model } from '@angular/core';
import { debounce, form, FormField } from '@angular/forms/signals';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Category } from '../../../categories/types/category';
import { MonthField } from '../../../shared/month-field/month-field';
import { TransactionFilters } from '../../types/transaction-filters';

@Component({
	selector: 'app-transactions-filters',
	templateUrl: './transactions-filters.html',
	styleUrl: './transactions-filters.scss',
	imports: [MatButtonModule, MatIconModule, MatInputModule, MatSelectModule, MonthField, FormField],
})
export class TransactionsFilters {
	readonly filters = model.required<TransactionFilters>();
	readonly categories = input<Category[]>([]);
	/** Every change reloads the list, so the description waits for a pause in the typing or for leaving the field. */
	protected readonly form = form(this.filters, (path) => debounce(path.description!, 300));

	/** Empties a filter, which then keeps every transaction. The click would also open the select around the button. */
	protected clear(event: Event, filter: 'description' | 'type' | 'categoryId') {
		event.stopPropagation();
		this.filters.update((filters) => ({ ...filters, [filter]: '' }));
	}
}
