import { CurrencyPipe } from '@angular/common';
import { Component, computed, input, model, output, Resource } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { MatChipColor } from '../../../shared/mat-chip-color/mat-chip-color';
import { Category } from '../../types/category';

@Component({
	selector: 'app-categories-table',
	templateUrl: './categories-table.html',
	styleUrl: './categories-table.scss',
	imports: [
		CurrencyPipe,
		RouterLink,
		MatButtonModule,
		MatChipsModule,
		MatIconModule,
		MatSortModule,
		MatProgressSpinnerModule,
		MatTableModule,
		MatTooltipModule,
		MatChipColor,
	],
})
export class CategoriesTable {
	readonly categories = input.required<Resource<Category[] | undefined>>();
	readonly sort = model<Sort>({ active: 'Name', direction: 'asc' });
	readonly edit = output<Category>();
	readonly delete = output<Category>();

	protected readonly columns = ['name', 'transactionCount', 'balance', 'actions'];

	/** `value()` throws while the resource is in error, `hasValue()` does not. */
	protected readonly rows = computed(() => {
		const categories = this.categories();
		return categories.hasValue() ? categories.value() : [];
	});
}
