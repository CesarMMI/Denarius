export interface Category {
	id: string;
	name: string;
	color: string;
	transactionCount: number;
	balance: number;
	canDelete: boolean;
	createdAt: string;
	updatedAt: string;
}

export type CategoryInput = Pick<Category, 'name' | 'color'>;
