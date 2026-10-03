import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatButtonToggleGroup } from '@angular/material/button-toggle';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { By } from '@angular/platform-browser';
import { buildCategory } from '../../../categories/testing/category-fixture';
import { toApiDate } from '../../../shared/utils/api-date';
import { buildTransaction } from '../../testing/transaction-fixture';
import { Transaction } from '../../types/transaction';
import { TransactionForm } from './transaction-form';

describe('TransactionForm', () => {
	const categories = [
		buildCategory({ id: 'mercado', name: 'Mercado' }),
		buildCategory({ id: 'salario', name: 'Salário' }),
	];
	const feira = buildTransaction({
		id: 't1',
		description: 'Feira da semana',
		categoryId: 'salario',
		value: -186.42,
		date: '2026-09-24T00:00:00Z',
	});

	let fixture: ComponentFixture<TransactionForm>;
	let element: HTMLElement;
	let close: ReturnType<typeof vi.fn>;

	async function render(transaction: Transaction | undefined) {
		close = vi.fn();
		TestBed.configureTestingModule({
			providers: [
				{ provide: MatDialogRef, useValue: { close } },
				{ provide: MAT_DIALOG_DATA, useValue: { transaction, categories } },
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
				categoryId: 'mercado',
				value: -12.5,
				date: toApiDate(new Date()),
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
				categoryId: 'salario',
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
});
