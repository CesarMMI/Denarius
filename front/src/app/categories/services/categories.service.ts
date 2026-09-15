import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { SortValue } from '../../shared/types/sort';
import { Category } from '../types/category';
import { CategoryFilters } from '../types/category-filters';
import { CategorySortField } from '../types/category-sort';

@Injectable({
	providedIn: 'root',
})
export class CategoriesService {
	private readonly httpClient = inject(HttpClient);
	private readonly baseUrl = `${environment.apiUrl}/Categories`;

	list(filter: CategoryFilters, sort: SortValue<CategorySortField>) {
		return { url: this.baseUrl, params: this.toParams(filter, sort) };
	}

	getById(id: string) {
		return this.httpClient.get<Category>(`${this.baseUrl}/${id}`);
	}

	create(category: Pick<Category, 'name' | 'color'>) {
		return this.httpClient.post<Category>(`${this.baseUrl}`, category);
	}

	update(id: string, category: Pick<Category, 'name' | 'color'>) {
		return this.httpClient.put<Category>(`${this.baseUrl}/${id}`, category);
	}

	delete(id: string) {
		return this.httpClient.delete<void>(`${this.baseUrl}/${id}`);
	}

	private toParams(filter: CategoryFilters, sort: SortValue<CategorySortField>) {
		let params = new HttpParams();
		if (filter.name) params = params.set('name', filter.name);
		if (typeof filter.withTransaction === 'boolean') params = params.set('withTransaction', filter.withTransaction);
		if (sort.orderBy) params = params.set('orderBy', sort.orderBy);
		if (sort.ascending !== undefined) params = params.set('asc', sort.ascending);
		return params;
	}
}
