import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { CategoryColorPicker } from './category-color-picker';

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

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [TestHost],
		}).compileComponents();

		fixture = TestBed.createComponent(TestHost);
		host = fixture.componentInstance;
		element = fixture.nativeElement;
		await fixture.whenStable();
	});

	function modeButtons() {
		return Array.from(element.querySelectorAll<HTMLButtonElement>('.mode-toggle button'));
	}

	function swatches() {
		return Array.from(element.querySelectorAll<HTMLButtonElement>('.swatch'));
	}

	function hexInput() {
		return element.querySelector<HTMLInputElement>('.hex-field input');
	}

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
