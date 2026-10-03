import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, input, output, Resource, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Category } from '../../../categories/types/category';
import { MatChipColor } from '../../../shared/mat-chip-color/mat-chip-color';
import { Transaction } from '../../types/transaction';

/** The row that opens a day, with its date and total. */
interface DayRow {
	key: string;
	date: string;
	total: number;
	collapsed: boolean;
}

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
		MatTableModule,
		MatTooltipModule,
		MatChipColor,
	],
})
export class TransactionsTable {
	readonly transactions = input.required<Resource<Transaction[] | undefined>>();
	/** Give the rows their names and colors. */
	readonly categories = input.required<Resource<Category[] | undefined>>();
	readonly edit = output<Transaction>();
	readonly delete = output<Transaction>();

	/** With every day collapsed there is no transaction to act on, so the values close the row. */
	protected readonly columns = computed(() => [
		'toggle',
		'description',
		'category',
		'value',
		...(this.allCollapsed() ? [] : ['actions']),
	]);
	protected readonly dayColumns = computed(() => [
		'dayToggle',
		'dayDate',
		'dayTotal',
		...(this.allCollapsed() ? [] : ['dayActions']),
	]);
	protected readonly isDay = (_: number, row: Transaction | DayRow) => !('id' in row);
	/** The day rows are rebuilt on every toggle, so the table only reuses them when tracked by key. */
	protected readonly trackRow = (_: number, row: Transaction | DayRow) => ('id' in row ? row.id : row.key);

	/** Waits for both lists. `value()` throws while a resource is in error, `hasValue()` does not. */
	protected readonly rows = computed(() => {
		const transactions = this.transactions();
		return transactions.hasValue() && this.categories().hasValue() ? transactions.value() : [];
	});

	/** The `YYYY-MM-DD` keys of the days whose transactions are hidden. */
	private readonly collapsedDays = signal(new Set<string>());

	/** The API sorts by day first, so each day's transactions come together and a day row opens them. */
	protected readonly groupedRows = computed(() => {
		const collapsed = this.collapsedDays();
		const grouped: (Transaction | DayRow)[] = [];
		let day: DayRow | undefined;
		for (const transaction of this.rows()) {
			const key = transaction.date.slice(0, 10);
			if (day?.key !== key) {
				day = { key, date: transaction.date, total: 0, collapsed: collapsed.has(key) };
				grouped.push(day);
			}
			day.total += transaction.value;
			if (!day.collapsed) grouped.push(transaction);
		}
		return grouped;
	});

	private readonly days = computed(() => new Set(this.rows().map((t) => t.date.slice(0, 10))));
	protected readonly allCollapsed = computed(() => {
		const days = this.days();
		return days.size > 0 && [...days].every((day) => this.collapsedDays().has(day));
	});

	protected toggleAll() {
		this.collapsedDays.set(this.allCollapsed() ? new Set() : new Set(this.days()));
	}

	protected toggle(day: DayRow) {
		this.collapsedDays.update((days) => {
			const next = new Set(days);
			if (!next.delete(day.key)) next.add(day.key);
			return next;
		});
	}

	protected readonly categoryById = computed(() => {
		const categories = this.categories();
		return new Map((categories.hasValue() ? categories.value() : []).map((c) => [c.id, c]));
	});
}
