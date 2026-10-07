import { Injectable } from '@angular/core';
import { NativeDateAdapter } from '@angular/material/core';

/**
 * Reads a typed date only as day/month/year with a four-digit year (`5/9/2026`, `05/09/2026`), as a local date.
 * The native adapter reads it with `Date.parse`, which takes `05/09/2026` as May 9 and accepts loose formats.
 * Days that don't exist and years before 0100 are invalid; formatting is inherited.
 */
@Injectable()
export class PtBrDateAdapter extends NativeDateAdapter {
	override parse(value: unknown, parseFormat?: unknown): Date | null {
		if (typeof value !== 'string') return super.parse(value, parseFormat);
		const text = value.trim();
		if (!text) return null;

		const match = text.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})$/);
		if (!match) return this.invalid();
		const [day, month, year] = match.slice(1).map(Number);
		const date = new Date(year, month - 1, day);
		const overflowed = date.getFullYear() !== year || date.getMonth() !== month - 1 || date.getDate() !== day;
		return overflowed ? this.invalid() : date;
	}
}
