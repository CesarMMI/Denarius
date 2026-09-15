import { Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
	selector: 'app-page-header',
	templateUrl: './page-header.html',
	styleUrl: './page-header.scss',
	imports: [MatButtonModule, MatIconModule],
})
export class PageHeader {
	readonly greeting = input.required<string>();
	readonly title = input.required<string>();
	readonly actionLabel = input.required<string>();

	readonly showSearchIcon = input(false);
	readonly showFilterButton = input(false);
	readonly filtersActive = input(false);

	readonly monthLabel = input.required<string>();
	readonly monthTag = input('');

	readonly chips = input<string[]>([]);

	readonly action = output<void>();
	readonly openFilters = output<void>();
	readonly openMonth = output<void>();
	readonly removeChip = output<number>();
}
