import { Component, forwardRef, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

const PALETTE = ['#F4511E', '#F6BF26', '#43A047', '#8E24AA', '#1E88E5', '#D81B60', '#00897B', '#FB8C00', '#C0CA33', '#78909C', '#E53935'];

type ColorMode = 'palette' | 'custom';

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
	protected readonly palette = PALETTE;

	protected readonly value = signal(PALETTE[0]);
	protected readonly disabled = signal(false);
	protected readonly mode = signal<ColorMode>('palette');

	private onChange: (value: string) => void = () => undefined;
	private onTouched: () => void = () => undefined;

	writeValue(value: string): void {
		const normalized = value ?? PALETTE[0];
		this.value.set(normalized);
		this.mode.set(this.isPaletteColor(normalized) ? 'palette' : 'custom');
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
		return this.palette.some((c) => c.toLowerCase() === color.toLowerCase());
	}
}
