import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ColorsService } from './colors.service';

const URL = 'data/default-colors.json';
const PALETTE = ['#F4511E', '#8E24AA', '#1E88E5'];

describe('ColorsService', () => {
	let service: ColorsService;
	let httpTesting: HttpTestingController;

	beforeEach(() => {
		TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
		service = TestBed.inject(ColorsService);
		httpTesting = TestBed.inject(HttpTestingController);
		TestBed.tick();
	});

	afterEach(() => httpTesting.verify());

	async function respond(body: string[] | 'error') {
		const req = httpTesting.expectOne(URL);
		if (body === 'error') req.flush('', { status: 404, statusText: 'Not Found' });
		else req.flush(body);
		await TestBed.inject(ApplicationRef).whenStable();
	}

	it('should have no colors while the palette loads', () => {
		httpTesting.expectOne(URL);

		expect(service.resource.isLoading()).toBe(true);
		expect(service.colors()).toEqual([]);
	});

	it('should expose the colors of the palette', async () => {
		await respond(PALETTE);

		expect(service.colors()).toEqual(PALETTE);
	});

	it('should have no colors when the palette fails to load', async () => {
		await respond('error');

		expect(service.resource.error()).toBeTruthy();
		expect(service.colors()).toEqual([]);
	});

	it('should fetch the palette again on reload', async () => {
		await respond('error');

		service.reload();
		TestBed.tick();
		await respond(PALETTE);

		expect(service.colors()).toEqual(PALETTE);
	});
});
