Consumidor
```typescript
type LoginForm = {
	name: string | null;
	age: number | null;
};
// Esse parametro é do tipo LoginAppForm
loginForm = appForm<LoginForm>({
    name: { value: null, label: 'Name' },
	age: { value: 18, label: 'Age' },
});
```
---
Tipos internos de appForm
```typescript
// LoginAppForm não é definido em nenhum lugar explicitamente, deve vir do T de appForm<T>()
type LoginAppForm = {
    name: AppFormField<string>;
	age: AppFormField<number>;
};
type AppFormField<T> = {
    value: T | null;
	label: string;
};
```
---
Futuramente quero ter outros tipos de fields, como:
```typescript
type AppSelectField<T> = AppFormField<T> & {
    multiple?: boolean;
	options: { value: T | null; label: string }[];
};
```
