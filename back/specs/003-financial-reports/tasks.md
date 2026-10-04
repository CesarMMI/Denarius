---

description: "Lista de tarefas da feature Relatórios financeiros"
---

# Tarefas: Relatórios financeiros

**Entrada**: Documentos de design em `/specs/003-financial-reports/`

**Pré-requisitos**: plan.md (presente), spec.md (presente), research.md (presente),
data-model.md (presente), contracts/ (presente)

**Testes**: Pedidos pela solicitação da feature — testes unitários para todos os casos de uso (mês
vazio, só receita, só despesa, virada de mês/ano, mês atual vs. passado, divisão por zero) e testes
de controller para todos os endpoints. Em cada história, os testes vêm primeiro e precisam falhar
antes de o código existir.

**Organização**: As tarefas estão agrupadas por história de usuário, na ordem de prioridade do
`spec.md` (P1–P5). Cada história acrescenta um relatório de ponta a ponta — consulta de repositório
(quando nova), records de IO, caso de uso, registro na DI, action do controller — e é testável
sozinha.

## Formato: `[ID] [P?] [História] Descrição`

- **[P]**: Arquivos diferentes, sem dependência de uma tarefa incompleta
- **[História]**: A história de usuário a que a tarefa pertence (US1–US5)
- Os caminhos de arquivo são exatos, relativos à raiz do repositório (`back/`)

## Fase 1: Preparação (infraestrutura compartilhada)

**Objetivo**: Confirmar o ponto de partida. Esta feature não adiciona nenhum projeto, pacote ou
ferramenta.

- [X] T001 Rodar `dotnet test Denarius.slnx` na solução intocada e registrar a linha de base (as
      três suítes precisam passar antes de qualquer mudança).

---

## Fase 2: Fundação (pré-requisitos bloqueantes)

**Objetivo**: O tipo de mês, o "hoje" em São Paulo e o índice de data de que todos os relatórios
dependem.

**⚠️ CRÍTICO**: Nenhuma história de usuário pode ser concluída antes desta fase.

- [X] T002 [P] `YearMonthTests` — `TryParse` aceita exatamente `yyyy-MM` (`2026-09`) e rejeita
      `2026-9`, `2026-13`, `2026-00`, `09-2026`, `2026-09-01`, `abc`, em branco e null;
      `AddMonths` atravessando anos nas duas direções; `DayCount` (30, 31, 28, 29 em 2028);
      `FromDate`; `CompareTo`; `ToString()` → `yyyy-MM` — em
      `tests/Denarius.Application.Tests/IO/Reports/YearMonthTests.cs`.
