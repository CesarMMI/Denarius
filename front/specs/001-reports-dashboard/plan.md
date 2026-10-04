# Plano de implementação: Painel de relatórios

**Branch**: `001-reports-dashboard` | **Data**: 2026-10-03 | **Spec**: [spec.md](./spec.md)

**Entrada**: Especificação da feature em `/specs/001-reports-dashboard/spec.md`

## Resumo

Uma nova feature `reports/`, carregada sob demanda, com as pastas que as outras features usam. O `ReportsService` tem
um método por relatório do backend, cada um devolvendo a requisição `{ url, params }` que um `httpResource` recebe. A
`ReportsPage` é dona do signal do mês e dos cinco `httpResource`s, e dispõe os blocos numa grade CSS que ocupa a altura
do conteúdo da sidenav. Cada bloco é um componente de apresentação que recebe o seu próprio `Resource` e emite
`retry`: `summary-cards`, `expenses-by-category-chart`, `income-vs-expense-chart`, `cumulative-comparison-chart` e
`transactions-list`. Um pequeno `report-card` dá a todos o mesmo card, título e estados de carregamento / vazio / erro
com nova tentativa. Os gráficos usam ng2-charts (Chart.js), adicionado com `ng add ng2-charts` e provido na rota lazy,
para que o bundle inicial não mude. As cores dos gráficos vêm dos tokens do tema do Material, resolvidas para o esquema
de cores em uso: despesa `error`, receita `tertiary` (o verde do tema), "Outras" `outline` — veja pesquisa → Cores.

## Contexto técnico

**Linguagem/versão**: TypeScript 5.9 / Angular 21.2 (standalone, zoneless, signals)

**Dependências principais**: Angular Material / CDK 21.2; **ng2-charts 10.0.0** + **chart.js 4.x**, adicionados por
`ng add ng2-charts` — o ng2-charts mais recente compatível com o Angular 21, já que o 11.x exige o Angular 22

**Armazenamento**: N/A (lê a API de relatórios do backend)

**Testes**: Vitest + jsdom via `@angular/build:unit-test`; `HttpTestingController` para o service e a página;
`resourceFromSnapshots` para os componentes dos blocos; uma diretiva fake `canvas[baseChart]` nos specs, para que o
Chart.js nunca desenhe no jsdom

**Plataforma-alvo**: Navegadores de desktop (recolher a grade em telas estreitas é o único comportamento responsivo
que a spec pede)

**Tipo de projeto**: web — a metade frontend de uma aplicação web de dois projetos; este plano cobre `front/`

**Metas de desempenho**: O bundle inicial mantém o tamanho atual (~630 kB, aviso do budget em 700 kB): o Chart.js só
carrega com a rota de relatórios (SC-005)

**Restrições**: `.specify/memory/constitution.md` (veja o Constitution Check); componentes do Material e tokens
`--mat-sys-*` em primeiro lugar; estilos de componente abaixo do budget de 4 kB; `ng lint` e Prettier

**Escala/escopo**: Uma rota, uma página, seis componentes de apresentação, um service de API, um service de tema dos
gráficos, os tipos dos cinco relatórios

## Constitution Check

_PORTÃO: precisa passar antes da pesquisa da Fase 0. Conferir de novo após o design da Fase 1._

