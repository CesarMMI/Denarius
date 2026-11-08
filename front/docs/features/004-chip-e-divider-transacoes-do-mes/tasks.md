# Tarefas: Chip colorido e divisores por dia em "Transações do mês"

**Entrada**: [spec.md](./spec.md), [plan.md](./plan.md) (seção "Testes"), [research.md](./research.md), [data-model.md](./data-model.md)

**Testes**: obrigatórios (TDD, princípio I): cada teste é escrito e visto falhar antes do código que ele cobre. Comandos em `front/`: `npx ng test --watch=false`, `npm run lint`, `npm run build`.

Arquivos de `front/src/app/reports/`: `types/report.ts`, `testing/report-fixtures.ts`, `components/transactions-list/transactions-list.{ts,html,scss,spec.ts}`.

## Fase 1: Base de teste

- [X] T001 Em `testing/report-fixtures.ts`, `buildMonthlyTransaction` ganha `color: '#43A047'` por padrão (`#RRGGBB` em maiúsculas, não nulo); o teste fica vermelho (erro de tipo) até o T006

## Fase 2: US1 — Reconhecer a categoria pela cor na descrição (P1)

**Objetivo**: FR-001 a FR-004 e FR-006. **Teste independente**: [quickstart.md](./quickstart.md), itens 1 e 3.

### Testes primeiro (devem falhar)

- [X] T002 [US1] Em `transactions-list.spec.ts`: cada descrição está num `mat-chip`; o texto é a descrição, ou o nome da categoria quando `description` é `null`; nenhum `.category` na coluna `description` (FR-001 a FR-003). Ajustar o teste existente "list the transactions in the API order…", cujo `['Presente', 'Presentes']` muda só porque a linha da categoria sai
- [X] T003 [US1] Em `transactions-list.spec.ts`: duas categorias de cores diferentes dão chips com estilo diferente, comparando com o de uma instância de `MatChipColor` da mesma cor (a diretiva grava `--mat-chip-*` em `light-dark(...)`, não o `#RRGGBB`), e duas linhas da mesma categoria têm o mesmo estilo (SC-001, FR-004)
- [X] T004 [US1] Em `transactions-list.spec.ts`: o texto do chip está num `span.label` dentro do `mat-chip` (a estrutura que recebe o corte por reticências, FR-006)
- [X] T005 [US1] Rodar `npx ng test --watch=false` e confirmar que T002 a T004 falham pelo motivo esperado

### Implementação

- [X] T006 [US1] `types/report.ts`: `MonthlyTransaction` ganha `color: string`, não nulo, como em `MonthlyTransactionOutput` do contrato do back (`back/specs/004-cor-categoria-transacoes-mes/contracts/monthly-transactions-api.yaml`)
- [X] T007 [US1] `transactions-list.ts` e `.html`: importar `MatChipsModule` e `MatChipColor`; a célula `description` vira `<mat-chip [appMatChipColor]="transaction.color"><span class="label">{{ transaction.description ?? transaction.categoryName }}</span></mat-chip>`, sem a linha `.category` (interpolação com `{{ }}`, sem `innerHTML`)
- [X] T008 [US1] `transactions-list.scss`: chip com `max-width: 100%`; `.label` com `min-width: 0`, `overflow: hidden`, `text-overflow: ellipsis`, `white-space: nowrap`; remover as regras `.category` e `.mat-column-description div`, que não têm mais alvo, sem usar classes internas do Material
- [X] T009 [US1] `npx ng test --watch=false`: T002 a T004 verdes, e os demais testes de `transactions-list` e `reports-page` ainda passam sem terem sido enfraquecidos

## Fase 3: US2 — Divisores só entre dias diferentes (P2)

**Objetivo**: FR-005. **Teste independente**: [quickstart.md](./quickstart.md), itens 1 e 3.

### Testes primeiro (devem falhar)

- [X] T010 [US2] Em `transactions-list.spec.ts`: a classe `same-date` fica nas linhas seguidas por outra do mesmo dia, inclusive com horários diferentes, e ausente entre dias diferentes; uma única linha não a tem; com mais de 10 transações, só as 10 exibidas contam (SC-002, como em `transactions-table.spec.ts`, linha 101)
- [X] T011 [US2] Rodar `npx ng test --watch=false` e confirmar que T010 falha pelo motivo esperado

### Implementação

- [X] T012 [US2] `transactions-list.ts`: `computed` `sameDate` sobre `latest()`, o `Set` das linhas cuja seguinte tem o mesmo `date.slice(0, 10)`; `.html`: `[class.same-date]="sameDate().has(row)"` no `tr[mat-row]`; `.scss`: `.same-date { --mat-table-row-item-outline-color: transparent; }`, como em `transactions-table`
- [X] T013 [US2] `npx ng test --watch=false`: T010 verde e a suíte inteira passando

## Fase 4: Entrega

- [X] T014 `npm run lint` e `npm run build` sem erros nem estouro de budget; `npx prettier --check` nos arquivos alterados
- [X] T015 Verificação visual do [quickstart.md](./quickstart.md), itens 3 e 4, com a API no ar: chip na cor da categoria sem a linha menor; mesmo dia sem divisor; descrição longa cortada por reticências sem rolagem horizontal; mesmas requisições de antes. Registrar o resultado no resumo, sem copiar saída
- [X] T016 Conferir com `git diff --stat` que só `front/src/app/reports/` mudou (a tabela de Transações, o back e o contrato do back intocados, FR-008)

## Dependências

- T001 antes de tudo; T002 a T004 no mesmo arquivo, em sequência; T005 depois deles.
- T006 a T008 em ordem (o tipo, o template, o estilo); T009 depois.
- A US2 só começa depois da T009 (mesmo arquivo de teste e de componente); T012 só depois de T011.
- T014 a T016 só depois da T013.

## Estratégia

MVP = US1; a US2 vem em seguida, no mesmo componente. Sem rota, serviço, dependência ou mudança no back.
