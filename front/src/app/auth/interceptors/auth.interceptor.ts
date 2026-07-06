import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

const UNAUTHENTICATED_URLS = ['/api/auth/login', '/api/auth/register'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
	const authService = inject(AuthService);
	const router = inject(Router);

	const token = authService.token();
	const isAuthRequest = UNAUTHENTICATED_URLS.some((url) => req.url.includes(url));

	const authorizedReq = token && !isAuthRequest ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

	return next(authorizedReq).pipe(
		catchError((error: unknown) => {
			if (error instanceof HttpErrorResponse && error.status === 401) {
				authService.logout();
				router.navigateByUrl('/auth/login');
			}

			return throwError(() => error);
		}),
	);
};