| Princípio                                 | Status | Evidência                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| ----------------------------------------- | ------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| I. Compatibilidade da API pública         | PASSA  | Consome `GET /api/reports/{summary,expensesByCategory,incomeVsExpense,cumulativeExpenses,transactions}` exatamente como `back/specs/003-financial-reports/contracts/reports-api.yaml` os define; `types/report.ts` espelha esse contrato. A mudança na API foi pedida pelo dono nesta mesma tarefa e é entregue antes em `back/`, sob a constituição do backend (aditiva). A nova rota `/reports` não renomeia nada.                                                                                                                                                                                          |
| II. Respeito às fronteiras de serviço     | PASSA  | Tudo fica em `front/src/app/reports/`, com `pages/ components/ services/ types/ testing/`. URLs e parâmetros só em `services/reports.service.ts`; a `ReportsPage` é dona dos cinco `httpResource`s; os componentes dos blocos são de apresentação (um input `Resource` e um output `retry`, sem HTTP). As peças compartilhadas vêm de `shared/` (`page-header`, `month-field`, `date-utils`); nada interno de outra feature é importado. A feature é somente leitura, então, do padrão estabelecido, ela usa página + componentes + service + tipos + rotas e não tem filtros, formulário nem tabela de CRUD. |
| III. Disciplina de rollback de migrations | PASSA  | Nenhuma dependência de esquema: a única migration do backend (`AddTransactionDateIndex`, com um `Down()` verificado) adiciona um índice, e a página funciona igual antes e depois dela.                                                                                                                                                                                                                                                                                                                                                                                                                       |
| IV. Verificação da suíte de testes        | PASSA  | Um `*.spec.ts` ao lado de cada arquivo novo (service, página, seis componentes, service de tema, acréscimos ao `DateUtils`) e o `app.spec.ts` atualizado; `npm test`, `npm run lint` e `npm run build` precisam passar, e o `dotnet test` para a metade `back/`.                                                                                                                                                                                                                                                                                                                                              |
| Restrições de arquitetura                 | PASSA  | Cards, spinner, botões, tabela e ícones do Material, tokens `--mat-sys-*` para todas as cores (resolvidos para cores concretas no canvas). Recolher numa única coluna está no escopo porque a spec pede (FR-011). O Chart.js fica fora do bundle inicial (veja pesquisa → Onde o Chart.js é provido).                                                                                                                                                                                                                                                                                                         |

Nenhuma violação — o Acompanhamento de complexidade não é necessário.

_Conferido de novo após o design da Fase 1: sem mudança._

## Estrutura do projeto

### Documentação (desta feature)

```text
specs/001-reports-dashboard/
├── plan.md              # Este arquivo (saída do comando /speckit-plan)
├── research.md          # Saída da Fase 0 (comando /speckit-plan)
├── data-model.md        # Saída da Fase 1 (comando /speckit-plan)
├── quickstart.md        # Saída da Fase 1 (comando /speckit-plan)
├── contracts/           # Saída da Fase 1 (comando /speckit-plan)
│   └── reports-ui.md
└── tasks.md             # Saída da Fase 2 (comando /speckit-tasks - NÃO é criado pelo /speckit-plan)
```

### Código-fonte (raiz do repositório)

```text
src/app/
├── app.routes.ts                     # + /reports lazy
├── app.ts / app.spec.ts              # + link "Relatórios" na sidenav (por último)
├── shared/date-utils/                # + toMonthKey, currentMonth (São Paulo)
└── reports/
    ├── reports.routes.ts             # página + provideCharts(withDefaultRegisterables())
    ├── types/report.ts               # os formatos dos cinco relatórios
    ├── services/
    │   ├── reports.service.ts        # uma requisição por endpoint
    │   └── chart-theme.service.ts    # cores do tema para o canvas, por esquema de cores
    ├── testing/
    │   ├── report-fixtures.ts        # fixtures build*
    │   └── fake-chart.ts             # substitui o canvas[baseChart] nos specs
    ├── pages/reports-page/           # mês + 5 httpResources + grade bento
    └── components/
        ├── report-card/              # card, título, estados de carregamento/vazio/erro
        ├── summary-cards/
        ├── expenses-by-category-chart/
        ├── income-vs-expense-chart/
        ├── cumulative-comparison-chart/
        └── transactions-list/
```

**Decisão de estrutura**: Uma pasta de feature como `categories/` e `transactions/`, com rota lazy a partir do
`app.routes.ts`. Os specs ficam ao lado dos seus arquivos.

## Acompanhamento de complexidade

Não se aplica — o Constitution Check não apontou violações.
