import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';
import { buildCategory } from '../../testing/category-fixture';
import { Category } from '../../types/category';
import { CategoryFormSheet } from './category-form-sheet';

describe('CategoryFormSheet', () => {
	let fixture: ComponentFixture<CategoryFormSheet>;
	let element: HTMLElement;
	let callback: ReturnType<typeof vi.fn>;
	let dismiss: ReturnType<typeof vi.fn>;

	async function render(category: Category | undefined) {
		callback = vi.fn();
		dismiss = vi.fn();
		await TestBed.configureTestingModule({
			imports: [CategoryFormSheet],
			providers: [
				{ provide: MatBottomSheetRef, useValue: { dismiss } },
				{ provide: MAT_BOTTOM_SHEET_DATA, useValue: { category, callback } },
			],
		}).compileComponents();

		fixture = TestBed.createComponent(CategoryFormSheet);
		element = fixture.nativeElement;
		await fixture.whenStable();
	}

	function nameInput() {
		return element.querySelector<HTMLInputElement>('input[formControlName="name"]')!;
	}

	function headerButtons() {
		return Array.from(element.querySelectorAll<HTMLButtonElement>('.header button'));
	}

	function saveButton() {
		return headerButtons()[1];
	}

	async function typeName(value: string) {
		const input = nameInput();
		input.value = value;
		input.dispatchEvent(new Event('input'));
		await fixture.whenStable();
	}

	describe('creating', () => {
		beforeEach(() => render(undefined));

		it('should show the create title, an empty name and no delete button', () => {
			expect(element.querySelector('.title')?.textContent?.trim()).toBe('Nova categoria');
			expect(nameInput().value).toBe('');
			expect(element.querySelector('.delete')).toBeNull();
		});

		it('should not save and should show an error when the name is empty', async () => {
			saveButton().click();
			await fixture.whenStable();

			expect(callback).not.toHaveBeenCalled();
			expect(element.querySelector('mat-error')?.textContent).toContain('Nome é obrigatório');
		});

		it('should not save a name longer than 100 characters', async () => {
			await typeName('a'.repeat(101));
			saveButton().click();
			await fixture.whenStable();

			expect(callback).not.toHaveBeenCalled();
			expect(element.querySelector('mat-error')?.textContent).toContain('no máximo 100 caracteres');
		});

		it('should save the name with the default color and no id', async () => {
			await typeName('Mercado');
			saveButton().click();

			expect(callback).toHaveBeenCalledWith(
				{ type: 'save', result: { name: 'Mercado', color: '#43A047', id: undefined } },
				expect.any(CategoryFormSheet),
			);
		});

		it('should save when the form is submitted with enter', async () => {
			await typeName('Mercado');
			element.querySelector('form')!.dispatchEvent(new Event('submit'));

			expect(callback).toHaveBeenCalledWith(
				expect.objectContaining({ type: 'save', result: expect.objectContaining({ name: 'Mercado' }) }),
				expect.any(CategoryFormSheet),
			);
		});

		it('should close without saving', () => {
			headerButtons()[0].click();

			expect(dismiss).toHaveBeenCalled();
			expect(callback).not.toHaveBeenCalled();
		});
	});

	describe('editing', () => {
		const category = buildCategory({ id: 'cat-1', name: 'Lazer', color: '#8E24AA' });

		beforeEach(() => render(category));

		it('should show the edit title and prefill the name', () => {
			expect(element.querySelector('.title')?.textContent?.trim()).toBe('Editar categoria');
			expect(nameInput().value).toBe('Lazer');
		});

		it('should save the changes with the category id', async () => {
			await typeName('Lazer e cultura');
			saveButton().click();

			expect(callback).toHaveBeenCalledWith(
				{ type: 'save', result: { name: 'Lazer e cultura', color: '#8E24AA', id: 'cat-1' } },
				expect.any(CategoryFormSheet),
			);
		});

		it('should ask to delete the category', () => {
			element.querySelector<HTMLButtonElement>('.delete')!.click();

			expect(callback).toHaveBeenCalledWith({ type: 'delete', category }, expect.any(CategoryFormSheet));
		});
	});
});
