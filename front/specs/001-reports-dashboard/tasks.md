---
description: 'Lista de tarefas da feature Painel de relatórios'
---

# Tarefas: Painel de relatórios

**Entrada**: Documentos de design em `/specs/001-reports-dashboard/`

**Pré-requisitos**: plan.md (presente), spec.md (presente), research.md (presente),
data-model.md (presente), contracts/ (presente)

**Testes**: Exigidos pela constituição (Princípio IV: um `*.spec.ts` ao lado de cada arquivo novo) e pelo pedido
("lint e testes existentes passando"). Em cada história, o spec é escrito junto com o código que ele cobre, no modelo
da skill `write-front-tests`.

**Organização**: As tarefas estão agrupadas por história de usuário, na ordem de prioridade do `spec.md` (P1–P6).

## Formato: `[ID] [P?] [História] Descrição`

- **[P]**: Arquivos diferentes, sem dependência de uma tarefa incompleta
- **[História]**: A história de usuário a que a tarefa pertence (US1–US6)
- Os caminhos de arquivo são exatos, relativos a `front/`

## Fase 1: Preparação (infraestrutura compartilhada)

- [x] T001 Rodar `npx ng test --watch=false`, `npm run lint` e `npm run build` no front intocado e registrar a linha
      de base (testes, lint, "Initial total").
- [x] T002 Rodar `npx ng add ng2-charts --skip-confirmation`; conferir que o `package.json` ganha `ng2-charts` (10.x,
      a linha do Angular 21) e `chart.js` (4.x) e o que o schematic acrescentou a `src/app/app.config.ts`.

---

## Fase 2: Fundação (pré-requisitos bloqueantes)

**⚠️ CRÍTICO**: Nenhuma história de usuário pode ser concluída antes desta fase.

- [x] T003 Mover `provideCharts(withDefaultRegisterables())` de `src/app/app.config.ts` para os `providers` da rota
      da página em `src/app/reports/reports.routes.ts`, com a fonte padrão do Chart.js definida como Roboto;
      carregá-la sob demanda a partir de `src/app/app.routes.ts` (`path: 'reports'`, `loadChildren`).
- [x] T004 [P] Tipos dos cinco relatórios — `MonthlySummary`, `PreviousMonthSummary`, `ExpensesByCategory`,
      `CategoryExpense`, `IncomeVsExpense`, `CumulativeExpenseComparison`, `AccumulatedExpense`, `MonthlyTransaction`
      (`type: 'in' | 'out'`) — em `src/app/reports/types/report.ts`.
- [x] T005 [P] `DateUtils.toMonthKey(date)` → `YYYY-MM` e `DateUtils.currentMonth()` → dia 1 do mês atual em
      America/Sao_Paulo, com testes (incluindo 02:00 UTC de 1º de outubro ainda ser setembro) em
      `src/app/shared/date-utils/date-utils.ts` e `date-utils.spec.ts`.
- [x] T006 `ReportsService` — `summary`, `expensesByCategory`, `incomeVsExpense(month, months?)`,
      `cumulativeExpenses`, `transactions`, cada um devolvendo `{ url, params }` com `month=YYYY-MM` só quando um mês
      é informado — e o seu spec em `src/app/reports/services/reports.service.ts` e `reports.service.spec.ts`
      (depende de T005).
- [x] T007 [P] `ChartThemeService` — `colors()` resolvendo `error`, `outline`, `outline-variant`,
      `on-surface-variant` e `surface-container-low` por meio de um elemento de sondagem, recalculado quando
      `prefers-color-scheme` muda — e o seu spec em `src/app/reports/services/chart-theme.service.ts` e
      `chart-theme.service.spec.ts`.
- [x] T008 [P] `report-card` — `mat-card` com título (e subtítulo opcional), spinner enquanto carrega sem dados,
      conteúdo esmaecido enquanto recarrega com dados, texto de erro + "Tentar novamente" (`retry`), texto de vazio e,
      nos demais casos, o `ng-template` do conteúdo — e o seu spec em `src/app/reports/components/report-card/`.
- [x] T009 [P] Utilitários de teste: fixtures `build*` dos cinco relatórios em
      `src/app/reports/testing/report-fixtures.ts` e uma diretiva fake `canvas[baseChart]` (inputs `type`, `data`,
      `options`) em `src/app/reports/testing/fake-chart.ts`.

**Ponto de controle**: A rota, o acesso aos dados e a casca do card existem.

---

## Fase 3: História de usuário 1 - Ver como está o mês atual (Prioridade: P1) 🎯 MVP

**Objetivo**: "Relatórios" no menu abre `/reports` no mês atual, com os cinco cards de resumo.

**Teste independente**: passo 2.1 do quickstart.md.

