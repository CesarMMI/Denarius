import { Transaction } from './transaction';

export type TransactionFormResult = Pick<Transaction, 'description' | 'categoryId' | 'value' | 'date'> &
	Partial<Pick<Transaction, 'id'>>;
