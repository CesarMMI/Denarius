import { Category } from '../types/category';

export function buildCategory(overrides: Partial<Category> = {}): Category {
	return {
		id: '3f2a1c4e-0000-4000-8000-000000000001',
		name: 'Mercado',
		color: '#43A047',
		transactionCount: 0,
		balance: 0,
		createdAt: '2026-01-01T00:00:00.000Z',
		updatedAt: '2026-01-01T00:00:00.000Z',
		...overrides,
	};
}
