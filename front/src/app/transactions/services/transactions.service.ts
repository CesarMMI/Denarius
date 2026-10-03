import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Sort } from '@angular/material/sort';
import { environment } from '../../../environments/environment';
import { DateUtils } from '../../shared/date-utils/date-utils';
import { Transaction, TransactionInput } from '../types/transaction';
import { TransactionFilters } from '../types/transaction-filters';

@Injectable({
	providedIn: 'root',
})
export class TransactionsService {
	private readonly httpClient = inject(HttpClient);
	private readonly baseUrl = `${environment.apiUrl}/transactions`;

	/** A request for an `httpResource`, sorted by the API field in `sort.active`. */
	list(filters: TransactionFilters = {}, sort?: Sort) {
		let params = new HttpParams();
		if (filters.description) params = params.set('description', filters.description);
		if (filters.type) params = params.set('type', filters.type);
		if (filters.categoryId) params = params.set('categoryId', filters.categoryId);
		if (filters.month) params = params.set('dateRef', DateUtils.toDateKey(filters.month));
		if (sort?.direction) params = params.set('orderBy', sort.active).set('asc', sort.direction === 'asc');
		return { url: this.baseUrl, params };
	}

	create(transaction: TransactionInput) {
		return this.httpClient.post<Transaction>(this.baseUrl, transaction);
	}

	update(id: string, transaction: TransactionInput) {
		return this.httpClient.put<Transaction>(`${this.baseUrl}/${id}`, transaction);
	}

	delete(id: string) {
		return this.httpClient.delete<void>(`${this.baseUrl}/${id}`);
	}
}