- [X] T003 [P] `readonly record struct` `YearMonth` ("um mês do calendário, guardado como o seu
      primeiro dia"): `Year`, `Month`, `FirstDay`, `DayCount`, `AddMonths(int)`,
      `FromDate(DateOnly)`, `TryParse(string?, IFormatProvider?, out YearMonth)` com
      `DateOnly.TryParseExact(value, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, ...)`,
      `IComparable<YearMonth>`, `ToString()` → `yyyy-MM` — em
      `src/Denarius.Application/IO/Reports/YearMonth.cs`.
- [X] T004 [P] `TimeProviderExtensions.GetTodayInSaoPaulo(this TimeProvider)` — `GetUtcNow()`
      convertido com `TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo")`, como
      `DateOnly` — em `src/Denarius.Application/UseCases/Reports/TimeProviderExtensions.cs`.
- [X] T005 Registrar `TimeProvider.System` com `TryAddSingleton` em
      `src/Denarius.Application/DependencyInjection.cs`.
- [X] T006 Indexar `Transactions.Date`: `builder.HasIndex(t => t.Date)` em
      `src/Denarius.Infrastructure/Persistence/Configurations/TransactionConfiguration.cs` e depois
      `dotnet ef migrations add AddTransactionDateIndex --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI`;
      conferir que o `Up()` só cria `IX_Transactions_Date` e o `Down()` só o remove
      (`src/Denarius.Infrastructure/Migrations/*_AddTransactionDateIndex.cs` e o snapshot do
      modelo).

**Ponto de controle**: Fundação pronta — todas as histórias abaixo podem começar.

---

## Fase 3: História de usuário 1 - Ver num relance como está o mês (Prioridade: P1) 🎯 MVP

**Objetivo**: `GET /api/reports/summary?month=YYYY-MM` devolve os totais do mês, a taxa de
poupança, a projeção e a comparação com o mês anterior.

**Teste independente**: Popular dois meses consecutivos, chamar o resumo do segundo e conferir cada
campo com os passos 4.1–4.4 do `quickstart.md`.

### Testes da história de usuário 1

- [X] T007 [P] [US1] `GetMonthlySummaryUseCaseTests` — mês sem dados (zeros, `SavingsRate` null,
      variações null); só receita (`SavingsRate` 100); só despesa (`SavingsRate` null, saldo
      negativo); taxa de poupança arredondada para duas casas decimais; mês passado (projeção =
      valores reais); mês atual (`ExpenseToDate / today.Day × DayCount`, arredondado;
      `ProjectedBalance` = receita − despesa projetada; intervalo da despesa até hoje
      `[day 1, today + 1)`); primeiro dia do mês atual; mês futuro (projeção 0); mês anterior e as
      suas variações, incluindo um valor anterior zero (null) e um saldo anterior negativo (medido
      pelo seu tamanho); janeiro comparado com dezembro do ano anterior (intervalo do repositório
      `[2025-12-01, 2026-02-01)`); sem mês → o mês atual em São Paulo, também quando em UTC já é o
      mês seguinte — em
      `tests/Denarius.Application.Tests/UseCases/Reports/GetMonthlySummary/GetMonthlySummaryUseCaseTests.cs`.
- [X] T008 [P] [US1] `ReportsControllerTests` do resumo — `200` com o corpo; `month=2026-09` com
      binding para `YearMonth(2026, 9)`; sem mês e com `month=` vazio → `null`; `400` sem chamar o
      caso de uso para `2026-13`, `2026-9`, `09-2026`, `2026-09-01`, `abc`; `savingsRate`
      serializado como `null` — em `tests/Denarius.WebAPI.Tests/Reports/ReportsControllerTests.cs`.

### Implementação da história de usuário 1

- [X] T009 [P] [US1] `SumByMonthAsync(DateOnly from, DateOnly to)` →
      `IEnumerable<(int Year, int Month, decimal Income, decimal Expense)>`: uma linha por mês com
      transações em `[from, to)`, receita = soma dos valores positivos, despesa = tamanho da soma
      dos valores negativos — declarado em
      `src/Denarius.Domain/Repositories/ITransactionRepository.cs` e implementado em
      `src/Denarius.Infrastructure/Repositories/TransactionRepository.cs` como uma única consulta
      com `GROUP BY` ano/mês sobre as meias-noites UTC do intervalo.
- [X] T010 [P] [US1] `GetMonthlySummaryInput` (`YearMonth? Month`), `MonthlySummaryOutput`
      (`Month`, `TotalIncome`, `TotalExpense`, `Balance`, `SavingsRate` (`decimal?`),
      `ProjectedExpense`, `ProjectedBalance`, `PreviousMonth`) e `PreviousMonthSummaryOutput`
      (`TotalIncome`, `TotalExpense`, `Balance`, `TotalIncomeChange`, `TotalExpenseChange`,
      `BalanceChange`, cada variação `decimal?`) em `src/Denarius.Application/IO/Reports/`.
- [X] T011 [US1] `IGetMonthlySummaryUseCase` e `GetMonthlySummaryUseCase` (`ITransactionRepository`,
      `TimeProvider`) — percentuais na escala 0–100 e dinheiro projetado arredondados para duas
      casas decimais com `AwayFromZero`; `SavingsRate` null sem receita; variação
      `(current − previous) / |previous| × 100`, null quando o valor anterior é 0 — em
      `src/Denarius.Application/UseCases/Reports/GetMonthlySummary/` (depende de T003, T004, T009,
      T010).
- [X] T012 [US1] Registrar `IGetMonthlySummaryUseCase` em
      `src/Denarius.Application/DependencyInjection.cs`.
- [X] T013 [US1] `ReportsController` (`[ApiController]`, `api/[controller]`) com
      `[HttpGet("summary")] Summary([FromQuery] YearMonth? month)` delegando ao caso de uso — em
      `src/Denarius.WebAPI/Controllers/ReportsController.cs` (depende de T011).

**Ponto de controle**: O resumo funciona de ponta a ponta — o MVP do painel.

---

## Fase 4: História de usuário 2 - Ver para onde foi o dinheiro (Prioridade: P2)

**Objetivo**: `GET /api/reports/expensesByCategory?month=YYYY-MM` devolve a despesa total do mês e o
valor e a participação de cada categoria, das maiores para as menores, com "Outras" além de oito
categorias.

**Teste independente**: Passo 4.5 do `quickstart.md` — setembro tem dez categorias (sete + Outras),
agosto, três.

### Testes da história de usuário 2

- [X] T014 [P] [US2] `GetExpensesByCategoryUseCaseTests` — mês sem despesa (total 0, sem itens);
      maiores primeiro, com percentuais, empates pelo nome, nome e cor vindos da categoria;
      exatamente oito categorias (sem Outras); dez categorias (sete + Outras por último, com id e
      cor null, a soma do resto e a sua participação); intervalo do repositório para o mês,
      atravessando o ano em dezembro; sem mês → mês atual em São Paulo — em
      `tests/Denarius.Application.Tests/UseCases/Reports/GetExpensesByCategory/GetExpensesByCategoryUseCaseTests.cs`.
- [X] T015 [P] [US2] Testes de controller do `expensesByCategory` — `200` com o corpo (Outras com
      id/cor `null`), binding do mês, `400` para um mês inválido — em
      `tests/Denarius.WebAPI.Tests/Reports/ReportsControllerTests.cs` (depois da T008, mesmo
      arquivo).

### Implementação da história de usuário 2

- [X] T016 [P] [US2] `SumExpensesByCategoryAsync(DateOnly from, DateOnly to)` →
      `IEnumerable<(Category Category, decimal Expense)>`: despesa por categoria com despesas em
      `[from, to)`, agrupada por `CategoryId` e com join em `Categories` — em
      `src/Denarius.Domain/Repositories/ITransactionRepository.cs` e
      `src/Denarius.Infrastructure/Repositories/TransactionRepository.cs` (depois da T009, mesmos
      arquivos).
- [X] T017 [P] [US2] `GetExpensesByCategoryInput`, `ExpensesByCategoryOutput` (`Total`, `Items`) e
      `CategoryExpenseOutput` (`CategoryId` `Guid?`, `CategoryName`, `Color` `string?`, `Amount`,
      `Percentage`) em `src/Denarius.Application/IO/Reports/`.
- [X] T018 [US2] `IGetExpensesByCategoryUseCase` e `GetExpensesByCategoryUseCase` — ordena pelo
      valor em ordem decrescente e depois pelo nome; mais de 8 → as 7 maiores mais "Outras";
      percentual = valor / total × 100, duas casas decimais — em
      `src/Denarius.Application/UseCases/Reports/GetExpensesByCategory/` (depende de T016, T017).
- [X] T019 [US2] Registrar `IGetExpensesByCategoryUseCase` em
      `src/Denarius.Application/DependencyInjection.cs`.
- [X] T020 [US2] `[HttpGet("expensesByCategory")] ExpensesByCategory([FromQuery] YearMonth? month)`
      em `src/Denarius.WebAPI/Controllers/ReportsController.cs` (depende de T018).

**Ponto de controle**: O resumo e as despesas por categoria funcionam de forma independente.

---

## Fase 5: História de usuário 3 - Acompanhar receitas vs. despesas ao longo dos meses (Prioridade: P3)

**Objetivo**: `GET /api/reports/incomeVsExpense?month=YYYY-MM&months=12` devolve uma série contínua
e cronológica de receita, despesa e saldo mensais.

**Teste independente**: Passo 4.6 do `quickstart.md` — doze meses terminando em outubro de 2026, com
fevereiro a julho zerados.

### Testes da história de usuário 3

- [X] T021 [P] [US3] `GetIncomeVsExpenseUseCaseTests` — padrão de 12 meses terminando no mês; meses
      sem dados preenchidos com zeros (e um histórico vazio: tudo zero); uma série atravessando o
      ano (`month=2026-02`, `months=4` → 2025-11 … 2026-02, intervalo do repositório
      `[2025-11-01, 2026-03-01)`); `months=1`; saldo = receita − despesa, só com receita e só com
      despesa; sem mês → mês atual em São Paulo — em
      `tests/Denarius.Application.Tests/UseCases/Reports/GetIncomeVsExpense/GetIncomeVsExpenseUseCaseTests.cs`.
- [X] T022 [P] [US3] Testes de controller do `incomeVsExpense` — `200` com a lista; `months` padrão
      12, `months=1` e `months=24` com binding; `400` sem chamar o caso de uso para `months=0`,
      `25`, `abc` e para um mês inválido — em
      `tests/Denarius.WebAPI.Tests/Reports/ReportsControllerTests.cs`.

### Implementação da história de usuário 3

- [X] T023 [P] [US3] `GetIncomeVsExpenseInput` (`YearMonth? Month`, `int Months = 12`) e
      `IncomeVsExpenseOutput` (`Month`, `Income`, `Expense`, `Balance`) em
      `src/Denarius.Application/IO/Reports/`.
- [X] T024 [US3] `IGetIncomeVsExpenseUseCase` e `GetIncomeVsExpenseUseCase` — uma entrada por mês,
      de `month − (months − 1)` até `month`, a partir do `SumByMonthAsync`, com zeros onde não há
      linha — em `src/Denarius.Application/UseCases/Reports/GetIncomeVsExpense/` (depende de T009,
      T023).
- [X] T025 [US3] Registrar `IGetIncomeVsExpenseUseCase` em
      `src/Denarius.Application/DependencyInjection.cs`.
- [X] T026 [US3]
      `[HttpGet("incomeVsExpense")] IncomeVsExpense([FromQuery] YearMonth? month, [FromQuery, Range(1, 24)] int months = 12)`
      em `src/Denarius.WebAPI/Controllers/ReportsController.cs` (depende de T024).

**Ponto de controle**: Três relatórios funcionam de forma independente.

---

## Fase 6: História de usuário 4 - Comparar o ritmo de gastos deste mês com o do mês passado (Prioridade: P4)

**Objetivo**: `GET /api/reports/cumulativeExpenses?month=YYYY-MM` devolve o total acumulado diário
da despesa do mês e do mês anterior, e a quantidade de dias de cada um.

**Teste independente**: Passo 4.7 do `quickstart.md` — outubro para no dia 3, setembro tem 30 dias.

### Testes da história de usuário 4

- [X] T027 [P] [US4] `GetCumulativeExpenseComparisonUseCaseTests` — mês passado: uma entrada por
      dia, o total acumulado inalterado nos dias sem despesa e terminando no total do mês; mês sem
      dados: zeros em todos os dias; mês atual: para em hoje, com o mês anterior completo; mês
      futuro: nenhum dia (e o mês anterior dele, quando também é futuro, nenhum dia); janeiro vs.
      dezembro do ano anterior (31 + 31 dias, intervalo do repositório
      `[2025-12-01, 2026-02-01)`); fevereiro de 2026 (28) e de 2028 (29); sem mês → mês atual em
      São Paulo — em
      `tests/Denarius.Application.Tests/UseCases/Reports/GetCumulativeExpenseComparison/GetCumulativeExpenseComparisonUseCaseTests.cs`.
- [X] T028 [P] [US4] Testes de controller do `cumulativeExpenses` — `200` com as duas séries e as
      quantidades de dias, binding do mês, `400` para um mês inválido — em
      `tests/Denarius.WebAPI.Tests/Reports/ReportsControllerTests.cs`.

### Implementação da história de usuário 4

- [X] T029 [P] [US4] `SumExpensesByDayAsync(DateOnly from, DateOnly to)` →
      `IEnumerable<(DateOnly Date, decimal Expense)>`: despesa por dia com despesas em
      `[from, to)`, agrupada pelo ano, mês e dia UTC — em
      `src/Denarius.Domain/Repositories/ITransactionRepository.cs` e
      `src/Denarius.Infrastructure/Repositories/TransactionRepository.cs` (depois da T016, mesmos
      arquivos).
- [X] T030 [P] [US4] `GetCumulativeExpenseComparisonInput`, `CumulativeExpenseComparisonOutput`
      (`CurrentMonth`, `PreviousMonth`, `DaysInCurrentMonth`, `DaysInPreviousMonth`) e
      `AccumulatedExpenseOutput` (`Day`, `Accumulated`) em `src/Denarius.Application/IO/Reports/`.
- [X] T031 [US4] `IGetCumulativeExpenseComparisonUseCase` e `GetCumulativeExpenseComparisonUseCase`
      — uma consulta para os dois meses; cada série do dia 1 ao último dia do mês, até hoje no mês
      atual, nenhum dia num mês futuro — em
      `src/Denarius.Application/UseCases/Reports/GetCumulativeExpenseComparison/` (depende de T029,
      T030).
- [X] T032 [US4] Registrar `IGetCumulativeExpenseComparisonUseCase` em
      `src/Denarius.Application/DependencyInjection.cs`.
- [X] T033 [US4] `[HttpGet("cumulativeExpenses")] CumulativeExpenses([FromQuery] YearMonth? month)`
      em `src/Denarius.WebAPI/Controllers/ReportsController.cs` (depende de T031).

**Ponto de controle**: Quatro relatórios funcionam de forma independente.

---

## Fase 7: História de usuário 5 - Listar tudo o que aconteceu no mês (Prioridade: P5)

**Objetivo**: `GET /api/reports/transactions?month=YYYY-MM` devolve todas as transações do mês com
nome da categoria, tipo e valor positivo, das mais recentes para as mais antigas.

**Teste independente**: Passo 4.8 do `quickstart.md` — as doze transações de setembro, das mais
recentes para as mais antigas.

### Testes da história de usuário 5

- [X] T034 [P] [US5] `ListMonthlyTransactionsUseCaseTests` — todas as transações mapeadas (nome da
      categoria, `In`/`Out` pelo sinal, valor absoluto, data e descrição); data mais recente
      primeiro, empates pela criada mais recentemente; sem limite (30 transações → 30); mês sem
      transações → vazio; intervalo do repositório para o mês, atravessando o ano em dezembro; sem
      mês → mês atual em São Paulo — em
      `tests/Denarius.Application.Tests/UseCases/Reports/ListMonthlyTransactions/ListMonthlyTransactionsUseCaseTests.cs`.
- [X] T035 [P] [US5] Testes de controller do `transactions` — `200` com a lista, `type` serializado
      como `"in"`/`"out"`, binding do mês, `400` para um mês inválido — em
      `tests/Denarius.WebAPI.Tests/Reports/ReportsControllerTests.cs`.

### Implementação da história de usuário 5

- [X] T036 [P] [US5] `GetWithCategoryNameAsync(DateOnly from, DateOnly to)` →
      `IEnumerable<(Transaction Transaction, string CategoryName)>`: as transações em `[from, to)`
      com join no nome da sua categoria, `AsNoTracking` — em
      `src/Denarius.Domain/Repositories/ITransactionRepository.cs` e
      `src/Denarius.Infrastructure/Repositories/TransactionRepository.cs` (depois da T029, mesmos
      arquivos).
- [X] T037 [P] [US5] `TransactionType` serializado como `"all"`/`"in"`/`"out"`:
      `[JsonConverter(typeof(JsonStringEnumConverter<TransactionType>))]` e
      `[JsonStringEnumMemberName]` em cada membro, em
      `src/Denarius.Application/IO/Transactions/TransactionType.cs` (binding da query inalterado).
- [X] T038 [P] [US5] `ListMonthlyTransactionsInput` e `MonthlyTransactionOutput` (`Id`, `Date`,
      `Description`, `CategoryName`, `Type`, `Amount`, montado a partir de uma `Transaction` e do
      nome da sua categoria) em `src/Denarius.Application/IO/Reports/`.
- [X] T039 [US5] `IListMonthlyTransactionsUseCase` e `ListMonthlyTransactionsUseCase` — ordena por
      `Date` decrescente e depois por `CreatedAt` decrescente — em
      `src/Denarius.Application/UseCases/Reports/ListMonthlyTransactions/` (depende de T036–T038).
- [X] T040 [US5] Registrar `IListMonthlyTransactionsUseCase` em
      `src/Denarius.Application/DependencyInjection.cs`.
- [X] T041 [US5] `[HttpGet("transactions")] Transactions([FromQuery] YearMonth? month)` em
      `src/Denarius.WebAPI/Controllers/ReportsController.cs` (depende de T039).

**Ponto de controle**: Os cinco relatórios funcionam de forma independente.

---

## Fase 8: Acabamento e aspectos transversais

- [X] T042 Rodar `dotnet test Denarius.slnx` — as três suítes passam, com os testes novos (Princípio
      IV da constituição): Domain.Tests 35, Application.Tests 133 (54 antes), WebAPI.Tests 63 (38
      antes) — 231 no total. O `RoutingExtensionsTests` lista todas as rotas, então a sua
      expectativa ganhou as cinco rotas de relatórios.
- [X] T043 Rodar o `quickstart.md` num banco descartável: migration aplicada, revertida para
      `AddTransaction` e reaplicada (Princípio III); seed; todas as chamadas do passo 4 batem;
      `400`s; a verificação de volume do passo 5; o `/openapi/v1.json` lista as cinco rotas; apagar
      o banco descartável. Rodado em 2026-10-03 no PostgreSQL 18 (fuso horário do servidor
      America/Sao_Paulo, então uma data à meia-noite UTC agrupada no fuso errado teria ido para o
      dia anterior): `IX_Transactions_Date` criado, removido pelo rollback com
      `IX_Transactions_CategoryId` intocado, criado de novo; todas as respostas do passo 4 bateram
      (o SQL gerado agrupa por `date_part(... AT TIME ZONE 'UTC')` com `SUM(CASE ...)` só sobre o
      intervalo de datas); todos os `400`s; com 10.027 transações, todos os relatórios responderam
      em 4–20 ms, com o plano usando um bitmap scan em `IX_Transactions_Date`; o documento OpenAPI
      lista as cinco rotas. O mesmo banco depois serviu à verificação visual do front, e foi
      apagado. O banco de desenvolvimento não foi tocado: ele ainda precisa de
      `dotnet ef database update` para o índice.
- [X] T044 [P] Conferir de novo `contracts/reports-api.yaml` e `data-model.md` com o código entregue
      e marcar esta lista de tarefas como concluída.

---

## Dependências e ordem de execução

### Dependências entre fases

- **Preparação (Fase 1)**: Sem dependências.
- **Fundação (Fase 2)**: Depende da Preparação — bloqueia todas as histórias (`YearMonth`, o "hoje",
  o índice).
- **Histórias de usuário (Fases 3–7)**: Dependem da Fundação. Construídas na ordem de prioridade
  (P1 → P5); cada uma é testável sozinha depois de construída.
- **Acabamento (Fase 8)**: Depende das cinco histórias.

### Dependências entre histórias de usuário

- **US1 (P1)**: Só a Fundação. Cria o `ReportsController` e o `SumByMonthAsync`.
- **US2 (P2)**: Fundação; acrescenta uma action ao controller que a US1 criou.
- **US3 (P3)**: Reaproveita o `SumByMonthAsync` da US1; acrescenta uma action.
- **US4 (P4)**: Fundação; acrescenta uma consulta e uma action.
- **US5 (P5)**: Fundação; acrescenta uma consulta e uma action.

### Dentro de cada história de usuário

- Testes primeiro, falhando; depois consulta de repositório → records de IO → caso de uso → DI →
  action do controller.
- `ITransactionRepository.cs`, `TransactionRepository.cs`, `DependencyInjection.cs`,
  `ReportsController.cs` e `ReportsControllerTests.cs` são arquivos compartilhados: as tarefas deles
  rodam uma história por vez, mesmo quando marcadas com [P] em relação aos outros arquivos da mesma
  história.

### Oportunidades de paralelismo

- T002/T003/T004 (Fundação): arquivos diferentes.
- Em cada história, os testes do caso de uso, os testes de controller, a consulta de repositório e os
  records de IO mexem em arquivos diferentes e podem ser escritos juntos.

---

## Exemplo de paralelismo: história de usuário 1

```bash
Task: "GetMonthlySummaryUseCaseTests em tests/Denarius.Application.Tests/UseCases/Reports/GetMonthlySummary/GetMonthlySummaryUseCaseTests.cs"
Task: "ReportsControllerTests (resumo) em tests/Denarius.WebAPI.Tests/Reports/ReportsControllerTests.cs"
Task: "SumByMonthAsync em src/Denarius.Domain/Repositories/ITransactionRepository.cs + src/Denarius.Infrastructure/Repositories/TransactionRepository.cs"
Task: "GetMonthlySummaryInput/MonthlySummaryOutput/PreviousMonthSummaryOutput em src/Denarius.Application/IO/Reports/"
```

---

## Estratégia de implementação

### MVP primeiro (só a história de usuário 1)

Preparação + Fundação e depois o resumo: os cards de destaque do painel podem ser entregues sozinhos.

### Entrega incremental

1. Preparação + Fundação → `YearMonth`, hoje em São Paulo, o índice de data.
2. US1 → resumo (MVP).
3. US2 → despesas por categoria.
4. US3 → série de receitas vs. despesas.
5. US4 → comparação acumulada.
6. US5 → transações do mês.
7. Acabamento → execução completa dos testes e o quickstart no PostgreSQL.

---

## Observações

- Tarefas [P] = arquivos diferentes, sem dependências.
- Cite as regras do data-model.md ao implementar: a despesa é positiva, os percentuais vão de 0–100
  com duas casas decimais, `null` para não aplicável.
- As consultas do repositório não têm projeto de testes automatizados; é a execução do quickstart
  da T043 que prova o SQL delas.
