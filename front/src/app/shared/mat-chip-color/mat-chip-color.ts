import { computed, Directive, input } from '@angular/core';

/**
 * Paints a chip with tones of a color, Material's tones 90 and 30: the light one fills the chip and the dark one
 * writes its label, swapped in dark mode. Whatever the color, the two keep about 7.2:1 of contrast, above the 7:1
 * of WCAG AAA, so reading comes before matching the color exactly.
 */
@Directive({
	selector: 'mat-chip[appMatChipColor]',
	host: {
		'[style.--mat-chip-elevated-container-color]': 'background()',
		'[style.--mat-chip-label-text-color]': 'foreground()',
		'[style.--mat-chip-outline-color]': 'background()',
	},
})
export class MatChipColor {
	/** A `#RRGGBB` color, the format the API stores. */
	readonly color = input.required<string>({ alias: 'appMatChipColor' });

	private readonly tones = computed(() => {
		const rgb = MatChipColor.toLinear(this.color());
		// The fill and the outline keep only part of the saturation, so vivid colors neither turn neon nor stand out.
		return {
			light: MatChipColor.toHex(MatChipColor.toTone(rgb, 90, 0.3)),
			dark: MatChipColor.toHex(MatChipColor.toTone(rgb, 30)),
		};
	});
	protected readonly background = computed(() => `light-dark(${this.tones().light}, ${this.tones().dark})`);
	protected readonly foreground = computed(() => `light-dark(${this.tones().dark}, ${this.tones().light})`);

	/**
	 * The color at a tone, the CIE lightness from 0 to 100 that sets its luminance, with the same hue. It keeps up to
	 * `saturation` of its saturation, less when the tone cannot hold that much. Black has no hue, so it turns gray.
	 */
	private static toTone(rgb: number[], tone: number, saturation = 1) {
		const target = ((tone + 16) / 116) ** 3;
		const current = MatChipColor.luminance(rgb);
		const scaled = current ? rgb.map((c) => (c * target) / current) : rgb.map(() => target);
		const max = Math.max(...scaled);
		const kept = Math.min(saturation, max > 1 ? (1 - target) / (max - target) : 1);
		// Mixing with the gray of the same luminance keeps the luminance.
		return scaled.map((c) => target + (c - target) * kept);
	}

	/** The WCAG relative luminance of a linear RGB color, from 0 for black to 1 for white. */
	private static luminance([r, g, b]: number[]) {
		return 0.2126 * r + 0.7152 * g + 0.0722 * b;
	}

	/** Linear RGB is where light adds up, so it is where colors scale and mix. */
	private static toLinear(hex: string) {
		return [1, 3, 5].map((i) => {
			const channel = parseInt(hex.slice(i, i + 2), 16) / 255;
			return channel <= 0.04045 ? channel / 12.92 : ((channel + 0.055) / 1.055) ** 2.4;
		});
	}

	private static toHex(rgb: number[]) {
		const channels = rgb.map((c) => {
			const channel = c <= 0.0031308 ? c * 12.92 : 1.055 * c ** (1 / 2.4) - 0.055;
			return Math.round(channel * 255)
				.toString(16)
				.padStart(2, '0');
		});
		return `#${channels.join('')}`;
	}
}
