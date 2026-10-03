import { Component, computed, input, model } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { Sort } from '@angular/material/sort';
import { MatTooltipModule } from '@angular/material/tooltip';

/** A sort the menu offers, named by what it puts first. */
export interface SortOption extends Sort {
	label: string;
}

/** Picks the sort of a list from a menu, with the one in use checked. */
@Component({
	selector: 'app-sort-menu',
	templateUrl: './sort-menu.html',
	styleUrl: './sort-menu.scss',
	imports: [MatButtonModule, MatIconModule, MatMenuModule, MatTooltipModule],
})
export class SortMenu {
	readonly options = input.required<SortOption[]>();
	readonly sort = model.required<Sort>();

	protected readonly selected = computed(() => {
		const { active, direction } = this.sort();
		return this.options().find((option) => option.active === active && option.direction === direction);
	});
	protected readonly label = computed(() => `Ordenar: ${this.selected()?.label ?? ''}`);

	protected select({ active, direction }: SortOption) {
		this.sort.set({ active, direction });
	}
}
