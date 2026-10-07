/**
 * Compares a signal's state by content, for its `equal` option: dates by their time, plain objects key by key (one
 * level deep, with dates compared by their time), anything else by `Object.is`. Writing the same filters, sort or month
 * again then doesn't notify, so nothing that reads the signal (a request, a chart) runs again.
 */
export function shallowEqual(a: unknown, b: unknown): boolean {
	if (isObject(a) && isObject(b) && !(a instanceof Date) && !(b instanceof Date)) {
		const keys = Object.keys(a);
		return keys.length === Object.keys(b).length && keys.every((key) => key in b && sameValue(a[key], b[key]));
	}
	return sameValue(a, b);
}

function sameValue(a: unknown, b: unknown): boolean {
	if (a instanceof Date && b instanceof Date) return Object.is(a.getTime(), b.getTime());
	return Object.is(a, b);
}

function isObject(value: unknown): value is Record<string, unknown> {
	return typeof value === 'object' && value !== null;
}
