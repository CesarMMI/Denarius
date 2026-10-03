export interface Category {
	id: string;
	name: string;
	color: string;
	transactionCount: number;
	balance: number;
	createdAt: string;
	updatedAt: string;
}

export type CategoryInput = Pick<Category, 'name' | 'color'>;