- [x] T010 [US1] `summary-cards` — cinco `mat-card`s (Saldo, Receitas, Despesas, Taxa de poupança, Projeção) com
      valores em BRL, despesas e saldos negativos em vermelho, variações como ↑/↓ + percentual +
      "vs. {mês anterior por extenso}" na cor boa (`tertiary`) ou ruim (`error`) conforme a métrica,
      "Sem comparação com {mês}" para uma variação nula, "—" para uma taxa de poupança nula; fora isso, um
      `report-card` com o estado do resumo ("Sem movimentações neste mês." quando receita e despesa são 0) — e o seu
      spec, em `src/app/reports/components/summary-cards/`.
- [x] T011 [US1] `ReportsPage` — cabeçalho da página "Relatórios" com "Recarregar" e o campo de mês, os cinco
      `httpResource`s sobre `month()`, a grade bento (áreas, `height: 100%`, media query de coluna única) com o
      `summary-cards` ligado — e o seu spec (primeiras requisições com o mês de São Paulo, spinner, cards, erro e nova
      tentativa de um bloco, recarga de todos) em `src/app/reports/pages/reports-page/`.
- [x] T012 [US1] Adicionar `{ route: '/reports', label: 'Relatórios', icon: 'insights' }` por último em `links` em
      `src/app/app.ts`, e o novo link às expectativas de `src/app/app.spec.ts`.

**Ponto de controle**: A página abre pelo menu com o resumo — o MVP.

---

## Fase 4: História de usuário 2 - Ver outro mês (Prioridade: P2)

**Objetivo**: O campo de mês recarrega todos os blocos; a nova tentativa de um bloco recarrega só aquele bloco.

**Teste independente**: passos 2.4 e 2.6 do quickstart.md.

- [x] T013 [US2] Spec da página: escolher um mês pelo model do campo de mês envia `month=YYYY-MM` aos cinco
      endpoints; o `retry` de um bloco refaz só a requisição do seu endpoint — em
      `src/app/reports/pages/reports-page/reports-page.spec.ts` (estende a T011 à medida que cada bloco é ligado).

---

## Fase 5: História de usuário 3 - Ver para onde foi o dinheiro (Prioridade: P3)

- [x] T014 [US3] `expenses-by-category-chart` — `report-card` "Despesas por categoria" (subtítulo: o total do mês em
      BRL), uma rosca com as cores das categorias, "Outras" em `outline`, um espaço de 2 px na cor da superfície do
      card, a legenda do próprio Chart.js com cada participação ("Mercado (23,08%)"), tooltip acrescentando o valor em
      BRL, `aria-label`; "Sem despesas neste mês." quando vazio — e o seu spec (dados, cores, legenda, tooltip,
      estados, nova tentativa) em `src/app/reports/components/expenses-by-category-chart/`; ligá-lo na página e no
      spec dela.

---

## Fase 6: História de usuário 4 - Comparar receitas e despesas ao longo dos meses (Prioridade: P4)

- [x] T015 [US4] `income-vs-expense-chart` — `report-card` "Receitas vs. despesas" (subtítulo "12 meses até {mês}"),
      colunas agrupadas (Receitas `outline`, Despesas `error`, no máximo 24 px, topos arredondados de 4 px), rótulos
      do eixo x sem inclinação ("set.", o ano abaixo do primeiro mês e de cada janeiro), eixo y em BRL compacto,
      tooltip em modo índice com o mês por extenso e os dois valores em BRL, `aria-label`; "Sem movimentações neste
      período." quando todos os meses são zero — e o seu spec em
      `src/app/reports/components/income-vs-expense-chart/`; ligá-lo na página e no spec dela.

---

## Fase 7: História de usuário 5 - Acompanhar o ritmo de gastos (Prioridade: P5)

- [x] T016 [US5] `cumulative-comparison-chart` — `report-card` "Despesas acumuladas", linhas sobre os dias 1…max dos
      dois meses (mês atual `error`, anterior `outline`, 2 px, sem marcadores), legenda com os dois meses por extenso
      (a partir do input `month`), eixo y em BRL compacto, tooltip em modo índice "Dia N" com os dois valores em BRL,
      `aria-label`; "Sem despesas neste mês nem no anterior." quando as duas séries são zero — e o seu spec em
      `src/app/reports/components/cumulative-comparison-chart/`; ligá-lo na página e no spec dela.

---

## Fase 8: História de usuário 6 - Conferir os detalhes (Prioridade: P6)

- [x] T017 [US6] `transactions-list` — `report-card` "Transações do mês" (subtítulo: quantas), uma `mat-table` com
      cabeçalho fixo e rolagem própria: Data (`dd/MM`, UTC), Descrição (com a categoria abaixo), Valor (BRL, `-` e
      `.negative` para `out`, alinhado ao fim); "Sem movimentações neste mês." quando vazia — e o seu spec em
      `src/app/reports/components/transactions-list/`; ligá-la na página e no spec dela.

---

## Fase 9: Acabamento e aspectos transversais

- [x] T018 `npx prettier --write` nos arquivos alterados; `npx ng test --watch=false`, `npm run lint`,
      `npm run build` — todos passam (27 arquivos de spec, 199 testes), sem aviso de budget; "Initial total" de
      633,45 kB contra 630,52 kB na T001, pela redivisão, feita pelo esbuild, dos chunks compartilhados de
      Angular/Material, sem código de gráfico nem de relatórios no grafo inicial (conferido com `--stats-json`).
