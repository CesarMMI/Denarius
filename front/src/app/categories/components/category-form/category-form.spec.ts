import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { buildCategory } from '../../testing/category-fixture';
import { Category } from '../../types/category';
import { CategoryForm } from './category-form';

const PALETTE = ['#F4511E', '#8E24AA', '#1E88E5'];

describe('CategoryForm', () => {
	let fixture: ComponentFixture<CategoryForm>;
	let element: HTMLElement;
	let close: ReturnType<typeof vi.fn>;

	async function render(category: Category | undefined, palette: string[] | 'error' = PALETTE) {
		close = vi.fn();
		TestBed.configureTestingModule({
			providers: [
				provideHttpClient(),
				provideHttpClientTesting(),
				{ provide: MatDialogRef, useValue: { close } },
				{ provide: MAT_DIALOG_DATA, useValue: category },
			],
		});
		fixture = TestBed.createComponent(CategoryForm);
		element = fixture.nativeElement;
		TestBed.tick();
		const req = TestBed.inject(HttpTestingController).expectOne('data/default-colors.json');
		if (palette === 'error') req.flush('', { status: 404, statusText: 'Not Found' });
		else req.flush(palette);
		await fixture.whenStable();
	}

	function nameInput() {
		return element.querySelector<HTMLInputElement>('input[formControlName="name"]')!;
	}

	function colorInput() {
		return element.querySelector<HTMLInputElement>('input[type="color"]')!;
	}

	function swatches() {
		return Array.from(element.querySelectorAll<HTMLButtonElement>('.colors button'));
	}

	function pressed() {
		return swatches()
			.filter((s) => s.getAttribute('aria-pressed') === 'true')
			.map((s) => s.getAttribute('aria-label'));
	}

	async function typeName(value: string) {
		nameInput().value = value;
		nameInput().dispatchEvent(new Event('input'));
		await fixture.whenStable();
	}

	async function save() {
		element.querySelector('form')!.dispatchEvent(new Event('submit'));
		await fixture.whenStable();
	}

	describe('creating', () => {
		beforeEach(() => render(undefined));

		it('should show one swatch per color of the palette and start with the first one', () => {
			expect(element.querySelector('[mat-dialog-title]')?.textContent?.trim()).toBe('Nova categoria');
			expect(swatches().map((s) => s.getAttribute('aria-label'))).toEqual(PALETTE);
			expect(pressed()).toEqual([PALETTE[0]]);
			expect(colorInput().value).toBe(PALETTE[0].toLowerCase());
		});

		it('should not save without a name', async () => {
			await save();

			expect(close).not.toHaveBeenCalled();
			expect(element.querySelector('mat-error')?.textContent).toBe('Informe um nome');
		});

		it('should save the name with the picked swatch', async () => {
			await typeName('Mercado');
			swatches()[2].click();
			await fixture.whenStable();
			expect(pressed()).toEqual([PALETTE[2]]);

			await save();
			expect(close).toHaveBeenCalledWith({ name: 'Mercado', color: PALETTE[2] });
		});

		it('should save a custom color', async () => {
			await typeName('Mercado');
			colorInput().value = '#123456';
			colorInput().dispatchEvent(new Event('input'));
			await fixture.whenStable();
			expect(pressed()).toEqual([]);

			await save();
			expect(close).toHaveBeenCalledWith({ name: 'Mercado', color: '#123456' });
		});
	});

	it('should still offer a custom color when the palette fails to load', async () => {
		await render(undefined, 'error');
		await typeName('Mercado');

		expect(swatches()).toEqual([]);
		await save();
		expect(close).toHaveBeenCalledWith({ name: 'Mercado', color: '#000000' });
	});

	describe('editing', () => {
		const category = buildCategory({ id: 'cat-1', name: 'Lazer', color: '#8E24AA' });

		beforeEach(() => render(category));

		it('should prefill the name and the color', () => {
			expect(element.querySelector('[mat-dialog-title]')?.textContent?.trim()).toBe('Editar categoria');
			expect(nameInput().value).toBe('Lazer');
			expect(pressed()).toEqual(['#8E24AA']);
		});

		it('should save the changes', async () => {
			await typeName('Lazer e cultura');
			await save();

			expect(close).toHaveBeenCalledWith({ name: 'Lazer e cultura', color: '#8E24AA' });
		});
	});
});
