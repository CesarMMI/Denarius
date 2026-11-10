# Plano: Cor da categoria nas transações do mês

**Spec**: [spec.md](./spec.md) · **Entregue em**: `876417c` · **Contrato**: [contracts/monthly-transactions-api.yaml](./contracts/monthly-transactions-api.yaml)

## Conferência com as regras

- Camadas (B1): o repositório devolve a entidade, o Application monta o output, o WebAPI não muda.
- Compatibilidade da API (B2): campo novo, obrigatório e não nulo, só na resposta. É aditivo e não pede rota nem versão nova.
- Migrations (B3): nenhuma.
- Segurança (II): sem entrada nova, sem segredo, sem SQL concatenado (LINQ); a cor não é dado sensível.
- Performance (III): o `Join` com `Category` já existe e a cor vem da mesma linha, sem consulta por item. A meta é o SC-003.

## Mudanças

- `ITransactionRepository.GetWithCategoryNameAsync` vira `GetWithCategoryAsync` e devolve `(Transaction, Category)` (`TransactionRepository.cs`).
- `MonthlyTransactionOutput` ganha `Color`, lido de `Category.Color.HexCode`.
- `ListMonthlyTransactionsUseCase` usa o método renomeado.
- Contrato novo em `contracts/monthly-transactions-api.yaml`, só com a operação e o schema alterados, e `pattern: '^#[0-9A-F]{6}$'`.

## Decisões

Detalhes e alternativas em [research.md](./research.md). Em resumo:

- **Origem da cor**: o repositório devolve a `Category` inteira, como `SumExpensesByCategoryAsync`; a cor sai atual a cada consulta. Se o SC-003 reprovar, a alternativa é devolver só nome e cor.
- **Nulidade**: `color` é `string` não nula (toda transação tem categoria e a cor é obrigatória no value object).
- **Contrato**: arquivo novo, para não reescrever o da 003 (FR-005).

## Testes previstos

Escritos antes do código e vistos falhar: use case, repositório e controller.

- Os testes de `ListMonthlyTransactionsUseCaseTests` só mudam a chamada renomeada do mock, o tipo do segundo item da tupla e o construtor do output; as asserções de nome, tipo, valor e número de chamadas ficam.
- Teste novo no repositório (`TransactionRepositoryTests`): cor devolvida, cor atualizada depois da criação, nome e filtro de data como antes.

## Verificação

1. `dotnet test` com as quatro suítes verdes (exige o Docker, por causa do Testcontainers).
2. Com a API no ar, `GET /api/reports/transactions?month=2026-11` traz `color` igual à da categoria em `GET /api/categories` (SC-001), e os campos antigos inalterados (SC-002).
3. Mudar a cor de uma categoria e repetir a chamada traz a cor nova (FR-003).
4. SC-003: com o log do EF em `Information`, a chamada mostra uma só consulta; o tempo fica em até 110% do de `main`.

## Tarefas

- [X] T001 [P] Teste novo em `TransactionRepositoryTests`: `GetWithCategoryAsync` devolve a cor, o nome e o filtro de data `[from, to)`, e a cor atual após `Category.Update` (FR-003)
- [X] T002 [P] Ajustar `ListMonthlyTransactionsUseCaseTests` ao método renomeado e à tupla `(Transaction, Category)`, mantendo as asserções; acrescentar `Color` por item e mesma cor para a mesma categoria
- [X] T003 [P] Ajustar `ReportsControllerTests` ao novo construtor do output; no teste de `?month=2026-09`, conferir que cada item serializa `color` como string, mantendo os campos antigos (FR-004)
- [X] T004 Rodar `dotnet test` e ver T001 a T003 falharem; medir em `main` o tempo médio de 5 chamadas como linha de base do SC-003
- [X] T005 Renomear o método em `ITransactionRepository` e mudar o retorno para `(Transaction, Category)`
- [X] T006 Ajustar o `Join` em `TransactionRepository` para projetar a `Category`, mantendo `DatedBetween` e `AsNoTracking()`
- [X] T007 `MonthlyTransactionOutput` recebe `(Transaction, Category)` e ganha `Color`
- [X] T008 `ListMonthlyTransactionsUseCase` usa `GetWithCategoryAsync`, sem mudar a ordenação
- [X] T009 `dotnet test` com as quatro suítes verdes, nenhum teste apagado ou enfraquecido
- [X] T010 `dotnet build` sem erros nem avisos novos
- [X] T011 Conferir a seção Verificação (itens 2 a 4) e registrar o resultado no resumo da entrega
- [X] T012 Confirmar que o contrato da 003, `/transactions` e o `front/` ficaram intactos e que o contrato desta feature (versão 1.2.0) descreve o `color`
