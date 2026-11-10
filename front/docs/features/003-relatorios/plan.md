# Plano: Painel de relatórios

**Spec**: [spec.md](./spec.md) · **Entregue em**: `257afad` · **Contrato consumido**: [back/docs/features/003-financial-reports/contracts/reports-api.yaml](../../../../back/docs/features/003-financial-reports/contracts/reports-api.yaml)

## Conferência com as regras

- Organização (F1): feature nova `src/app/reports/` com `pages/ components/ services/ types/ testing/`; URLs e parâmetros só em `services/reports.service.ts`; a `ReportsPage` é dona dos cinco `httpResource`s e os blocos são de apresentação (input `Resource`, output `retry`, sem HTTP). Peças compartilhadas vêm de `shared/` (`page-header`, `month-field`, `date-utils`).
- Contrato (F2 e IV): consome `GET /api/reports/{summary,expensesByCategory,incomeVsExpense,cumulativeExpenses,transactions}` como o contrato define; `types/report.ts` o espelha. A mudança na API foi pedida junto e entregue antes no back, de forma aditiva.
- Rotas (F2): `/reports` é nova, lazy a partir de `app.routes.ts`; nada é renomeado.
- Banco (F3): nenhuma dependência de schema (a migration `AddTransactionDateIndex` do back só adiciona um índice).
- Material (F4): `mat-card`, spinner, botões, tabela e ícones, tokens `--mat-sys-*`; cores dos gráficos resolvidas desses tokens para o esquema em uso. Código próprio: a grade CSS e o `report-card`.
- Segurança (II): nomes de categoria interpolados com `{{ }}`, sem `innerHTML` nem `bypassSecurityTrust*`.
- Performance (III): SC-005; o Chart.js só carrega com a rota (bundle inicial ~630 kB, aviso do budget em 700 kB); estilos de componente abaixo do budget de 4 kB.
- Dependência nova: `ng2-charts@10.0.0` e `chart.js` 4.x, pedidos para a feature, adicionados por `ng add ng2-charts`; é a versão mais recente compatível com o Angular 21 (a 11.x exige o 22).

## Mudanças

- **Telas e componentes**: `reports/pages/reports-page` (mês + cinco `httpResource`s + grade) e, em `components/`, `report-card` (card, título e estados de carregamento/vazio/erro), `summary-cards`, `expenses-by-category-chart`, `income-vs-expense-chart`, `cumulative-comparison-chart` e `transactions-list`. `reports.routes.ts` provê `provideCharts(withDefaultRegisterables())`.
- **Serviços e tipos**: `ReportsService` (um método por relatório, devolvendo `{ url, params }`), `ChartThemeService` (cores do tema por esquema de cores), `types/report.ts`.
- **Fora de `reports/`**: link "Relatórios" por último na sidenav (`app.ts`); `toMonthKey` e `currentMonth` (São Paulo) em `shared/date-utils`; `?month=YYYY-MM` na página de transações (FR-013).
- **`docs/design.md`**: registrar o painel (grade bento, cores dos gráficos, estados por bloco).

## Decisões

Detalhes e alternativas em [research.md](./research.md).

- **Chart.js na rota lazy**: o provider sai do `app.config.ts` (onde o schematic o põe) para `reports.routes.ts`, para não pesar o bundle inicial.
- **Um `httpResource` por bloco, na página**: cada bloco tem carregamento, erro e nova tentativa próprios (FR-008) sem HTTP em `components/` (F1).
- **Cores**: despesa `error`, receita `tertiary`, "Outras" `outline`; linhas da comparação acumulada `primary`/`secondary`, decisão do dono no bug `chart-theme-teste-tokens-desatualizados` (FR-010).
- **Rosca e lista**: a página reduz a cinco fatias e dez linhas o que a API devolve; o back não muda.

## Testes previstos

Vitest com `HttpTestingController` para service e página, `resourceFromSnapshots` para os blocos e uma diretiva fake `canvas[baseChart]` (`testing/fake-chart.ts`) para o Chart.js não desenhar no jsdom. Um `*.spec.ts` ao lado de cada arquivo novo, mais `DateUtils` e o `app.spec.ts` (link novo):

- Service e página: URLs e parâmetros por relatório; mês inicial, troca por seletor e botões, cada bloco carregado uma vez; "Tentar novamente" recarrega só o bloco (FR-001, FR-002, FR-008, SC-002, SC-003).
- Cards: valores, setas e cores semânticas, taxa sem receitas, sem comparação (FR-003, FR-009).
- Gráficos: no máximo cinco fatias e "Outras" (FR-004); doze meses com zeros (FR-005); séries da linha (FR-006); cores (FR-010, SC-004); tooltips em BRL.
- Lista: dez linhas, ordem, título com a contagem, "Ver todas" com `?month=` (FR-007, FR-013).
- Estados vazio, de carga e de erro de cada bloco; `ChartThemeService` nos dois esquemas.

## Verificação

`npx ng test --watch=false`, `npm run lint` e `npm run build` sem erro nem aviso de budget; o "Initial total" fica em ~630 kB. Manual, com a API de relatórios no ar e dados de vários meses (seed do quickstart do back): a página abre no mês atual com os cinco cards; barras com doze meses e meses vazios; linha do mês parando em hoje; "Mês anterior" recarrega cada bloco uma vez e "Ver todas" abre as transações filtradas; um mês sem dados mostra os estados vazios; com a API parada, cada bloco mostra seu erro e só o bloco tentado recarrega; em 1366×768 nada rola além da lista, abaixo de 1200 px a grade empilha, e o modo escuro redesenha os gráficos.
