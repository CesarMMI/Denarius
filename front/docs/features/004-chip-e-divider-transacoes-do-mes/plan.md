# Plano: Chip colorido e divisores por dia em "Transações do mês"

**Spec**: [spec.md](./spec.md) · **Entregue em**: `2171566` · **Contrato consumido**: `color` de `back/docs/features/004-cor-categoria-transacoes-mes/contracts/monthly-transactions-api.yaml` (obrigatório, não nulo, `#RRGGBB` em maiúsculas)

## Conferência com as regras

- Organização (F1): só `reports/components/transactions-list` (apresentação, sem HTTP); reaproveita `shared/mat-chip-color`; não importa o interior de outra feature.
- Contrato (F2 e IV): consome só o que o contrato oferece; o back entregou a mudança antes. O tipo espelha o contrato.
- Rotas, banco, dependência: nenhuma mudança.
- Material (F4): `mat-chip` e tokens `--mat-*`; CSS próprio só para o chip caber na coluna.
- Segurança (II): a descrição, que vem do usuário, continua interpolada com `{{ }}`, sem `innerHTML` nem `bypassSecurityTrust*`; a cor só chega à diretiva.
- Performance (III): sem requisição nova (SC-004); o conjunto de linhas "mesmo dia" é um `computed` sobre as 10 linhas exibidas.

## Mudanças

- `transactions-list.{ts,html,scss}`: `<mat-chip [appMatChipColor]="transaction.color">` com `description ?? categoryName` no lugar do texto e da linha da categoria; as linhas seguidas do mesmo dia ganham a classe `same-date`, que tira o divisor, como em `transactions-table`.
- `reports/types/report.ts`: `MonthlyTransaction.color: string`.
- `reports/testing/report-fixtures.ts`: `buildMonthlyTransaction` ganha `color: '#43A047'` por padrão.
- `docs/design.md` não muda: o chip segue o padrão já documentado.

## Decisões

Detalhes e alternativas descartadas em [research.md](./research.md).

- **Chip**: reaproveitar `MatChipColor`, o mesmo visual da tabela de Transações; `description ?? categoryName` basta porque o back normaliza descrição em branco para `null`.
- **Corte (FR-006)**: texto em `span.label` dentro do chip, com as regras de reticências no `.label`; o jsdom não mede layout, então o teste confere só a estrutura e o corte é verificação visual.
- **Divisor (FR-005)**: `computed` `sameDate` sobre `latest()`, pelo `date.slice(0, 10)` (data exibida em UTC); a regra não é extraída para `shared/`, para não mexer em `transactions-table` (FR-008).
- **Tipo**: `color: string` não nulo, como no contrato.

## Testes previstos

Em `transactions-list.spec.ts`, escritos antes do código e vistos falhar; o fixture ganha `color` primeiro.

1. Chip por linha: o texto é a descrição, ou o nome da categoria sem descrição.
2. Nenhum elemento `.category` na coluna de descrição.
3. Cor (SC-001): categorias diferentes dão estilos diferentes, comparando com uma instância de `MatChipColor` da mesma cor (a diretiva grava `--mat-chip-*` em `light-dark(...)`, não o `#RRGGBB`); mesma categoria, mesmo estilo.
4. Divisor: `same-date` entre linhas do mesmo dia (inclusive com horários diferentes); ausente entre dias diferentes; uma linha sem `same-date`; só as 10 linhas exibidas.
5. Corte: o texto está em `span.label` dentro do chip.
6. Mantidos: ordem da API, valores com sinal, título com contagem, limite de 10, "Ver todas" e os estados vazio, de carga e de erro. Só os testes de `column('description')` mudam, porque a linha da categoria saiu.
7. `reports-page.spec.ts` continua passando com o fixture novo.

## Verificação

`npx ng test --watch=false`, `npm run lint` e `npm run build` sem erro nem estouro de budget. Visual, com a API no ar: chip na cor da categoria, sem a linha menor; divisor só entre dias; descrição de 255 caracteres cortada dentro do chip, sem rolagem horizontal (SC-003). SC-004: a aba de rede mostra as mesmas requisições de antes.
