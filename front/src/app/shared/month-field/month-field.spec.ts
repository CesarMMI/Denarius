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

	describe('calendar labels', () => {
		function calendarButton(selector: string) {
			return document.querySelector<HTMLButtonElement>(`.mat-datepicker-content ${selector}`)!;
		}

		function label(selector: string) {
			return calendarButton(selector).getAttribute('aria-label');
		}

		function periodDescription() {
			const id = calendarButton('.mat-calendar-period-button').getAttribute('aria-describedby')!;
			return document.getElementById(id)!.textContent?.trim();
		}

		async function switchView() {
			calendarButton('.mat-calendar-period-button').click();
			await fixture.whenStable();
		}

		it('should name the calendar toggle in Portuguese', () => {
			expect(element.querySelector('mat-datepicker-toggle button')!.getAttribute('aria-label')).toBe('Abrir calendário');
		});

		it('should name the calendar controls in Portuguese in every view', async () => {
			host.month.set(new Date(2026, 8, 1));
			await fixture.whenStable();
			datepicker().open();
			await fixture.whenStable();

			expect(label('.mat-calendar-period-button')).toBe('Escolher data');
			expect(label('.mat-calendar-previous-button')).toBe('Ano anterior');
			expect(label('.mat-calendar-next-button')).toBe('Próximo ano');
			expect(calendarButton('.mat-datepicker-close-button').textContent?.trim()).toBe('Fechar calendário');

			await switchView();
			expect(label('.mat-calendar-period-button')).toBe('Escolher mês e ano');
			expect(label('.mat-calendar-previous-button')).toBe('Mês anterior');
			expect(label('.mat-calendar-next-button')).toBe('Próximo mês');

			await switchView();
			expect(label('.mat-calendar-period-button')).toBe('Escolher data');
			expect(periodDescription()).toBe('de 2016 a 2039');
			expect(label('.mat-calendar-previous-button')).toBe('24 anos anteriores');
			expect(label('.mat-calendar-next-button')).toBe('Próximos 24 anos');

			datepicker().close();
			await fixture.whenStable();
		});
	});
});
