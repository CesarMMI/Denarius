import { Transaction } from '../types/transaction';

export function buildTransaction(overrides: Partial<Transaction> = {}): Transaction {
	return {
		id: '7b1d2e3f-0000-4000-8000-000000000001',
		description: 'Feira da semana',
		categoryId: '3f2a1c4e-0000-4000-8000-000000000001',
		value: -186.42,
		date: '2026-09-24T00:00:00Z',
		createdAt: '2026-09-24T12:00:00Z',
		updatedAt: '2026-09-24T12:00:00Z',
		...overrides,
	};
}
