# Plano de implementação: Gestão de transações

**Branch**: `002-transaction-management` | **Data**: 2026-09-22 | **Spec**: [spec.md](./spec.md)

**Entrada**: Especificação da feature em `/specs/002-transaction-management/spec.md`

**Observação**: Este é um plano **retroativo** — documenta o design já entregue desta feature,
conferido com o código em `src/` e as suítes de testes em `tests/`, em vez de propor trabalho
novo. Um `/speckit-tasks` rodado sobre este plano não deve encontrar nada pendente além da lacuna
de documentação apontada na pesquisa. Atualizado em 2026-09-24 para cobrir a História de usuário 3
(filtro e ordenação da lista), projetada e entregue na mesma mudança desta atualização.

## Resumo

A gestão de transações oferece CRUD de transações financeiras individuais — data, valor (positivo
ou negativo), referência obrigatória a uma categoria e descrição opcional. Ela é implementada como
uma única fatia vertical pela arquitetura de quatro camadas existente: a entidade de domínio
`Transaction`, que se autovalida, impõe as regras de negócio; quatro casos de uso da camada
Application, cada um com um único propósito (Create/Update/Delete/List), orquestram a
persistência, e Create/Update também verificam se a `Category` referenciada existe; o EF Core
persiste `Transaction` no PostgreSQL, com uma chave estrangeira restrict-on-delete para
`Category`; um único `TransactionsController` expõe os casos de uso via REST; e o
`GlobalExceptionHandler` converte as exceções de domínio/não encontrado em respostas
`ProblemDetails`. Como em [001-category-management](../001-category-management/plan.md), o caso de
uso `List` recebe um `ListTransactionsInput` (descrição, mês `DateRef`, tipo, categoria,
`orderBy`/`asc`) e aplica os filtros e a ordenação em memória sobre as transações carregadas; não
há paginação.

## Contexto técnico

**Linguagem/versão**: C# 13 / .NET 10 (`net10.0`)

**Dependências principais**: ASP.NET Core (`Microsoft.AspNetCore.OpenApi`) para o host web; Entity
Framework Core 10 + `Npgsql.EntityFrameworkCore.PostgreSQL` para a persistência;
`Microsoft.Extensions.DependencyInjection.Abstractions` para os contratos de DI da camada
Application

**Armazenamento**: PostgreSQL, acessado via migrations code-first do EF Core
(`Denarius.Infrastructure/Migrations`); uma tabela `Transactions` com uma chave estrangeira
restrict-on-delete para `Categories.Id` e um índice não único em `CategoryId`

**Testes**: xUnit nos três projetos de teste; `Denarius.Domain.Tests` e
`Denarius.Application.Tests` usam NSubstitute para os dublês de repositório/unit of work;
`Denarius.WebAPI.Tests` usa `Microsoft.AspNetCore.TestHost` para testes de integração in-process

**Plataforma-alvo**: ASP.NET Core Web API (lado do servidor), consumida pelo front-end Angular do
Denarius (projeto separado, `front/`) e acessível em `api/transactions`

**Tipo de projeto**: web — a metade backend de uma aplicação web de dois projetos; este plano
cobre só o backend

**Metas de desempenho**: Nenhuma especificada formalmente. Comportamento atual:
`GET /api/transactions` carrega todas as transações em memória uma vez por requisição e depois
filtra e ordena no caso de uso (mais uma consulta extra para os nomes das categorias, só quando a
ordenação é por `CategoryName`) — a mesma postura de "aceitável na escala atual" do
`ListCategoriesUseCase`. Levar os filtros para a consulta do repositório é o próximo passo natural
se os volumes superarem isso (veja research.md → Filtro e ordenação da lista).

**Restrições**: Regidas por `.specify/memory/constitution.md` — as respostas da API precisam
continuar retrocompatíveis (Princípio I), o código precisa ficar na sua camada da Clean
Architecture (Princípio II), qualquer migration futura precisa de um rollback verificado
(Princípio III) e o `dotnet test` precisa passar nas três suítes (Princípio IV).

**Escala/escopo**: Uso single-tenant de finanças pessoais; por usuário, de centenas a poucos
milhares de transações — a lista sem paginação e filtrada em memória foi dimensionada para essa
faixa, e não para grandes volumes multi-tenant.

## Constitution Check

*PORTÃO: precisa passar antes da pesquisa da Fase 0. Conferir de novo após o design da Fase 1.*

