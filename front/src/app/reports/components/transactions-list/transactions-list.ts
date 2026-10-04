import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, input, output, Resource } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { DateUtils } from '../../../shared/date-utils/date-utils';
import { MonthlyTransaction } from '../../types/report';
import { ReportCard } from '../report-card/report-card';

/** The month's latest transactions, newest first, scrolling inside its card, with a way to all of them. */
@Component({
	selector: 'app-transactions-list',
	templateUrl: './transactions-list.html',
	styleUrl: './transactions-list.scss',
	imports: [CurrencyPipe, DatePipe, MatButtonModule, MatTableModule, RouterLink, ReportCard],
})
export class TransactionsList {
	readonly transactions = input.required<Resource<MonthlyTransaction[] | undefined>>();
	/** The month listed, which "Ver todas" filters the transactions page by. */
	readonly month = input.required<Date | null>();
	readonly retry = output<void>();

	protected readonly columns = ['date', 'description', 'amount'];

	protected readonly rows = computed(() => {
		const transactions = this.transactions();
		return transactions.hasValue() ? transactions.value() : [];
	});
	/** The API lists the month newest first; the card shows the ten most recent and counts them all. */
	protected readonly latest = computed(() => this.rows().slice(0, 10));
	protected readonly empty = computed(() => this.transactions().hasValue() && this.rows().length === 0);
	protected readonly heading = computed(() => `Transações do mês (${this.rows().length})`);
	/** The query string that opens the transactions page on the month. */
	protected readonly monthFilter = computed(() => {
		const month = this.month();
		return month ? { month: DateUtils.toMonthKey(month) } : {};
	});

	/** Money out carries its minus sign, as on the transactions page. */
	protected signedAmount(transaction: MonthlyTransaction) {
		return transaction.type === 'out' ? -transaction.amount : transaction.amount;
	}
}
