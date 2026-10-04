import { TestBed } from '@angular/core/testing';
import { ChartThemeService } from './chart-theme.service';

describe('ChartThemeService', () => {
	let schemeChanged: ((event: Pick<MediaQueryListEvent, 'matches'>) => void) | undefined;

	beforeEach(() => {
		schemeChanged = undefined;
		vi.stubGlobal('matchMedia', (query: string) => ({
			matches: false,
			media: query,
			addEventListener: (_: string, listener: typeof schemeChanged) => (schemeChanged = listener),
			removeEventListener: vi.fn(),
		}));
		// jsdom resolves no CSS variable: echo the color the probe asked for.
		vi
			.spyOn(window, 'getComputedStyle')
			.mockImplementation((element) => ({ color: (element as HTMLElement).style.color }) as CSSStyleDeclaration);
	});

	afterEach(() => {
		vi.unstubAllGlobals();
		vi.restoreAllMocks();
	});

	it('should read each color from its Material system token', () => {
		const colors = TestBed.inject(ChartThemeService).colors();

		expect(colors).toEqual({
			expense: 'var(--mat-sys-error)',
			income: 'var(--mat-sys-tertiary)',
			neutral: 'var(--mat-sys-outline)',
			primary: 'var(--mat-sys-primary)',
			primaryVariant: 'var(--mat-sys-on-primary)',
			text: 'var(--mat-sys-on-surface)',
			grid: 'var(--mat-sys-surface-container-high)',
			surface: 'var(--mat-sys-surface-container-low)',
		});
		expect(document.body.querySelector('span')).toBeNull();
	});

	it('should read the colors again when the color scheme changes', () => {
		const service = TestBed.inject(ChartThemeService);
		const before = service.colors();

		expect(service.colors()).toBe(before);
		schemeChanged!({ matches: true });

		expect(service.colors()).not.toBe(before);
		expect(service.colors()).toEqual(before);
	});
});
