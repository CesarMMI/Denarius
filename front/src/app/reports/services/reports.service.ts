import { HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { DateUtils } from '../../shared/date-utils/date-utils';

/** Requests for `httpResource`s, one per report. Without a month, the API reports the current one. */
@Injectable({
	providedIn: 'root',
})
export class ReportsService {
	private readonly baseUrl = `${environment.apiUrl}/reports`;

	summary(month: Date | null) {
		return this.request('summary', month);
	}

	expensesByCategory(month: Date | null) {
		return this.request('expensesByCategory', month);
	}

	/** The `months` months ending in `month`; the API covers 12 when left out. */
	incomeVsExpense(month: Date | null, months?: number) {
		const { url, params } = this.request('incomeVsExpense', month);
		return { url, params: months ? params.set('months', months) : params };
	}

	cumulativeExpenses(month: Date | null) {
		return this.request('cumulativeExpenses', month);
	}

	transactions(month: Date | null) {
		return this.request('transactions', month);
	}

	private request(report: string, month: Date | null) {
		const params = month ? new HttpParams().set('month', DateUtils.toMonthKey(month)) : new HttpParams();
		return { url: `${this.baseUrl}/${report}`, params };
	}
}
