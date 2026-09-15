export function formatMoney(value: number): string {
	const amount = Math.abs(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
	return value < 0 ? `− ${amount}` : amount;
}

export function formatSignedMoney(value: number): string {
	const amount = Math.abs(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
	return value < 0 ? `− ${amount}` : `+ ${amount}`;
}
