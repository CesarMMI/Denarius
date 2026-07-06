import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest } from '../models/login-request.model';
import { LoginResponse } from '../models/login-response.model';
import { RegisterRequest } from '../models/register-request.model';
import { User } from '../models/user.model';

const TOKEN_KEY = 'auth_token';
const USER_KEY = 'auth_user';

@Injectable({
	providedIn: 'root',
})
export class AuthService {
	private readonly http = inject(HttpClient);
	private readonly baseUrl = `${environment.apiUrl}/api/auth`;

	private readonly currentUserSignal = signal<User | null>(this.readStoredUser());
	readonly currentUser = this.currentUserSignal.asReadonly();
	readonly isAuthenticated = computed(() => this.currentUserSignal() !== null);

	login(request: LoginRequest): Observable<LoginResponse> {
		return this.http.post<LoginResponse>(`${this.baseUrl}/login`, request).pipe(
			tap((response) => {
				localStorage.setItem(TOKEN_KEY, response.token);
				localStorage.setItem(USER_KEY, JSON.stringify(response.user));
				this.currentUserSignal.set(response.user);
			}),
		);
	}

	register(request: RegisterRequest): Observable<User> {
		return this.http.post<User>(`${this.baseUrl}/register`, request);
	}

	logout(): void {
		localStorage.removeItem(TOKEN_KEY);
		localStorage.removeItem(USER_KEY);
		this.currentUserSignal.set(null);
	}

	token(): string | null {
		return localStorage.getItem(TOKEN_KEY);
	}

	private readStoredUser(): User | null {
		const stored = localStorage.getItem(USER_KEY);
		if (!stored) return null;

		try {
			return JSON.parse(stored) as User;
		} catch {
			return null;
		}
	}
}
