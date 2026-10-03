import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { App } from './app';

@Component({ template: '' })
class Blank {}

describe('AppComponent', () => {
	let fixture: ComponentFixture<App>;

	beforeEach(async () => {
		TestBed.configureTestingModule({
			providers: [
				provideRouter([
					{ path: 'dashboard', component: Blank },
					{ path: 'transactions', component: Blank },
					{ path: 'categories', component: Blank },
				]),
			],
		});
		fixture = TestBed.createComponent(App);
		await fixture.whenStable();
	});

	function links() {
		return Array.from(fixture.nativeElement.querySelectorAll('mat-nav-list a') as NodeListOf<HTMLAnchorElement>);
	}

	it('should link every page from the side menu', () => {
		expect(links().map((a) => a.getAttribute('href'))).toEqual(['/dashboard', '/transactions', '/categories']);
		expect(links().map((a) => a.textContent)).toEqual([
			expect.stringContaining('Resumo'),
			expect.stringContaining('Transações'),
			expect.stringContaining('Categorias'),
		]);
	});

	it('should mark the link of the current page', async () => {
		await TestBed.inject(Router).navigateByUrl('/transactions');
		await fixture.whenStable();

		const current = links().filter((a) => a.getAttribute('aria-current') === 'page');
		expect(current.map((a) => a.getAttribute('href'))).toEqual(['/transactions']);
	});
});
