import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatChipsModule } from '@angular/material/chips';
import { MatChipColor } from './mat-chip-color';

@Component({
	imports: [MatChipsModule, MatChipColor],
	template: `<mat-chip [appMatChipColor]="color()">Mercado</mat-chip>`,
})
class Host {
	readonly color = signal('#000000');
}

describe('MatChipColor', () => {
	let fixture: ComponentFixture<Host>;

	beforeEach(async () => {
		fixture = TestBed.createComponent(Host);
		await fixture.whenStable();
	});

	async function chipStyle(color: string) {
		fixture.componentInstance.color.set(color);
		await fixture.whenStable();
		return (fixture.nativeElement as HTMLElement).querySelector<HTMLElement>('mat-chip')!.style;
	}

	it('should fill and outline the chip with a pale tone of the color and write in a dark one', async () => {
		const style = await chipStyle('#43A047');

		expect(style.getPropertyValue('--mat-chip-elevated-container-color')).toBe('light-dark(#c9ebca, #1e5120)');
		expect(style.getPropertyValue('--mat-chip-outline-color')).toBe('light-dark(#c9ebca, #1e5120)');
		expect(style.getPropertyValue('--mat-chip-label-text-color')).toBe('light-dark(#1e5120, #c9ebca)');
	});

	// The colors of colors.json where black or white text read worst, and the extremes.
	it.each(['#D81B60', '#00897B', '#E53935', '#F4511E', '#000000', '#FFFFFF', '#FFFF00', '#00FF00', '#0000FF'])(
		'should keep at least 7:1 of contrast on %s',
		async (color) => {
			const style = await chipStyle(color);
			const [light, dark] = style.getPropertyValue('--mat-chip-elevated-container-color').match(/#\w{6}/g)!;

			expect((luminance(light) + 0.05) / (luminance(dark) + 0.05)).toBeGreaterThanOrEqual(7);
		},
	);
});

/** The WCAG relative luminance of a `#rrggbb` color. */
function luminance(hex: string) {
	const [r, g, b] = [1, 3, 5].map((i) => {
		const channel = parseInt(hex.slice(i, i + 2), 16) / 255;
		return channel <= 0.04045 ? channel / 12.92 : ((channel + 0.055) / 1.055) ** 2.4;
	});
	return 0.2126 * r + 0.7152 * g + 0.0722 * b;
}