| Princípio | Status | Evidência |
|---|---|---|
| I. Compatibilidade da API pública | PASSA | A História de usuário 3 é **aditiva**: `GET /api/transactions` ganha seis parâmetros de query opcionais (`description`, `dateRef`, `type`, `categoryId`, `orderBy`, `asc`), e uma chamada sem nenhum deles continua devolvendo todas as transações, com o formato de resposta inalterado. A única diferença observável para quem já chama a API é que a ordem, antes não especificada, agora é data decrescente. Nenhuma outra rota, DTO ou código de status mudou. |
| II. Respeito às fronteiras de serviço | PASSA | A validação fica em `Denarius.Domain` (`Transaction`); a orquestração, nos casos de uso de `Denarius.Application`, que dependem só de `Domain` (mais o `ICategoryRepository`, para a verificação de existência entre entidades e para os nomes das categorias na ordenação por `CategoryName`); os detalhes do EF Core ficam confinados a `Denarius.Infrastructure`; o `TransactionsController` de `Denarius.WebAPI` só faz o binding dos parâmetros de query e chama interfaces de casos de uso. Nenhuma camada é pulada. |
| III. Disciplina de rollback de migrations | PASSA | A migration que cria a tabela `Transactions` (`20260814132713_AddTransaction`) tem um `Down()` limpo e não destrutivo (`DropTable`). A História de usuário 3 não adiciona migration. |
| IV. Verificação da suíte de testes | PASSA | A entidade `Transaction` e os quatro casos de uso têm cobertura própria em xUnit; o próprio `TransactionsController` é coberto por `tests/Denarius.WebAPI.Tests/Transactions/TransactionsControllerTests.cs` (adicionado pelo `/speckit-implement` em 2026-09-22, fechando a lacuna que a pesquisa tinha apontado). Os filtros, as opções de ordenação, o binding da query e o comportamento de 400 para parâmetro inválido da História de usuário 3 são cobertos em `ListTransactionsUseCaseTests.cs` e `TransactionsControllerTests.cs`. |

Nenhuma violação — o Acompanhamento de complexidade não é necessário.

*Conferido de novo após o design da Fase 1: sem mudança — data-model.md e contracts/ descrevem o
design entregue e não introduzem nada novo que precise passar pelo portão de novo.*

## Estrutura do projeto

### Documentação (desta feature)

```text
specs/002-transaction-management/
├── plan.md               # Este arquivo (saída do comando /speckit-plan)
├── research.md           # Saída da Fase 0 (comando /speckit-plan)
├── data-model.md         # Saída da Fase 1 (comando /speckit-plan)
├── quickstart.md         # Saída da Fase 1 (comando /speckit-plan)
├── contracts/            # Saída da Fase 1 (comando /speckit-plan)
└── tasks.md              # Saída da Fase 2 (comando /speckit-tasks - NÃO é criado pelo /speckit-plan)
```

### Código-fonte (raiz do repositório)

```text
src/
├── Denarius.Domain/
│   ├── Entities/Transaction.cs
│   └── Repositories/ITransactionRepository.cs
├── Denarius.Application/
│   ├── IO/Transactions/               # CreateTransactionInput, UpdateTransactionInput,
│   │                                   # ListTransactionsInput, TransactionOutput,
│   │                                   # TransactionType, TransactionOrderField
│   └── UseCases/Transactions/         # Create, Update, Delete, List
├── Denarius.Infrastructure/
│   ├── Persistence/Configurations/TransactionConfiguration.cs
│   ├── Repositories/TransactionRepository.cs
│   └── Migrations/20260814132713_AddTransaction.cs
└── Denarius.WebAPI/
    └── Controllers/TransactionsController.cs

tests/
├── Denarius.Domain.Tests/Entities/TransactionTests.cs
├── Denarius.Application.Tests/UseCases/Transactions/   # uma pasta por caso de uso
└── Denarius.WebAPI.Tests/Transactions/TransactionsControllerTests.cs
```

**Decisão de estrutura**: A mesma de [001-category-management](../001-category-management/plan.md)
— a Opção 2 do template (aplicação web: backend/frontend separados) restrita ao backend, com o
layout real de Clean Architecture deste repositório (`Denarius.Domain` / `Denarius.Application` /
`Denarius.Infrastructure` / `Denarius.WebAPI` em `src/`, espelhados 1:1 em `tests/`) em vez da
divisão genérica `models/services/api` do template.

## Acompanhamento de complexidade

Não se aplica — o Constitution Check não apontou violações.
