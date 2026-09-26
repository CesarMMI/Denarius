import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { MonthRef, monthRefToDateRef } from '../../shared/types/month-ref';
import { SortValue } from '../../shared/types/sort';
import { Transaction } from '../types/transaction';
import { TRANSACTION_TYPE, TransactionFilters } from '../types/transaction-filters';
import { TransactionInput } from '../types/transaction-form-result';
import { TransactionSortField } from '../types/transaction-sort';

@Injectable({
	providedIn: 'root',
})
export class TransactionsService {
	private readonly httpClient = inject(HttpClient);
	private readonly baseUrl = `${environment.apiUrl}/Transactions`;

	list(filter: TransactionFilters, sort: SortValue<TransactionSortField>, monthRef: MonthRef | null = null) {
		return { url: this.baseUrl, params: this.toParams(filter, sort, monthRef) };
	}

	getById(id: string) {
		return this.httpClient.get<Transaction>(`${this.baseUrl}/${id}`);
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

	private toParams(filter: TransactionFilters, sort: SortValue<TransactionSortField>, monthRef: MonthRef | null) {
		let params = new HttpParams();
		if (filter.description) params = params.set('description', filter.description);
		if (filter.type !== TRANSACTION_TYPE.All) params = params.set('type', filter.type);
		if (filter.categoryId !== 'all') params = params.set('categoryId', filter.categoryId);
		if (monthRef) params = params.set('dateRef', monthRefToDateRef(monthRef));
		if (sort.orderBy) params = params.set('orderBy', sort.orderBy);
		if (sort.ascending !== undefined) params = params.set('asc', sort.ascending);
		return params;
	}
}
