import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, LOCALE_ID, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideNativeDateAdapter } from '@angular/material/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { delayInterceptor } from './delay-interceptor';

export const appConfig: ApplicationConfig = {
	providers: [
		provideBrowserGlobalErrorListeners(),
		provideHttpClient(withInterceptors([delayInterceptor])),
		provideRouter(routes),
		provideNativeDateAdapter(),
		{ provide: LOCALE_ID, useValue: 'pt-BR' },
	],
};
