import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { LOCALE_ID } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatButtonToggleGroup } from '@angular/material/button-toggle';
import { MatDatepicker } from '@angular/material/datepicker';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSelectHarness } from '@angular/material/select/testing';
import { By } from '@angular/platform-browser';
import { buildCategory } from '../../../categories/testing/category-fixture';
import { Category } from '../../../categories/types/category';
import { DateUtils } from '../../../shared/date-utils/date-utils';
import { buildTransaction } from '../../testing/transaction-fixture';
import { Transaction } from '../../types/transaction';
import { TransactionForm } from './transaction-form';

const MERCADO_ID = '3f2a1c4e-0000-4000-8000-0000000000a1';
const SALARIO_ID = '3f2a1c4e-0000-4000-8000-0000000000a2';
const PAGAMENTO_ID = '7b1d2e3f-0000-4000-8000-0000000000b2';

describe('TransactionForm', () => {
	const categories = [
		buildCategory({ id: MERCADO_ID, name: 'Mercado' }),
		buildCategory({ id: SALARIO_ID, name: 'Salário' }),
	];
	const feira = buildTransaction({
		id: PAGAMENTO_ID,
		description: 'Feira da semana',
		categoryId: SALARIO_ID,
		value: -186.42,
		date: '2026-09-24T00:00:00Z',
	});

	let fixture: ComponentFixture<TransactionForm>;
	let element: HTMLElement;
	let close: ReturnType<typeof vi.fn>;

	async function render(transaction: Transaction | undefined, options: Category[] = categories) {
		close = vi.fn();
		TestBed.configureTestingModule({
			providers: [
				{ provide: MatDialogRef, useValue: { close } },
				{ provide: MAT_DIALOG_DATA, useValue: { transaction, categories: options } },
			],
		});
		fixture = TestBed.createComponent(TransactionForm);
		element = fixture.nativeElement;
		await fixture.whenStable();
	}

	function input(name: string) {
		return element.querySelector<HTMLInputElement>(`input[formControlName="${name}"]`)!;
	}

	async function type(name: string, value: string) {
		input(name).value = value;
		input(name).dispatchEvent(new Event('input'));
		await fixture.whenStable();
	}

	function sign() {
		return fixture.debugElement.query(By.directive(MatButtonToggleGroup)).injector.get(MatButtonToggleGroup);
	}

	async function setSign(label: 'Entrada' | 'Saída') {
		const toggle = Array.from(element.querySelectorAll<HTMLButtonElement>('mat-button-toggle button')).find(
			(b) => b.textContent?.trim() === label,
		);
		toggle!.click();
		await fixture.whenStable();
	}

	function errors() {
		return Array.from(element.querySelectorAll('mat-error')).map((error) => error.textContent?.trim());
	}

	async function save() {
		element.querySelector('form')!.dispatchEvent(new Event('submit'));
		await fixture.whenStable();
	}

	describe('creating', () => {
		beforeEach(() => render(undefined));

		it('should start as an empty expense in the first category, dated today', () => {
			expect(element.querySelector('[mat-dialog-title]')?.textContent?.trim()).toBe('Nova transação');
			expect(input('value').value).toBe('');
			expect(sign().value).toBe('out');
			expect(element.querySelector('mat-select')?.textContent).toContain('Mercado');
		});

		it('should submit from the save button of the dialog actions', () => {
			const button = element.querySelector<HTMLButtonElement>('mat-dialog-actions button[type="submit"]')!;

			expect(button.getAttribute('form')).toBe(element.querySelector('form')!.id);
		});

		it('should not save without a valid value', async () => {
			await type('value', '12,345');
			await save();

			expect(close).not.toHaveBeenCalled();
			expect(element.querySelector('mat-error')?.textContent).toBe('Informe um valor válido');
		});

		it('should save an expense as a negative value with the day as UTC midnight', async () => {
			await type('value', '12,50');
			await type('description', '  Pão  ');
			await save();

			expect(close).toHaveBeenCalledWith({
				description: 'Pão',
				categoryId: MERCADO_ID,
				value: -12.5,
				date: DateUtils.toApiDate(new Date()),
			});
		});

		it('should save an income as a positive value and a blank description as null', async () => {
			await setSign('Entrada');
			await type('value', '8600');
			await type('description', '   ');
			await save();

			expect(close).toHaveBeenCalledWith(expect.objectContaining({ value: 8600, description: null }));
		});

		it('should close without a result when cancelled', () => {
			element.querySelector<HTMLButtonElement>('button[mat-dialog-close]')!.click();

			expect(close).toHaveBeenCalledOnce();
			expect(close.mock.calls[0][0]).toBeFalsy();
		});
	});

	describe('validating', () => {
		beforeEach(() => render(undefined));

		it('should not save without a date', async () => {
			await type('value', '12,50');
			await type('date', '');
			await save();

			expect(close).not.toHaveBeenCalled();
			expect(errors()).toEqual(['Informe uma data válida']);
		});

		it.each(['8.600,00', '-12'])('should not save the value %s', async (value) => {
			await type('value', value);
			await save();

			expect(close).not.toHaveBeenCalled();
			expect(errors()).toEqual(['Informe um valor válido']);
		});

		it.each(['99999999999999,99', '12345678901234,56', '10000000000000', '999999999999999,99', '00000000000001'])(
			'should not save a value with more than 13 integer digits: %s',
			async (value) => {
				await type('value', value);
				await save();

				expect(close).not.toHaveBeenCalled();
				expect(errors()).toEqual(['Informe um valor válido']);
			},
		);

		it('should save the largest value with 13 integer digits exactly', async () => {
			await type('value', '9999999999999,99');
			await save();

			expect(close).toHaveBeenCalledWith(expect.objectContaining({ value: -9999999999999.99 }));
			expect(JSON.stringify(close.mock.calls[0][0].value)).toBe('-9999999999999.99');
		});

		it('should save a value with a decimal point', async () => {
			await type('value', '12.50');
			await save();

			expect(close).toHaveBeenCalledWith(expect.objectContaining({ value: -12.5 }));
		});

		it('should limit the description to 255 characters and count them', async () => {
			const hint = () => element.querySelector('mat-hint')?.textContent?.trim();
			expect(input('description').getAttribute('maxlength')).toBe('255');

			await type('description', 'Pão ');
			expect(hint()).toBe('4/255');

			await type('description', 'a'.repeat(255));
			expect(hint()).toBe('255/255');
		});
	});

	it('should not save without a category', async () => {
		await render(undefined, []);

		await type('value', '12,50');
		await save();

		expect(close).not.toHaveBeenCalled();
		expect(errors()).toEqual(['Escolha uma categoria']);
	});

	it('should show the category names as plain text', async () => {
		await render(undefined, [buildCategory({ id: MERCADO_ID, name: '<b>teste</b>' })]);

		const trigger = element.querySelector('mat-select .mat-mdc-select-value')!;
		expect(trigger.textContent).toContain('<b>teste</b>');
		expect(trigger.querySelector('b')).toBeNull();

		await (await TestbedHarnessEnvironment.loader(fixture).getHarness(MatSelectHarness)).open();
		const options = Array.from(document.querySelectorAll('mat-option'));
		expect(options.map((option) => option.textContent?.trim())).toEqual(['<b>teste</b>']);
		expect(options.some((option) => option.querySelector('b'))).toBe(false);
	});

	describe('typing the date', () => {
		beforeEach(() => {
			TestBed.configureTestingModule({ providers: [{ provide: LOCALE_ID, useValue: 'pt-BR' }] });
			return render(undefined);
		});

		it.each([
			['05/10/2026', '2026-10-05T00:00:00.000Z'],
			['5/9/2026', '2026-09-05T00:00:00.000Z'],
		])('should save a typed date as day/month/year: %s', async (typed, date) => {
			await type('value', '12,50');
			await type('date', typed);
			await save();

			expect(close).toHaveBeenCalledWith(expect.objectContaining({ date }));
		});

		it('should show the typed date unchanged after leaving the field', async () => {
			await type('date', '5/9/2026');
			input('date').dispatchEvent(new Event('blur'));
			await fixture.whenStable();

			expect(input('date').value).toBe('05/09/2026');
		});

		it.each(['02/30/2026', '2026-09-24', '05/10/26'])('should not save the date %s', async (typed) => {
			await type('value', '12,50');
			await type('date', typed);
			await save();

			expect(close).not.toHaveBeenCalled();
			expect(errors()).toEqual(['Informe uma data válida']);
		});
	});

	describe('editing', () => {
		it('should prefill the transaction', async () => {
			await render(feira);

			expect(element.querySelector('[mat-dialog-title]')?.textContent?.trim()).toBe('Editar transação');
			expect(input('value').value).toBe('186,42');
			expect(input('description').value).toBe('Feira da semana');
			expect(sign().value).toBe('out');
			expect(element.querySelector('mat-select')?.textContent).toContain('Salário');
		});

		it('should save the changes keeping the calendar day', async () => {
			await render(feira);

			await setSign('Entrada');
			await type('value', '200');
			await save();

			expect(close).toHaveBeenCalledWith({
				description: 'Feira da semana',
				categoryId: SALARIO_ID,
				value: 200,
				date: '2026-09-24T00:00:00.000Z',
			});
		});

		it('should prefill a value of thousands that can be saved as is', async () => {
			await render({ ...feira, value: 8600 });

			expect(input('value').value).toBe('8600,00');
			expect(sign().value).toBe('in');

			await save();
			expect(close).toHaveBeenCalledWith(expect.objectContaining({ value: 8600 }));
		});

		it('should prefill a transaction without description', async () => {
			await render({ ...feira, description: null });

			expect(input('description').value).toBe('');
		});
	});

	it('should name the date calendar controls in Portuguese', async () => {
		await render(feira);
		const toggle = element.querySelector<HTMLButtonElement>('mat-datepicker-toggle button')!;
		expect(toggle.getAttribute('aria-label')).toBe('Abrir calendário');

		const picker: MatDatepicker<Date> = fixture.debugElement.query(By.directive(MatDatepicker)).componentInstance;
		picker.open();
		await fixture.whenStable();

		const calendarButton = (selector: string) =>
			document.querySelector<HTMLButtonElement>(`.mat-datepicker-content ${selector}`)!;
		expect(calendarButton('.mat-calendar-period-button').getAttribute('aria-label')).toBe('Escolher mês e ano');
		expect(calendarButton('.mat-calendar-previous-button').getAttribute('aria-label')).toBe('Mês anterior');
		expect(calendarButton('.mat-calendar-next-button').getAttribute('aria-label')).toBe('Próximo mês');
		expect(calendarButton('.mat-datepicker-close-button').textContent?.trim()).toBe('Fechar calendário');

		picker.close();
		await fixture.whenStable();
	});
});