- [x] T019 Passo 2 do quickstart.md contra o banco descartável do backend: renderizar a página (desktop e estreita) e
      conferir os números, as cores, os estados e o layout; corrigir o que as capturas de tela mostrarem. Renderizado
      no Chrome headless pelo protocolo do DevTools em 1440×900 (claro e escuro), 1366×768 e 1024 px de largura, para
      outubro (atual), setembro (oito categorias) e março (sem movimentação): os números batem com o quickstart do
      backend, sem rolagem da página no desktop, coluna única abaixo de 1200 px. Corrigido: a rosca de volta na área
      alta (a legenda dela cortava categorias num bloco de uma linha), a tabela de transações transbordando em
      1366 px (a descrição agora trunca), rótulos do resumo em duas linhas (estilo de rótulo pequeno), rótulos de eixo
      inclinados.
- [x] T020 [P] Marcar esta lista de tarefas como concluída e registrar o que a validação encontrou (research.md →
      Achados da validação). "Initial total": 630,52 kB antes, 633,45 kB depois (sem aviso de budget; nenhum código
      de Chart.js, ng2-charts ou relatórios nos chunks iniciais — 800,33 kB com o provider do schematic deixado em
      `app.config.ts`).

---

## Dependências e ordem de execução

- **Preparação (Fase 1)** → **Fundação (Fase 2)** → histórias na ordem de prioridade → **Acabamento**.
- A US1 cria a página; a US2 estende o spec da página; as US3–US6 acrescentam, cada uma, um bloco à página (template
  e spec), então mexem em `reports-page.*` uma de cada vez, mesmo com componentes independentes.
- T004, T005, T007, T008 e T009 mexem em arquivos diferentes e podem ser escritas juntas.

## Exemplo de paralelismo: Fundação

```bash
Task: "Tipos em src/app/reports/types/report.ts"
Task: "DateUtils.toMonthKey/currentMonth em src/app/shared/date-utils/"
Task: "ChartThemeService em src/app/reports/services/chart-theme.service.ts"
Task: "report-card em src/app/reports/components/report-card/"
Task: "Fixtures e gráfico fake em src/app/reports/testing/"
```

## Estratégia de implementação

MVP = Preparação + Fundação + US1 (a página com os cards de resumo, aberta pelo menu). Depois, cada história
acrescenta um bloco, testável sozinho, e o Acabamento roda as suítes completas, o budget do build e a verificação
visual.

## Fase 10: Convergência

Os ajustes do dono antes de terminar (spec.md → Esclarecimentos, Sessão 2026-10-03). Uma linha de saldo sobre as
barras foi construída e descartada a pedido do dono; as barras não mudaram.

- [x] T021 Reduzir a rosca a no máximo cinco fatias — as quatro maiores e "Outras" com o resto, a sua participação
      recalculada a partir do `total`, na cor neutra (`ChartColors.neutral`, `outline`) — com specs em
      `src/app/reports/components/expenses-by-category-chart/` e `src/app/reports/services/chart-theme.service.*`
      conforme FR-004, US3/AC2 (partial)
- [x] T022 Adicionar `DateUtils.fromMonthKey` e ler `?month=YYYY-MM` na `TransactionsPage`, abrindo os filtros
      naquele mês, com specs em `src/app/shared/date-utils/` e `src/app/transactions/pages/transactions-page/`
      conforme FR-013, US6/AC3 (missing)
- [x] T023 Listar as dez transações mais recentes sob um título com a contagem do mês e ligar "Ver todas" a
      `/transactions?month=YYYY-MM` (novo input `month`, ligado na página), com specs em
      `src/app/reports/components/transactions-list/` e na página conforme FR-007, US6/AC1, US6/AC3 (partial)
- [x] T024 Cobrir o "Mês anterior" / "Próximo mês" do dono — `DateUtils.previousMonth`/`nextMonth` e uma requisição
      por bloco a partir do cabeçalho da página — em `date-utils.spec.ts` e `reports-page.spec.ts` conforme FR-002,
      US2/AC3, Constitution IV (missing)
- [x] T025 Alinhar os specs que se desviaram das mudanças posteriores do dono, verificando o novo comportamento:
      ordem do menu (Relatórios primeiro), despesas como valores negativos sem vermelho nos cards de resumo, sem
      subtítulos nos cards, ticks de valor `count: 5`, o espaço de 4 px da rosca, cores dos gráficos (receita
      `tertiary`, linhas `primary` e `on-primary`, texto `on-surface`) conforme Constitution IV (contradicts)
- [x] T026 `npx prettier --write` nos arquivos alterados; `npx ng test --watch=false` (27 arquivos, 223 testes),
      `npm run lint` e `npm run build` passam — "Initial total" de 633,08 kB, sem aviso de budget (polish)
