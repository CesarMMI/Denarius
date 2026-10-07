import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatTooltip } from '@angular/material/tooltip';
import { By } from '@angular/platform-browser';
import { Sort } from '@angular/material/sort';
import { SortMenu, SortOption } from './sort-menu';

@Component({
	imports: [SortMenu],
	template: `<app-sort-menu [options]="options" [(sort)]="sort" />`,
})
class Host {
	readonly options: SortOption[] = [
		{ active: 'name', direction: 'asc', label: 'Nome (A–Z)' },
		{ active: 'name', direction: 'desc', label: 'Nome (Z–A)' },
		{ active: 'balance', direction: 'desc', label: 'Maior saldo primeiro' },
	];
	readonly sort = signal<Sort>({ active: 'name', direction: 'asc' });
}

describe('SortMenu', () => {
	let fixture: ComponentFixture<Host>;
	let host: Host;
	let element: HTMLElement;

	beforeEach(async () => {
		fixture = TestBed.createComponent(Host);
		host = fixture.componentInstance;
		element = fixture.nativeElement;
		await fixture.whenStable();
	});

	function trigger() {
		return element.querySelector<HTMLButtonElement>('button')!;
	}

	function tooltip() {
		return fixture.debugElement.query(By.directive(MatTooltip)).injector.get(MatTooltip).message;
	}

	/** The menu opens in an overlay, outside the host. */
	function items() {
		return Array.from(document.querySelectorAll<HTMLButtonElement>('.mat-mdc-menu-panel [mat-menu-item]'));
	}

	async function open() {
		trigger().click();
		await fixture.whenStable();
	}

	it('should name the sort in use on its button', async () => {
		expect(trigger().getAttribute('aria-label')).toBe('Ordenar: Nome (A–Z)');

		host.sort.set({ active: 'balance', direction: 'desc' });
		await fixture.whenStable();

		expect(trigger().getAttribute('aria-label')).toBe('Ordenar: Maior saldo primeiro');
	});

	it('should show the sort in use in the tooltip of its button', async () => {
		expect(tooltip()).toBe('Ordenar: Nome (A–Z)');

		host.sort.set({ active: 'balance', direction: 'desc' });
		await fixture.whenStable();

		expect(tooltip()).toBe('Ordenar: Maior saldo primeiro');
		expect(tooltip()).toBe(trigger().getAttribute('aria-label'));
	});

	it('should list the options with the one in use checked', async () => {
		await open();

		expect(items().map((item) => item.textContent?.replace('check', '').trim())).toEqual([
			'Nome (A–Z)',
			'Nome (Z–A)',
			'Maior saldo primeiro',
		]);
		expect(items().map((item) => item.getAttribute('aria-checked'))).toEqual(['true', 'false', 'false']);
		expect(items().map((item) => item.getAttribute('role'))).toEqual(['menuitemradio', 'menuitemradio', 'menuitemradio']);
	});

	it('should take the option chosen and close', async () => {
		await open();
		expect(trigger().getAttribute('aria-expanded')).toBe('true');

		items()[2].click();
		await fixture.whenStable();

		expect(host.sort()).toEqual({ active: 'balance', direction: 'desc' });
		expect(trigger().getAttribute('aria-expanded')).toBe('false');
	});
});
