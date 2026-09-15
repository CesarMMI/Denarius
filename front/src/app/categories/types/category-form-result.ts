import { Category } from './category';

export type CategoryFormResult = Pick<Category, 'name' | 'color'> & Partial<Pick<Category, 'id'>>;
