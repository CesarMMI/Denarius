import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';
import { buildCategory } from '../../../categories/testing/category-fixture';
import { ConfirmDeleteDialog } from '../../../shared/confirm-delete-dialog/confirm-delete-dialog';
import { toApiDate } from '../../../shared/utils/api-date';
import { buildTransaction } from '../../testing/transaction-fixture';
import { Transaction } from '../../types/transaction';
import { TransactionFormSheet } from './transaction-form-sheet';

describe('TransactionFormSheet', () => {
	const categories = [buildCategory({ id: 'mercado', name: 'Mercado' }), buildCategory({ id: 'salario', name: 'Salário' })];
	const feira = buildTransaction({
		id: 't1',
		description: 'Feira da semana',
		categoryId: 'salario',
		value: -186.42,
		date: '2026-09-24T00:00:00Z',
	});

	let fixture: ComponentFixture<TransactionFormSheet>;
	let element: HTMLElement;
	let callback: ReturnType<typeof vi.fn>;
	let dialog: { open: ReturnType<typeof vi.fn> };

	async function render(transaction: Transaction | undefined) {
		callback = vi.fn();
		dialog = { open: vi.fn() };
		await TestBed.configureTestingModule({
			imports: [TransactionFormSheet],
			providers: [
				{ provide: MatBottomSheetRef, useValue: { dismiss: () => undefined } },
				{ provide: MAT_BOTTOM_SHEET_DATA, useValue: { transaction, categories, callback } },
			],
		})
			.overrideComponent(TransactionFormSheet, { add: { providers: [{ provide: MatDialog, useValue: dialog }] } })
			.compileComponents();

		fixture = TestBed.createComponent(TransactionFormSheet);
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

	function signButtons() {
		return Array.from(element.querySelectorAll<HTMLButtonElement>('.sign-btn'));
	}

	async function save() {
		element.querySelectorAll<HTMLButtonElement>('.header button')[1].click();
		await fixture.whenStable();
	}

	describe('creating', () => {
		beforeEach(() => render(undefined));

		it('should start as an empty expense in the first category, dated today', () => {
			expect(element.querySelector('.title')?.textContent?.trim()).toBe('Nova transação');
			expect(input('value').value).toBe('');
			expect(signButtons()[1].classList).toContain('selected');
			expect(element.querySelector('mat-select')?.textContent).toContain('Mercado');
			expect(element.querySelector('.delete')).toBeNull();
		});

		it('should not save without a valid value', async () => {
			await type('value', '12,345');
			await save();

			expect(callback).not.toHaveBeenCalled();
			expect(element.querySelector('.hint.error')?.textContent).toBe('Informe um valor válido');
		});

		it('should save an expense as a negative value with the day as UTC midnight', async () => {
			await type('value', '12,50');
			await type('description', '  Pão  ');
			await save();

			expect(callback).toHaveBeenCalledWith(
				{
					type: 'save',
					result: {
						id: undefined,
						description: 'Pão',
						categoryId: 'mercado',
						value: -12.5,
						date: toApiDate(new Date()),
					},
				},
				expect.any(TransactionFormSheet),
			);
		});

		it('should save an income as a positive value and a blank description as null', async () => {
			signButtons()[0].click();
			await type('value', '8600');
			await type('description', '   ');
			await save();

			expect(callback).toHaveBeenCalledWith(
				expect.objectContaining({ result: expect.objectContaining({ value: 8600, description: null }) }),
				expect.any(TransactionFormSheet),
			);
		});
	});

	describe('editing', () => {
		beforeEach(() => render(feira));

		it('should prefill the transaction', () => {
			expect(element.querySelector('.title')?.textContent?.trim()).toBe('Editar transação');
			expect(input('value').value).toBe('186,42');
			expect(input('description').value).toBe('Feira da semana');
			expect(signButtons()[1].classList).toContain('selected');
			expect(element.querySelector('mat-select')?.textContent).toContain('Salário');
		});

		it('should save the changes keeping the id and the calendar day', async () => {
			signButtons()[0].click();
			await type('value', '200');
			await save();

			expect(callback).toHaveBeenCalledWith(
				{
					type: 'save',
					result: {
						id: 't1',
						description: 'Feira da semana',
						categoryId: 'salario',
						value: 200,
						date: '2026-09-24T00:00:00.000Z',
					},
				},
				expect.any(TransactionFormSheet),
			);
		});

		it('should prefill a transaction without description', async () => {
			TestBed.resetTestingModule();
			await render({ ...feira, description: null });

			expect(input('description').value).toBe('');
		});

		it('should ask before deleting and emit the deletion when confirmed', async () => {
			dialog.open.mockReturnValue({ afterClosed: () => of(true) });

			element.querySelector<HTMLButtonElement>('.delete')!.click();

			expect(dialog.open).toHaveBeenCalledWith(ConfirmDeleteDialog, expect.anything());
			expect(callback).toHaveBeenCalledWith({ type: 'delete', id: 't1' }, expect.any(TransactionFormSheet));
		});

		it('should not delete when the confirmation is cancelled', () => {
			dialog.open.mockReturnValue({ afterClosed: () => of(false) });

			element.querySelector<HTMLButtonElement>('.delete')!.click();

			expect(callback).not.toHaveBeenCalled();
		});
	});
});
