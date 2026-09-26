import { Transaction } from './transaction';

export type TransactionInput = Pick<Transaction, 'description' | 'categoryId' | 'value' | 'date'>;

export type TransactionFormResult = TransactionInput & Partial<Pick<Transaction, 'id'>>;
