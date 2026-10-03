import { Component, LOCALE_ID, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDatepicker } from '@angular/material/datepicker';
import { By } from '@angular/platform-browser';
import { MonthField } from './month-field';

@Component({
	imports: [MonthField],
	template: `<app-month-field [(value)]="month" [clearable]="clearable()" />`,
})
class Host {
	readonly month = signal<Date | null>(null);
	readonly clearable = signal(false);
}

describe('MonthField', () => {
	let fixture: ComponentFixture<Host>;
	let host: Host;
	let element: HTMLElement;

	beforeEach(async () => {
		TestBed.configureTestingModule({ providers: [{ provide: LOCALE_ID, useValue: 'pt-BR' }] });
		fixture = TestBed.createComponent(Host);
		host = fixture.componentInstance;
		element = fixture.nativeElement;
		await fixture.whenStable();
	});

	function input() {
		return element.querySelector('input')!;
	}

	function clearButton() {
		return element.querySelector<HTMLButtonElement>('button[aria-label="Limpar mês"]');
	}

	function datepicker(): MatDatepicker<Date> {
		return fixture.debugElement.query(By.directive(MatDatepicker)).componentInstance;
	}

	it('should show every month while empty', () => {
		expect(input().value).toBe('');
		expect(input().placeholder).toBe('Todos os meses');
	});

	it('should show the month and its year', async () => {
		host.month.set(new Date(2026, 8, 1));
		await fixture.whenStable();

		expect(input().value).toBe('09/2026');
	});

	it('should pick the month selected in the year view and close the picker', async () => {
		const close = vi.spyOn(datepicker(), 'close');

		datepicker().monthSelected.emit(new Date(2026, 2, 1));
		await fixture.whenStable();

		expect(host.month()).toEqual(new Date(2026, 2, 1));
		expect(input().value).toBe('03/2026');
		expect(close).toHaveBeenCalled();
	});

	it('should open the year view when the input is clicked', () => {
		const open = vi.spyOn(datepicker(), 'open').mockImplementation(() => undefined);

		input().click();

		expect(open).toHaveBeenCalled();
		expect(datepicker().startView).toBe('year');
	});

	it('should only offer to clear a month when clearable', async () => {
		host.month.set(new Date(2026, 8, 1));
		await fixture.whenStable();
		expect(clearButton()).toBeNull();

		host.clearable.set(true);
		await fixture.whenStable();
		clearButton()!.click();
		await fixture.whenStable();

		expect(host.month()).toBeNull();
		expect(clearButton()).toBeNull();
	});
});
