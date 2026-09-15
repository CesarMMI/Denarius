export function softBackground(hex: string): string {
	return `color-mix(in srgb, ${hex} 22%, var(--mat-sys-surface-container))`;
}
