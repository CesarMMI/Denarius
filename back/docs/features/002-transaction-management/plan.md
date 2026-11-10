# Plano: Gestão de transações

**Spec**: [spec.md](./spec.md) · **Entregue em**: `aad5ed3` (filtros e ordenação), `401eeea` (consulta no banco) · **Contrato**: [contracts/transactions-api.yaml](./contracts/transactions-api.yaml) (1.3.0)

## Conferência com as regras

- Camadas (B1): `Transaction` se autovalida no `Domain`; os casos de uso Create, Update, Delete e List ficam no `Application`, e Create e Update checam a categoria por `ICategoryRepository`. A consulta da lista fica no `Infrastructure`, atrás de `ITransactionRepository.ListAsync(TransactionListQuery)`, com os tipos da consulta no `Domain`; o `Application` não vê EF.
- Compatibilidade da API (B2): os filtros e a ordenação entraram como parâmetros de query opcionais, e a chamada sem eles segue devolvendo tudo. Levar a consulta ao banco não muda rotas, parâmetros, status nem `ProblemDetails`; o contrato foi para a 1.3.0, aditivo.
- Migrations (B3): `AddTransaction` (`Down()` com `DropTable`), `AddTransactionDateIndex` e `AddPortugueseCollation`. Esta cria a collation `pt_br_icu`; o `Down()` gerado pelo Npgsql já traz o `DROP COLLATION`, e `Up()` e `Down()` foram aplicados, revertidos e reaplicados numa base descartável.
- Segurança (II): o texto buscado vai como parâmetro (`Unaccent`/`Contains` do EF), o nome da collation é constante, nenhum parâmetro de entrada novo e nada de descrição ou busca em log (FR-021).
- Performance (III): filtros e ordenação no banco, `AsNoTracking()` e índices em `Date` e `CategoryId`; busca e tipo não ganham índice sem medição. Meta: SC-008.
- Dependência nova: nenhuma. A extensão `unaccent` veio da correção da [busca de categorias](../../bugs/busca-de-nome-diferencia-maiusculas/fix.md).

## Mudanças

- **Domínio**: `Transaction`, `ITransactionRepository`, `TransactionListQuery` (com `TransactionKind` e `TransactionSortField`). Regras em [docs/dominio/transacao.md](../../dominio/transacao.md).
- **Aplicação**: `Create`, `Update`, `Delete` e `List` em `UseCases/Transactions`. O `ListTransactionsUseCase` só traduz o input na consulta (trim, branco vira nulo, tipo, campo, direção, mês em intervalo UTC) e monta o output. O `GET` por id foi retirado em `085573b`.
- **Infraestrutura**: `TransactionRepository.ListAsync`; `HasCollation("pt_br_icu", ...)` no `DenariusDbContext`; migration `AddPortugueseCollation`. Tabela `Transactions` com FK `Restrict` para `Categories.Id` e índices não únicos em `Date` e `CategoryId`.
- **API**: `TransactionsController` com `GET` (lista), `POST`, `PUT /{id}` e `DELETE /{id}`; erros pelo `GlobalExceptionHandler` (convenções em [docs/api.md](../../api.md)).

## Decisões

Alternativas e medições em [research.md](./research.md).

- **Consulta no banco**: busca, mês, tipo, categoria e ordenação viram uma consulta SQL num método novo do repositório, em vez de carregar tudo e filtrar em memória.
- **Busca sem acentos**: `unaccent`, como a busca de categorias.
- **Ordem alfabética**: collation ICU pt-BR declarada em migration, para a ordem não depender do ambiente (FR-018). Pede PostgreSQL com ICU; sem ele a migration falha explicitamente.
- **Mês**: intervalo meio-aberto `[início, início do mês seguinte)`.
- **Índices**: os de `Date` e `CategoryId` bastam; busca e tipo ficam sem índice até haver medição (FR-019).
- **Ordem de implantação**: a migration vai antes do código novo, porque `COLLATE "pt_br_icu"` sem a collation responde `500`; reverter a migration com o código novo no ar também quebra a lista.

## Testes previstos

Escritos antes do código; os testes antigos de `ListTransactionsUseCaseTests` passaram a simular `ListAsync`, sem perder asserção.

- **Domain** (`TransactionTests`): validação de data, valor e descrição (FR-002 a FR-007).
- **Application**: Create, Update, Delete (incluindo categoria e transação inexistentes) e List, com a tradução do input em consulta e o mapeamento do output (FR-001 a FR-012).
- **Infrastructure** (`TransactionRepositoryTests`, Testcontainers): busca (acentos, caixa, `%`, `_`, `\`, nulos), mês (00:00 do dia 1 e 23:59:59,999 do último), tipo, categoria, combinação, cada campo de ordenação nas duas direções com desempates, posição das sem descrição, ordem "agua, Agua, água, Água, Banana" (FR-013 a FR-018, SC-007, SC-009). `PortugueseCollationMigrationTests`: aplicar, reverter e reaplicar.
- **WebAPI** (`TransactionsControllerTests`): binding da query, 400 para parâmetro inválido e formato da lista inalterado (FR-020, SC-010).

## Verificação

1. `dotnet test` com as quatro suítes verdes (os testes de repositório exigem Docker).
2. Com a API no ar, criar uma transação em `POST /api/transactions` (`201` e `Location`), listá-la com `GET /api/transactions` e com filtros (`description`, `dateRef`, `type`, `categoryId`, `orderBy`, `asc`), editar com `PUT` (descrição em branco vira `null`) e excluir com `DELETE`; `type=invalid` responde `400` com `ProblemDetails`.
3. SC-008: medido em 2026-10-11 com 10.000 transações, 200 no mês e 200 na categoria, e API em `Release`: 31,5 para 4,4 ms (mês, 7,2 vezes) e 32,3 para 4,3 ms (categoria, 7,5 vezes), detalhes em [research.md](./research.md#medição-de-base-e-de-depois).
