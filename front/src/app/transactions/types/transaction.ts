export interface Transaction {
	id: string;
	description: string | null;
	categoryId: string;
	value: number;
	date: string;
	createdAt: string;
	updatedAt: string;
}

export type TransactionInput = Pick<Transaction, 'description' | 'categoryId' | 'value' | 'date'>;
