import { httpResource } from '@angular/common/http';
import { Component, computed, effect, forwardRef, linkedSignal, signal, untracked } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

/** Served from `public/`, so the palette can change without rebuilding the app. */
const PALETTE_URL = 'colors.json';

type ColorMode = 'palette' | 'custom';

function parsePalette(raw: unknown): string[] {
	if (!Array.isArray(raw) || !raw.every((color) => typeof color === 'string')) {
		throw new Error(`${PALETTE_URL} must be an array of color strings`);
	}
	return raw;
}

@Component({
	selector: 'app-category-color-picker',
	templateUrl: './category-color-picker.html',
	styleUrl: './category-color-picker.scss',
	imports: [MatIconModule],
	providers: [
		{
			provide: NG_VALUE_ACCESSOR,
			useExisting: forwardRef(() => CategoryColorPicker),
			multi: true,
		},
	],
})
export class CategoryColorPicker implements ControlValueAccessor {
	protected readonly paletteResource = httpResource(() => PALETTE_URL, { parse: parsePalette, defaultValue: [] });
	protected readonly palette = computed(() => (this.paletteResource.hasValue() ? this.paletteResource.value() : []));

	protected readonly value = signal('');
	protected readonly disabled = signal(false);

	/** The value last written by the form, which picks the starting mode once the palette is known. */
	private readonly writtenValue = signal('');
	protected readonly mode = linkedSignal<ColorMode>(() =>
		this.paletteResource.isLoading() || this.isPaletteColor(this.writtenValue()) ? 'palette' : 'custom',
	);

	private onChange: (value: string) => void = () => undefined;
	private onTouched: () => void = () => undefined;

	constructor() {
		// With no color yet, as for a new category, start from the first color in the palette.
		effect(() => {
			const first = this.palette()[0];
			if (!first || untracked(this.writtenValue)) return;
			this.writtenValue.set(first);
			this.value.set(first);
			this.onChange(first);
		});
	}

	writeValue(value: string | null): void {
		this.value.set(value ?? '');
		this.writtenValue.set(value ?? '');
	}

	registerOnChange(fn: (value: string) => void): void {
		this.onChange = fn;
	}

	registerOnTouched(fn: () => void): void {
		this.onTouched = fn;
	}

	setDisabledState(isDisabled: boolean): void {
		this.disabled.set(isDisabled);
	}

	protected setMode(mode: ColorMode) {
		this.mode.set(mode);
	}

	protected select(color: string) {
		this.value.set(color);
		this.onChange(color);
		this.onTouched();
	}

	protected onCustomInput(event: Event) {
		this.select((event.target as HTMLInputElement).value);
	}

	private isPaletteColor(color: string) {
		return this.palette().some((c) => c.toLowerCase() === color.toLowerCase());
	}
}
