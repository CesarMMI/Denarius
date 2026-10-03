import { Component, model } from '@angular/core';
import { debounce, form, FormField } from '@angular/forms/signals';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MonthField } from '../../../shared/month-field/month-field';
import { CategoryFilters } from '../../types/category-filters';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
	selector: 'app-categories-filters',
	imports: [MatButtonModule, MatIconModule, MatInputModule, MatSelectModule, MonthField, FormField],
	templateUrl: './categories-filters.html',
	styleUrl: './categories-filters.scss',
})
export class CategoriesFilters {
	readonly filters = model.required<CategoryFilters>({});
	/** Every change reloads the list, so the name waits for a pause in the typing or for leaving the field. */
	protected readonly form = form(this.filters, (path) => debounce(path.name!, 300));

	protected clearName(event: Event) {
		event.stopPropagation();
		this.filters.update((filters) => ({ ...filters, name: '' }));
	}

	protected clearWithTransaction(event: Event) {
		event.stopPropagation();
		this.filters.update((filters) => ({ ...filters, withTransaction: '' }));
	}
}
