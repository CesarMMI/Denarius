export interface SortOption<T extends string> {
	label: string;
	orderBy: T;
	ascending: boolean;
}

export interface SortValue<T extends string> {
	orderBy: T;
	ascending: boolean;
}
