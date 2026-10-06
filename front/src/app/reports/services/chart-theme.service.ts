import { DOCUMENT } from '@angular/common';
import { computed, DestroyRef, inject, Injectable, signal } from '@angular/core';

/** The theme colors the charts paint with. */
export interface ChartColors {
	/** Money out, the same red as everywhere else. */
	expense: string;
	/** Money in. */
	income: string;
	/** Whatever is context, like "Outras". */
	neutral: string;
	/** The primary color for the chart. */
	primary: string;
	/** The variant of the primary color for the chart. */
	primaryVariant: string;
	text: string;
	grid: string;
	/** The card behind the charts, which separates touching marks. */
	surface: string;
}

/**
 * The Material theme colors as concrete colors: a canvas can't read the `--mat-sys-*` variables, nor the
 * `light-dark()` the theme defines them with. They are read again when the system switches between light and dark.
 */
@Injectable({
	providedIn: 'root',
})
export class ChartThemeService {
	private readonly document = inject(DOCUMENT);
	private readonly darkScheme = this.document.defaultView?.matchMedia?.('(prefers-color-scheme: dark)');
	private readonly dark = signal(this.darkScheme?.matches ?? false);

	readonly colors = computed<ChartColors>(() => {
		this.dark();
		return {
			expense: this.resolve('error'),
			income: this.resolve('tertiary'),
			neutral: this.resolve('outline'),
			primary: this.resolve('primary'),
			primaryVariant: this.resolve('secondary'),
			text: this.resolve('on-surface'),
			grid: this.resolve('surface-container-high'),
			surface: this.resolve('surface-container-low'),
		};
	});

	constructor() {
		const changed = (event: MediaQueryListEvent) => this.dark.set(event.matches);
		this.darkScheme?.addEventListener('change', changed);
		inject(DestroyRef).onDestroy(() => this.darkScheme?.removeEventListener('change', changed));
	}

	private resolve(token: string) {
		const probe = this.document.createElement('span');
		probe.style.color = `var(--mat-sys-${token})`;
		this.document.body.append(probe);
		const color = this.document.defaultView?.getComputedStyle(probe).color ?? '';
		probe.remove();
		return color;
	}
}
