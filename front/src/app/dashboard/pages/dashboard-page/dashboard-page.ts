import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { MatChipColor } from '../../../shared/mat-chip-color/mat-chip-color';
import { MOCK_CATEGORY_BY_ID, MOCK_TRANSACTIONS } from '../../../shared/mock-data/mock-data';
import { MonthField } from '../../../shared/month-field/month-field';
import { PageHeader } from '../../../shared/page-header/page-header';
import { toDateKey } from '../../../shared/utils/api-date';

@Component({
	selector: 'app-dashboard-page',
	templateUrl: './dashboard-page.html',
	styleUrl: './dashboard-page.scss',
	imports: [
		CurrencyPipe,
		DatePipe,
		RouterLink,
		MatButtonModule,
		MatCardModule,
		MatChipsModule,
		MatTableModule,
		MatChipColor,
		MonthField,
		PageHeader,
	],
})
export class DashboardPage {
	protected readonly columns = ['date', 'description', 'category', 'value'];
	protected readonly categoryById = MOCK_CATEGORY_BY_ID;

	/** Starts on the month of the mock data. */
	protected readonly month = signal<Date | null>(new Date(2026, 8, 1));

	private readonly transactions = computed(() => {
		const month = this.month();
		const prefix = month ? toDateKey(month).slice(0, 7) : '';
		return MOCK_TRANSACTIONS.filter((t) => t.date.startsWith(prefix));
	});

	protected readonly totalIn = computed(() =>
		this.transactions().reduce((sum, t) => (t.value > 0 ? sum + t.value : sum), 0),
	);
	protected readonly totalOut = computed(() =>
		this.transactions().reduce((sum, t) => (t.value < 0 ? sum + t.value : sum), 0),
	);
	protected readonly balance = computed(() => this.totalIn() + this.totalOut());

	protected readonly recent = computed(() =>
		[...this.transactions()].sort((a, b) => b.date.localeCompare(a.date)).slice(0, 5),
	);
}
