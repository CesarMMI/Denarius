import { shallowEqual } from './shallow-equal';

describe('shallowEqual', () => {
	const sameObject = { name: 'mer' };

	it.each<[string, unknown, unknown]>([
		['the same object', sameObject, sameObject],
		['objects with the same values', { name: 'mer', withTransaction: true }, { name: 'mer', withTransaction: true }],
		['dates of the same moment', new Date(2026, 8, 1), new Date(2026, 8, 1)],
		[
			'objects with dates of the same moment',
			{ name: '', month: new Date(2026, 8, 1) },
			{ name: '', month: new Date(2026, 8, 1) },
		],
		['two invalid dates', new Date(NaN), new Date(NaN)],
		['two nulls', null, null],
		['equal strings', 'mer', 'mer'],
	])('should take %s as equal', (_, a, b) => {
		expect(shallowEqual(a, b)).toBe(true);
	});

	it.each<[string, unknown, unknown]>([
		['objects with a different value', { name: 'mer' }, { name: 'merc' }],
		['objects with different keys', { name: 'mer' }, { name: 'mer', withTransaction: true }],
		['objects where a key is missing on one side', { name: 'mer', month: undefined }, { name: 'mer', type: undefined }],
		['dates of different days', new Date(2026, 8, 1), new Date(2026, 8, 2)],
		['objects with dates of different months', { month: new Date(2026, 8, 1) }, { month: new Date(2026, 9, 1) }],
		['a date and null', new Date(2026, 8, 1), null],
		['an object with a date and one with null', { month: new Date(2026, 8, 1) }, { month: null }],
		['an empty string and false', '', false],
		['nested objects with the same values', { range: { from: 1 } }, { range: { from: 1 } }],
	])('should take %s as different', (_, a, b) => {
		expect(shallowEqual(a, b)).toBe(false);
	});
});
