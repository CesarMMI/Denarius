import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { routes } from './app.routes';
// Loads the page and its Material dependencies at import time, as the other specs do,
// so the test only times the navigation.
import './transactions/pages/transactions-page/transactions-page';

describe('routes', () => {
	beforeEach(() => {
		TestBed.configureTestingModule({ providers: [provideRouter(routes)] });
	});

	it('should open the transactions page at the root', async () => {
		const router = TestBed.inject(Router);

		expect(await router.navigateByUrl('')).toBe(true);

		expect(router.url).toBe('/transactions');
	});
});
