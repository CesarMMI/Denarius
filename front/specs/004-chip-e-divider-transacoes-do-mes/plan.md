# Plano de implementação: Chip colorido e divisores por dia em "Transações do mês"

**Branch**: `main` | **Data**: 2026-11-07 | **Spec**: [spec.md](./spec.md)

## Resumo

Em `reports/components/transactions-list` (só apresentação, sem HTTP): a coluna de descrição passa a mostrar um
`mat-chip` com `appMatChipColor` (a diretiva de `shared/mat-chip-color`, a mesma da tabela de Transações) em vez do
texto e da linha com o nome da categoria; e as linhas seguidas do mesmo dia ganham a classe `same-date`, que tira o
divisor, como em `transactions-table`. O tipo `MonthlyTransaction` ganha `color: string`, espelhando o contrato do
back (back 004, concluída). Sem rota, serviço, dependência ou mudança no back. Ver [research.md](./research.md).

## Contexto técnico

- **Stack**: Angular 21 (standalone, signals), Angular Material, Vitest (`npx ng test --watch=false`).
- **Contrato consumido**: `back/specs/004-cor-categoria-transacoes-mes/contracts/monthly-transactions-api.yaml`: `color` obrigatório, não nulo, `#RRGGBB` em maiúsculas.
- **Metas**: SC-004 (nenhuma requisição a mais) e SC-003 (sem rolagem horizontal; verificação visual, [quickstart.md](./quickstart.md)).
- **Escopo**: só `front/`; a tabela de Transações e o back ficam como estão (FR-008).

## Constitution Check

| Regra | Situação |
| --- | --- |
| I. TDD | Testes primeiro em `transactions-list.spec.ts` e no fixture, vistos falhar antes do código; nenhum teste apagado ou enfraquecido (os de `column('description')` mudam de expectativa porque o requisito mudou: sem a linha da categoria; os demais seguem iguais) |
| II. Segurança | O texto (descrição, vinda do usuário) continua interpolado com `{{ }}`, sem `innerHTML` nem `bypassSecurityTrust*`; a cor só chega à diretiva, que a lê como `#RRGGBB` e a transforma em tons |
| III. Performance | Sem requisição nova (SC-004); o conjunto de linhas "mesmo dia" é um `computed` sobre as 10 linhas exibidas |
| IV. Contrato | Consome só o que o contrato oferece (`color`); o back entregou a mudança antes (ciclo próprio) |
| V. Escopo | Só o `transactions-list`, o tipo e o fixture; sem abstração nova (ver research: divisor) |
| F1. Organização | Componente de apresentação, sem HTTP; reaproveita `shared/mat-chip-color`; sem importar o interior de outra feature |
| F2. Contrato e rotas | Tipo espelha o contrato; nenhuma rota muda |
| F3. Sem banco | Não se aplica |
| F4. Material primeiro | `mat-chip` e tokens `--mat-*`; CSS próprio só para o chip caber na coluna |

Sem violações; sem Complexity Tracking.

## Testes (TDD, nesta ordem; viram as tasks de teste)

Em `transactions-list.spec.ts`, escritos e vistos falhar antes do código; o fixture ganha `color` primeiro:

1. Chip por linha: cada descrição está num `mat-chip`; o texto é a descrição, ou o nome da categoria quando não há descrição.
2. Sem a linha da categoria: nenhum elemento `.category` na coluna de descrição.
3. Cor (SC-001): duas categorias de cores diferentes dão chips com estilo diferente, comparando o estilo do chip com o de uma instância de `MatChipColor` da mesma cor (a diretiva grava `--mat-chip-*` em `light-dark(...)`, não o `#RRGGBB`); duas linhas da mesma categoria têm o mesmo estilo.
4. Divisor: `same-date` entre linhas do mesmo dia, inclusive com horários diferentes; ausente entre dias diferentes; uma única linha sem `same-date`; só considera as 10 linhas exibidas.
5. Corte (FR-006): o texto vai num `span.label` dentro do chip (o elemento que recebe as regras de reticências); o efeito visual é do quickstart.
6. Mantidos sem alteração: ordem da API, valores com sinal, título com contagem, limite de 10, link "Ver todas", estados vazio, de carga e de erro; os de `column('description')` mudam só porque a linha da categoria sai.
7. `reports-page.spec.ts` continua passando com o fixture novo.

## Estrutura

```text
front/specs/004-chip-e-divider-transacoes-do-mes/   plan.md, research.md, data-model.md, quickstart.md
front/src/app/reports/types/report.ts                                  # MonthlyTransaction.color
front/src/app/reports/testing/report-fixtures.ts                       # buildMonthlyTransaction com color
front/src/app/reports/components/transactions-list/transactions-list.{ts,html,scss,spec.ts}
```
