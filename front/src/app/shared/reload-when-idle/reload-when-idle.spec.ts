import { HttpResourceRef, httpResource, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { reloadWhenIdle } from './reload-when-idle';

describe('reloadWhenIdle', () => {
	const url = '/items';

	let httpTesting: HttpTestingController;
	let items: HttpResourceRef<string[] | undefined>;
	let reload: () => void;

	beforeEach(() => {
		TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
		httpTesting = TestBed.inject(HttpTestingController);
		TestBed.runInInjectionContext(() => {
			items = httpResource<string[]>(() => url);
			reload = reloadWhenIdle(items);
		});
	});

	afterEach(() => httpTesting.verify());

	/**
	 * The resource takes a response in a later task, then the effects run. `whenStable()` would wait on the request the
	 * reload may send, so the test waits for a task instead.
	 */
	async function settle() {
		await new Promise((resolve) => setTimeout(resolve));
		TestBed.tick();
	}

	async function expectGet(): Promise<TestRequest> {
		await settle();
		return httpTesting.expectOne(url);
	}

	async function expectNoGet() {
		await settle();
		httpTesting.expectNone(url);
	}

	it('should reload at once when the list is not loading', async () => {
		(await expectGet()).flush(['a']);
		await settle();

		reload();

		(await expectGet()).flush(['b']);
		await settle();
		expect(items.value()).toEqual(['b']);
		await expectNoGet();
	});

	it('should reload once the load in progress succeeds, instead of dropping the reload', async () => {
		const first = await expectGet();

		reload();
		await expectNoGet();

		first.flush(['old']);
		(await expectGet()).flush(['new']);
		await settle();
		expect(items.value()).toEqual(['new']);
	});

	it('should send a single request for several reloads during the same load', async () => {
		const first = await expectGet();

		reload();
		reload();
		reload();

		first.flush(['old']);
		(await expectGet()).flush(['new']);
		await expectNoGet();
	});

	it('should not reload again after the pending reload is answered', async () => {
		const first = await expectGet();
		reload();
		first.flush(['old']);

		(await expectGet()).flush(['new']);

		await expectNoGet();
		await expectNoGet();
	});

	it('should drop the pending reload when the load in progress fails, and reload at once afterwards', async () => {
		const first = await expectGet();
		reload();

		first.flush(null, { status: 500, statusText: 'Server Error' });
		await expectNoGet();
		expect(items.error()).toBeTruthy();

		reload();
		(await expectGet()).flush(['a']);
		await settle();
		expect(items.value()).toEqual(['a']);
		await expectNoGet();
	});
});
