import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { CategoryColorPicker } from './category-color-picker';

const PALETTE = ['#F4511E', '#43A047', '#1E88E5'];

@Component({
	imports: [CategoryColorPicker, ReactiveFormsModule],
	template: `<app-category-color-picker [formControl]="control" />`,
})
class TestHost {
	readonly control = new FormControl('#43A047', { nonNullable: true });
}

describe('CategoryColorPicker', () => {
	let fixture: ComponentFixture<TestHost>;
	let host: TestHost;
	let element: HTMLElement;
	let httpTesting: HttpTestingController;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [TestHost],
			providers: [provideHttpClient(), provideHttpClientTesting()],
		}).compileComponents();

		httpTesting = TestBed.inject(HttpTestingController);
		fixture = TestBed.createComponent(TestHost);
		host = fixture.componentInstance;
		element = fixture.nativeElement;
	});

	afterEach(() => httpTesting.verify());

	function expectPalette(): TestRequest {
		TestBed.tick();
		return httpTesting.expectOne('colors.json');
	}

	function modeButtons() {
		return Array.from(element.querySelectorAll<HTMLButtonElement>('.mode-toggle button'));
	}

	function swatches() {
		return Array.from(element.querySelectorAll<HTMLButtonElement>('.swatch'));
	}

	function hexInput() {
		return element.querySelector<HTMLInputElement>('.hex-field input');
	}

	describe('loading the palette', () => {
		it('should render one swatch per color in colors.json', async () => {
			expectPalette().flush(PALETTE);
			await fixture.whenStable();

			expect(swatches().map((s) => s.getAttribute('aria-label'))).toEqual(PALETTE);
		});

		it('should stay in palette mode while the palette loads', () => {
			const req = expectPalette();
			fixture.detectChanges();

			expect(modeButtons()[0].classList).toContain('selected');
			expect(swatches()).toHaveLength(0);
			req.flush(PALETTE);
		});

		it('should switch to custom mode and explain when the palette fails to load', async () => {
			expectPalette().flush('', { status: 404, statusText: 'Not Found' });
			await fixture.whenStable();

			expect(modeButtons()[1].classList).toContain('selected');
			expect(hexInput()?.value).toBe('#43A047');

			modeButtons()[0].click();
			await fixture.whenStable();

			expect(element.querySelector('.palette-error')?.textContent).toContain('Não foi possível carregar a paleta');
		});

		it('should start from the first palette color when the control is empty', async () => {
			const req = expectPalette();
			host.control.setValue('');
			req.flush(PALETTE);
			await fixture.whenStable();

			expect(host.control.value).toBe(PALETTE[0]);
			expect(host.control.touched).toBe(false);
			expect(modeButtons()[0].classList).toContain('selected');
			expect(element.querySelector('.swatch.selected')?.getAttribute('aria-label')).toBe(PALETTE[0]);
		});

		it('should leave an empty control empty when the palette fails to load', async () => {
			const req = expectPalette();
			host.control.setValue('');
			req.flush('', { status: 404, statusText: 'Not Found' });
			await fixture.whenStable();

			expect(host.control.value).toBe('');
			expect(modeButtons()[1].classList).toContain('selected');
		});

		it('should treat a colors.json that is not a list of colors as a failure', async () => {
			expectPalette().flush({ primary: '#43A047' });
			await fixture.whenStable();

			expect(modeButtons()[1].classList).toContain('selected');
			modeButtons()[0].click();
			await fixture.whenStable();

			expect(element.querySelector('.palette-error')).not.toBeNull();
		});
	});

	describe('with the palette loaded', () => {
		beforeEach(async () => {
			expectPalette().flush(PALETTE);
			await fixture.whenStable();
		});

		it('should open in palette mode with the current color selected', () => {
			expect(modeButtons()[0].classList).toContain('selected');
			const selected = swatches().filter((s) => s.classList.contains('selected'));
			expect(selected.map((s) => s.getAttribute('aria-label'))).toEqual(['#43A047']);
		});

		it('should match palette colors case-insensitively', async () => {
			host.control.setValue('#43a047');
			await fixture.whenStable();

			expect(modeButtons()[0].classList).toContain('selected');
			expect(element.querySelector('.swatch.selected')?.getAttribute('aria-label')).toBe('#43A047');
		});

		it('should update the control when a swatch is clicked', async () => {
			const target = swatches().find((s) => s.getAttribute('aria-label') === '#1E88E5')!;
			target.click();
			await fixture.whenStable();

			expect(host.control.value).toBe('#1E88E5');
			expect(host.control.touched).toBe(true);
			expect(target.classList).toContain('selected');
		});

		it('should open in custom mode for a color outside the palette', async () => {
			host.control.setValue('#123456');
			await fixture.whenStable();

			expect(modeButtons()[1].classList).toContain('selected');
			expect(swatches()).toHaveLength(0);
			expect(hexInput()?.value).toBe('#123456');
		});

		it('should update the control from the custom hex input', async () => {
			modeButtons()[1].click();
			await fixture.whenStable();

			const input = hexInput()!;
			input.value = '#abcdef';
			input.dispatchEvent(new Event('input'));

			expect(host.control.value).toBe('#abcdef');
		});

		it('should disable every button when the control is disabled', async () => {
			host.control.disable();
			await fixture.whenStable();

			const buttons = [...modeButtons(), ...swatches()];
			expect(buttons.length).toBeGreaterThan(0);
			expect(buttons.every((b) => b.disabled)).toBe(true);
		});
	});
});
