import { TestBed } from '@angular/core/testing';
import { MAT_DATE_LOCALE, MAT_NATIVE_DATE_FORMATS } from '@angular/material/core';
import { PtBrDateAdapter } from './pt-br-date-adapter';

describe('PtBrDateAdapter', () => {
	let adapter: PtBrDateAdapter;

	beforeEach(() => {
		TestBed.configureTestingModule({
			providers: [PtBrDateAdapter, { provide: MAT_DATE_LOCALE, useValue: 'pt-BR' }],
		});
		adapter = TestBed.inject(PtBrDateAdapter);
	});

	function parse(value: string) {
		return adapter.parse(value, MAT_NATIVE_DATE_FORMATS.parse.dateInput);
	}

	it('should parse a typed date as day/month/year', () => {
		expect(parse('05/10/2026')).toEqual(new Date(2026, 9, 5));
		expect(parse('5/9/2026')).toEqual(new Date(2026, 8, 5));
	});

	it('should parse a day above 12', () => {
		expect(parse('24/09/2026')).toEqual(new Date(2026, 8, 24));
	});

	it('should ignore surrounding spaces', () => {
		expect(parse(' 05/10/2026 ')).toEqual(new Date(2026, 9, 5));
	});

	it.each([
		'31/02/2026',
		'02/30/2026',
		'13/13/2026',
		'00/10/2026',
		'05/10/26',
		'05/10/0026',
		'5/10',
		'05-10-2026',
		'2026-09-24',
		'Oct 5 2026',
		'05/10/2026x',
	])('should reject %s', (value) => {
		const date = parse(value);

		expect(date).not.toBeNull();
		expect(adapter.isValid(date!)).toBe(false);
	});

	it('should parse an empty text as no date', () => {
		expect(parse('')).toBeNull();
		expect(parse('   ')).toBeNull();
	});

	it('should keep formatting as dd/mm/yyyy', () => {
		expect(adapter.format(new Date(2026, 9, 5), MAT_NATIVE_DATE_FORMATS.display.dateInput)).toBe('05/10/2026');
	});
});
