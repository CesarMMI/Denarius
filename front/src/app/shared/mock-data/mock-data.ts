import { Category } from '../../categories/types/category';
import { Transaction } from '../../transactions/types/transaction';

const TIMESTAMP = '2026-01-01T00:00:00.000Z';

export const MOCK_CATEGORIES: Category[] = [
	{ id: 'aluguel', name: 'Aluguel', color: '#F4511E', transactionCount: 1, balance: -2400, createdAt: TIMESTAMP, updatedAt: TIMESTAMP },
	{
		id: 'assinaturas',
		name: 'Assinaturas',
		color: '#D81B60',
		transactionCount: 5,
		balance: -149.6,
		createdAt: TIMESTAMP,
		updatedAt: TIMESTAMP,
	},
	{ id: 'educacao', name: 'Educação', color: '#F6BF26', transactionCount: 0, balance: 0, createdAt: TIMESTAMP, updatedAt: TIMESTAMP },
	{
		id: 'freelance',
		name: 'Freelance',
		color: '#C0CA33',
		transactionCount: 2,
		balance: 1250,
		createdAt: TIMESTAMP,
		updatedAt: TIMESTAMP,
	},
	{ id: 'lazer', name: 'Lazer', color: '#8E24AA', transactionCount: 6, balance: -712.4, createdAt: TIMESTAMP, updatedAt: TIMESTAMP },
	{
		id: 'mercado',
		name: 'Mercado',
		color: '#43A047',
		transactionCount: 14,
		balance: -1842.55,
		createdAt: TIMESTAMP,
		updatedAt: TIMESTAMP,
	},
	{ id: 'outros', name: 'Outros', color: '#78909C', transactionCount: 4, balance: -233.1, createdAt: TIMESTAMP, updatedAt: TIMESTAMP },
	{ id: 'salario', name: 'Salário', color: '#1E88E5', transactionCount: 1, balance: 8600, createdAt: TIMESTAMP, updatedAt: TIMESTAMP },
	{ id: 'saude', name: 'Saúde', color: '#00897B', transactionCount: 3, balance: -268, createdAt: TIMESTAMP, updatedAt: TIMESTAMP },
	{
		id: 'transporte',
		name: 'Transporte',
		color: '#FB8C00',
		transactionCount: 9,
		balance: -386.9,
		createdAt: TIMESTAMP,
		updatedAt: TIMESTAMP,
	},
];

export const MOCK_CATEGORY_BY_ID: Record<string, Category> = Object.fromEntries(
	MOCK_CATEGORIES.map((category) => [category.id, category]),
);

export const MOCK_TRANSACTIONS: Transaction[] = [
	{ id: 't1', description: 'Feira da semana', categoryId: 'mercado', value: -186.42, date: '2026-09-28' },
	{ id: 't2', description: '', categoryId: 'transporte', value: -18.9, date: '2026-09-28' },
	{ id: 't3', description: 'Uber para o aeroporto', categoryId: 'transporte', value: -42.9, date: '2026-09-26' },
	{ id: 't4', description: 'Consulta dermatologista', categoryId: 'saude', value: -180, date: '2026-09-24' },
	{ id: 't5', description: 'Projeto landing page — 2ª parcela', categoryId: 'freelance', value: 750, date: '2026-09-22' },
	{ id: 't6', description: 'Cinema e jantar', categoryId: 'lazer', value: -164.8, date: '2026-09-20' },
	{ id: 't7', description: 'Streaming e música', categoryId: 'assinaturas', value: -59.8, date: '2026-09-15' },
	{ id: 't8', description: 'Aluguel setembro', categoryId: 'aluguel', value: -2400, date: '2026-09-10' },
	{ id: 't9', description: 'Salário setembro', categoryId: 'salario', value: 8600, date: '2026-09-05' },
	{ id: 't10', description: 'Mercado do mês', categoryId: 'mercado', value: -612.35, date: '2026-09-03' },
	{ id: 't11', description: 'Aluguel agosto', categoryId: 'aluguel', value: -2400, date: '2026-08-10' },
	{ id: 't12', description: 'Salário agosto', categoryId: 'salario', value: 8600, date: '2026-08-05' },
	{ id: 't13', description: 'Mercado do mês', categoryId: 'mercado', value: -540.1, date: '2026-08-12' },
	{ id: 't14', description: 'Consulta oftalmologista', categoryId: 'saude', value: -220, date: '2026-08-18' },
	{ id: 't15', description: 'Aluguel julho', categoryId: 'aluguel', value: -2400, date: '2026-07-10' },
	{ id: 't16', description: 'Salário julho', categoryId: 'salario', value: 8600, date: '2026-07-05' },
];
