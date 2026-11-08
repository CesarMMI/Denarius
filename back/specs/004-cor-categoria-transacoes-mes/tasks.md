# Tarefas: Cor da categoria nas transações do mês

**Entrada**: [spec.md](./spec.md), [plan.md](./plan.md), [research.md](./research.md), [data-model.md](./data-model.md), [contrato](./contracts/monthly-transactions-api.yaml)

**Testes**: obrigatórios (TDD, princípio I): cada teste é escrito e visto falhar antes do código que ele cobre. Com a renomeação do método e do construtor, "falhar" inclui não compilar.

Há uma só user story (US1), então não há fase de Setup nem Foundational.

## Fase 1: US1 — Ver a cor da categoria em cada transação do mês (P1)

**Objetivo**: cada item de `GET /api/reports/transactions` traz `color`, a cor atual da categoria (FR-001 a FR-004).

**Teste independente**: [quickstart.md](./quickstart.md), itens 1 a 3.

### Testes primeiro (devem falhar)

- [X] T001 [P] [US1] Teste novo em `back/tests/Denarius.Infrastructure.Tests/Repositories/TransactionRepositoryTests.cs`: `GetWithCategoryAsync(from, to)` devolve `(Transaction, Category)` de cada transação do intervalo, com a cor da categoria (`Color.HexCode`), o nome e o filtro de data `[from, to)` como no método atual, e a cor atual depois de `Category.Update` com outra cor (FR-003)
- [X] T002 [P] [US1] Em `back/tests/Denarius.Application.Tests/UseCases/Reports/ListMonthlyTransactions/ListMonthlyTransactionsUseCaseTests.cs`: trocar `GetWithCategoryNameAsync` por `GetWithCategoryAsync` nos mocks (linhas 22, 26, 95 e 105) e a tupla para `(Transaction, Category)`, mantendo todas as asserções atuais (nome, tipo, valor, ordem, número de chamadas); acrescentar a asserção de `Color` (`#`+6 dígitos em maiúsculas) por item, e que transações da mesma categoria trazem a mesma cor
- [X] T003 [P] [US1] Em `back/tests/Denarius.WebAPI.Tests/Reports/ReportsControllerTests.cs`: ajustar `Transactions` (linha 46) ao novo construtor de `MonthlyTransactionOutput(transaction, category)` e, no teste de `?month=2026-09` (linhas 387-401), acrescentar que cada item serializa `color` como string, mantendo as asserções dos campos antigos (FR-004)
- [X] T004 [US1] Rodar `dotnet test` em `back/` e confirmar que T001 a T003 falham (não compilam) pelo motivo esperado. Antes, em `main` (ainda sem código novo), com a API no ar e a massa de dados do banco, medir 5 chamadas a `GET /api/reports/transactions?month=2026-11` e guardar o tempo médio como linha de base do SC-003

### Implementação

- [X] T005 [US1] `back/src/Denarius.Domain/Repositories/ITransactionRepository.cs`: renomear `GetWithCategoryNameAsync` para `GetWithCategoryAsync`, retorno `Task<IEnumerable<(Transaction Transaction, Category Category)>>`, e atualizar o resumo ("Each transaction, with its category")
- [X] T006 [US1] `back/src/Denarius.Infrastructure/Repositories/TransactionRepository.cs:127-134`: o `Join` passa a projetar a `Category` (`(t, c) => new { Transaction = t, Category = c }`), mantendo `DatedBetween` e o `AsNoTracking()`
- [X] T007 [US1] `back/src/Denarius.Application/IO/Reports/MonthlyTransactionOutput.cs`: construtor `(Transaction transaction, Category category)`; `CategoryName = category.Name`; novo `string Color { get; init; }` = `category.Color.HexCode`, não nulo
- [X] T008 [US1] `back/src/Denarius.Application/UseCases/Reports/ListMonthlyTransactions/ListMonthlyTransactionsUseCase.cs`: chamar `GetWithCategoryAsync` e montar `new MonthlyTransactionOutput(t.Transaction, t.Category)`, sem mudar a ordenação
- [X] T009 [US1] `dotnet test` em `back/`: as quatro suítes passam, com T001 a T003 agora verdes e nenhum teste apagado, ignorado ou enfraquecido

## Fase 2: Entrega

- [X] T010 `dotnet build` em `back/` sem erros nem avisos novos; rodar `dotnet format --verify-no-changes` só se ele já passar em `main` (o `back/AGENTS.md` não o define). Sem dependência nova, não há `--vulnerable` a rodar
- [X] T011 Conferir o [quickstart.md](./quickstart.md): itens 2 e 3 com a API no ar e o item 4 (uma consulta; tempo médio de 5 chamadas até 110% da linha de base do T004). Registrar o resultado no resumo da entrega, sem copiar saída
- [X] T012 Confirmar que `back/specs/003-financial-reports/contracts/reports-api.yaml`, `/transactions` e o `front/` estão sem alteração (`git diff --stat`) e que o contrato desta feature (`contracts/monthly-transactions-api.yaml`, versão 1.2.0) descreve o `color` como entregue

## Dependências

- T001, T002 e T003 são independentes entre si ([P]); T004 depende dos três.
- T005 a T008 seguem em ordem (a interface, o repositório, o output, o use case); T009 depende deles.
- T010 a T012 só depois de T009.

## Estratégia

Entrega única (MVP = US1): testes, vistos falhar, e a menor mudança que os faz passar. Sem migration (B3 não se aplica); contrato aditivo (B2).
