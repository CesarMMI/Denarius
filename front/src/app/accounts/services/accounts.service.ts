import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Account } from '../models/account.model';
import { CreateAccountRequest } from '../models/create-account-request.model';
import { UpdateAccountRequest } from '../models/update-account-request.model';

@Injectable({
	providedIn: 'root',
})
export class AccountsService {
	private readonly http = inject(HttpClient);
	private readonly baseUrl = `${environment.apiUrl}/api/accounts`;

	list(): Observable<Account[]> {
		return this.http.get<Account[]>(this.baseUrl);
	}

	get(id: string): Observable<Account> {
		return this.http.get<Account>(`${this.baseUrl}/${id}`);
	}

	create(request: CreateAccountRequest): Observable<Account> {
		return this.http.post<Account>(this.baseUrl, request);
	}

	update(id: string, request: UpdateAccountRequest): Observable<Account> {
		return this.http.put<Account>(`${this.baseUrl}/${id}`, request);
	}

	delete(id: string): Observable<void> {
		return this.http.delete<void>(`${this.baseUrl}/${id}`);
	}
}
