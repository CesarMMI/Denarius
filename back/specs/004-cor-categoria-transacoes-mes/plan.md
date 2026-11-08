# Plano de implementação: Cor da categoria nas transações do mês

**Branch**: `main` | **Data**: 2026-11-07 | **Spec**: [spec.md](./spec.md)

## Resumo

`GET /api/reports/transactions` passa a devolver, em cada item, o campo `color`: a cor atual da categoria da
transação. A consulta que já junta a transação à categoria (`TransactionRepository.GetWithCategoryNameAsync`)
passa a devolver a própria `Category` em vez de só o nome; o `MonthlyTransactionOutput` lê nome e cor dela. Sem
consulta, migration, rota ou parâmetro novo. Contrato aditivo ([contracts/](./contracts/monthly-transactions-api.yaml)).

## Contexto técnico

- **Linguagem e stack**: .NET 10, EF Core, PostgreSQL (como em `back/AGENTS.md`).
- **Testes**: xUnit, NSubstitute; testes do repositório em PostgreSQL descartável (Testcontainers).
- **Metas** (SC-003): uma consulta por requisição e tempo de resposta de até 110% do anterior com a mesma massa de dados.
- **Escopo**: só `ReportsController` / `ListMonthlyTransactions`; `/transactions` e o front ficam como estão.

## Constitution Check

| Regra | Situação |
| --- | --- |
| I. TDD | Testes primeiro (use case, repositório, controller), vistos falhar antes do código. Nenhum teste é apagado ou enfraquecido; os de `ListMonthlyTransactionsUseCaseTests` só mudam a chamada renomeada do mock (`GetWithCategoryAsync`), o tipo do segundo item da tupla e o construtor do output, com as asserções de nome, tipo, valor e número de chamadas mantidas. O teste de repositório é novo (hoje o método só é coberto no use case, via mock): cor devolvida, cor atualizada depois da criação, nome e filtro de data como antes |
| II. Segurança | Sem entrada nova, sem segredo, sem SQL concatenado (LINQ). A cor não é dado sensível |
| III. Performance | O `Join` com `Category` já existe; a cor vem da mesma linha, sem consulta por item. Nada a medir além do SC-003, conferido no quickstart |
| IV. Contrato | Campo novo, obrigatório e não nulo, só na resposta: aditivo (B2), que não pede rota nem versão de API nova (`back/AGENTS.md`, B2: "um campo opcional" e adições compatíveis). O front só o consome numa feature própria, depois desta |
| V. Escopo | Só o back; menor mudança; segue o padrão de `SumExpensesByCategoryAsync`, que já devolve `Category` |
| B1. Camadas | Repositório (Domain/Infrastructure) devolve a entidade; o Application monta o output; o WebAPI não muda |
| B3. Migrations | Nenhuma migration |

Sem violações; sem Complexity Tracking.

## Estrutura

```text
back/specs/004-cor-categoria-transacoes-mes/  plan.md, research.md, data-model.md, quickstart.md, contracts/
back/src/Denarius.Domain/Repositories/ITransactionRepository.cs                  # GetWithCategoryAsync -> (Transaction, Category)
back/src/Denarius.Infrastructure/Repositories/TransactionRepository.cs
back/src/Denarius.Application/IO/Reports/MonthlyTransactionOutput.cs              # + Color
back/src/Denarius.Application/UseCases/Reports/ListMonthlyTransactions/ListMonthlyTransactionsUseCase.cs
back/tests/{Application,Infrastructure,WebAPI}.Tests/...                          # ListMonthlyTransactions, TransactionRepository, ReportsController
back/specs/003-financial-reports/contracts/reports-api.yaml                       # intocado (FR-005)
```
