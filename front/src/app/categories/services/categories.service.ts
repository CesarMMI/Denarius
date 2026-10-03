import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Sort } from '@angular/material/sort';
import { environment } from '../../../environments/environment';
import { DateUtils } from '../../shared/date-utils/date-utils';
import { Category, CategoryInput } from '../types/category';
import { CategoryFilters } from '../types/category-filters';

@Injectable({
	providedIn: 'root',
})
export class CategoriesService {
	private readonly httpClient = inject(HttpClient);
	private readonly baseUrl = `${environment.apiUrl}/categories`;

	/** A request for an `httpResource`, sorted by the API field in `sort.active`. */
	list(filters: CategoryFilters = {}, sort?: Sort) {
		let params = new HttpParams();
		if (filters.name) params = params.set('name', filters.name);
		if (typeof filters.withTransaction === 'boolean') params = params.set('withTransaction', filters.withTransaction);
		if (filters.month) params = params.set('dateRef', DateUtils.toDateKey(filters.month));
		if (sort?.direction) params = params.set('orderBy', sort.active).set('asc', sort.direction === 'asc');
		return { url: this.baseUrl, params };
	}

	create(category: CategoryInput) {
		return this.httpClient.post<Category>(this.baseUrl, category);
	}

	update(id: string, category: CategoryInput) {
		return this.httpClient.put<Category>(`${this.baseUrl}/${id}`, category);
	}

	delete(id: string) {
		return this.httpClient.delete<void>(`${this.baseUrl}/${id}`);
	}
}
