import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { providePrimeNG } from 'primeng/config';
import { authInterceptor } from './auth/interceptors/auth.interceptor';
import { routes } from './app.routes';
import { primengTheme } from './primeng-theme';

export const appConfig: ApplicationConfig = {
	providers: [
		provideBrowserGlobalErrorListeners(),
		provideRouter(routes),
		provideHttpClient(withInterceptors([authInterceptor])),
		providePrimeNG({ theme: primengTheme, ripple: true }),
	],
};
