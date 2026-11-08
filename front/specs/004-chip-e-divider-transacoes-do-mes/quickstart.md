# Quickstart: Chip colorido e divisores por dia em "Transações do mês"

Pré-requisito: de dentro de `front/`, `npm ci` feito.

1. `npx ng test --watch=false`: a suíte inteira passa, incluindo os testes novos de `transactions-list` (cor do chip por linha, texto do chip, ausência da linha com a categoria, `same-date` só entre linhas do mesmo dia, uma linha sem divisor).
2. `npm run lint` e `npm run build` sem erros nem estouro de budget.
3. Visual, com a API no ar (back 004 aplicada) e dados de teste: o painel de relatórios mostra cada descrição num chip da cor da categoria, sem a linha menor com o nome da categoria; transações do mesmo dia sem divisor entre si e com divisor entre dias; uma descrição longa (255 caracteres) cortada por reticências dentro do chip, sem rolagem horizontal (SC-003).
4. SC-004: a aba de rede do navegador mostra as mesmas requisições do painel de antes da mudança.
