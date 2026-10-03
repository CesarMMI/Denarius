import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { LOCALE_ID } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { MatChipColor } from '../../../shared/mat-chip-color/mat-chip-color';
import { DashboardPage } from './dashboard-page';

registerLocaleData(localePt);

describe('DashboardPage', () => {
	let fixture: ComponentFixture<DashboardPage>;
	let element: HTMLElement;

	beforeEach(async () => {
		TestBed.configureTestingModule({ providers: [{ provide: LOCALE_ID, useValue: 'pt-BR' }, provideRouter([])] });
		fixture = TestBed.createComponent(DashboardPage);
		element = fixture.nativeElement;
		await fixture.whenStable();
	});

	function totals() {
		return Array.from(element.querySelectorAll('mat-card-title'))
			.slice(0, 3)
			.map((t) => t.textContent?.trim());
	}

	function column(name: string) {
		return Array.from(element.querySelectorAll(`tr[mat-row] .mat-column-${name}`)).map((cell) =>
			cell.textContent?.trim(),
		);
	}

	async function pickMonth(month: Date) {
		fixture.debugElement.query(By.css('app-month-field')).triggerEventHandler('valueChange', month);
		await fixture.whenStable();
	}

	it('should sum the balance, the incomes and the expenses of September 2026', () => {
		expect(totals()).toEqual([
			expect.stringContaining('5.684,83'),
			expect.stringContaining('9.350,00'),
			expect.stringContaining('3.665,17'),
		]);
	});

	it('should list the 5 most recent transactions of the month', () => {
		expect(column('date')).toEqual(['28/09/2026', '28/09/2026', '26/09/2026', '24/09/2026', '22/09/2026']);
		expect(column('category')).toEqual(['Mercado', 'Transporte', 'Transporte', 'Saúde', 'Freelance']);
		expect(fixture.debugElement.query(By.directive(MatChipColor)).injector.get(MatChipColor).color()).toBe('#43A047');
		expect(column('value')[0]).toContain('186,42');
	});

	it('should follow the month picked', async () => {
		await pickMonth(new Date(2026, 7, 1));

		expect(totals()).toEqual([
			expect.stringContaining('5.439,90'),
			expect.stringContaining('8.600,00'),
			expect.stringContaining('3.160,10'),
		]);
		expect(column('description')).toEqual([
			'Consulta oftalmologista',
			'Mercado do mês',
			'Aluguel agosto',
			'Salário agosto',
		]);
	});

	it('should say when the month has no transactions', async () => {
		await pickMonth(new Date(2026, 0, 1));

		expect(column('date')).toEqual([]);
		expect(element.querySelector('td[colspan]')?.textContent?.trim()).toBe('Nenhuma transação neste mês.');
	});

	it('should link to every transaction', () => {
		const link = Array.from(element.querySelectorAll('a')).find((a) => a.textContent?.trim() === 'Ver todas');

		expect(link?.getAttribute('href')).toBe('/transactions');
	});
});
