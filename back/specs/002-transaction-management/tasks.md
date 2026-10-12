---

description: "Lista de tarefas da feature Gestão de transações"
---

# Tarefas: Gestão de transações

**Entrada**: Documentos de design em `/specs/002-transaction-management/`

**Pré-requisitos**: plan.md (presente), spec.md (presente), research.md (presente),
data-model.md (presente), contracts/ (presente)

**Status**: Esta é uma lista de tarefas **retroativa** — a gestão de transações já está
implementada. `[x]` marca uma tarefa já atendida pelo código/testes existentes (com o caminho do
arquivo, para que o mapeamento de requisito para implementação seja rastreável). O
`/speckit-implement` rodou em 2026-09-22 e fechou os dois itens que ainda estavam pendentes (T025,
T026). A História de usuário 3 (filtro e ordenação da lista, T027-T033) e a sua execução de
validação (T034) foram adicionadas e concluídas em 2026-09-24 — as 34 tarefas agora estão `[x]`.

**Testes**: As tarefas de teste abaixo refletem os testes automatizados que já existem por
história; nenhuma tarefa de teste nova foi adicionada além da única lacuna de cobertura explícita
(T025), que o `research.md` já tinha apontado, em vez de um genérico "adicionar mais testes", e dos
testes entregues com a História de usuário 3 (T027, T028).

**Organização**: As tarefas estão agrupadas por história de usuário, conforme as prioridades do
`spec.md` (P1/P2/P3).

## Formato: `[ID] [P?] [História] Descrição`

- **[P]**: Arquivos diferentes, sem dependência de uma tarefa incompleta
- **[História]**: A história de usuário a que a tarefa pertence (US1, US2, US3)
- Os caminhos de arquivo são exatos, relativos à raiz do repositório

## Fase 1: Preparação (infraestrutura compartilhada)

**Objetivo**: Inicialização do projeto. É compartilhada por todas as features do backend, e não
foi criada especificamente para Transaction — está aqui só para que esta lista siga a estrutura de
fases exigida.

- [x] T001 Confirmar que a solução de Clean Architecture com quatro projetos existe, conforme o
      `Denarius.slnx` (`Denarius.Domain`, `Denarius.Application`, `Denarius.Infrastructure`,
      `Denarius.WebAPI`, com os projetos `*.Tests` correspondentes) — preexistente, compartilhada
      com a T001 de [001-category-management](../001-category-management/tasks.md).
- [x] T002 [P] Confirmar que as referências de pacote do EF Core +
      `Npgsql.EntityFrameworkCore.PostgreSQL` (em
      `src/Denarius.Infrastructure/Denarius.Infrastructure.csproj`) e do xUnit + NSubstitute /
      `Microsoft.AspNetCore.TestHost` (nos três `tests/*.csproj`) estão presentes —
      preexistente, compartilhada com a T002 de 001-category-management.

---

## Fase 2: Fundação (pré-requisitos bloqueantes)

**Objetivo**: A entidade, a persistência e a estrutura do controller de que a história de usuário
abaixo depende.

**⚠️ CRÍTICO**: Nenhuma história de usuário funciona sem esta fase.

- [x] T003 [P] Entidade `Transaction` — `Description`: "opcional; com trim; em branco/só espaços é
      normalizada para null; até 255 caracteres depois do trim, senão lança DomainException";
      `Date`: "obrigatório; `default(DateTime)` lança DomainException"; `Value`: "obrigatório;
      decimal diferente de zero, positivo ou negativo; `0` lança DomainException"; `CategoryId`:
      "obrigatório; `Guid.Empty` lança DomainException" — em
      `src/Denarius.Domain/Entities/Transaction.cs`.
- [x] T004 [P] `ITransactionRepository` (estende `IRepository<Transaction>` e adiciona
      `GetAllAsync()` e `ExistsByCategoryIdAsync(Guid categoryId)`) em
      `src/Denarius.Domain/Repositories/ITransactionRepository.cs` (depende de T003).
- [x] T005 [P] Mapeamento EF `TransactionConfiguration` — `Description`
      `character varying(255)` anulável, `Date` `timestamp with time zone` not null, `Value`
      `numeric(18,2)` not null, `CategoryId` `uuid` not null com a FK
      `FK_Transactions_Categories_CategoryId` `ON DELETE RESTRICT` e um índice em `CategoryId`
      — em `src/Denarius.Infrastructure/Persistence/Configurations/TransactionConfiguration.cs`
      (depende de T003; o alvo da FK depende das T003/T006 de
      [001-category-management](../001-category-management/tasks.md)).
- [x] T006 [P] `TransactionRepository.GetAllAsync()` e
      `TransactionRepository.ExistsByCategoryIdAsync(Guid)` em
      `src/Denarius.Infrastructure/Repositories/TransactionRepository.cs` (depende de T004).
- [x] T007 Migration da tabela `Transactions` com um `Down()` limpo e reversível (`DropTable`),
      incluindo a FK para `Categories` e o índice `IX_Transactions_CategoryId`, em
      `src/Denarius.Infrastructure/Migrations/20260814132713_AddTransaction.cs` (depende de
      T005) — atende ao Princípio III da constituição.
- [x] T008 Registrar `ITransactionRepository → TransactionRepository` em
      `src/Denarius.Infrastructure/DependencyInjection.cs` (depende de T006).
- [x] T009 Criar a estrutura do `TransactionsController`, reaproveitando o `GlobalExceptionHandler`
      compartilhado que mapeia `NotFoundException → 404` e `DomainException`/`AppException → 400`,
      em `src/Denarius.WebAPI/Controllers/TransactionsController.cs` (depende de T008; handler
      compartilhado com a T011 de 001-category-management).

**Ponto de controle**: Fundação pronta — a história de usuário abaixo se apoia nela.

---

## Fase 3: História de usuário 1 - Registrar e manter transações individuais (Prioridade: P1) 🎯 MVP

**Objetivo**: Criar, visualizar, editar e excluir transações, cada uma referenciando uma categoria
existente.

**Teste independente**: Criar uma transação numa categoria existente, confirmar que ela aparece
com a data/valor/categoria/descrição escolhidos, editá-la e excluí-la — sem precisar de outras
transações.

### Testes da história de usuário 1 (já existem)

- [x] T010 [P] [US1] Testes de validação/trim/update de `Transaction` (normalização/tamanho da
      descrição, rejeição de data/valor/categoryId, aceitação de valor negativo) em
      `tests/Denarius.Domain.Tests/Entities/TransactionTests.cs`.
- [x] T011 [P] [US1] Testes do `CreateTransactionUseCase` — incluindo a proteção de categoria não
      encontrada — em
      `tests/Denarius.Application.Tests/UseCases/Transactions/Create/CreateTransactionUseCaseTests.cs`.
- [x] T012 [P] [US1] Testes do `UpdateTransactionUseCase` — incluindo as proteções de transação não
      encontrada e de categoria não encontrada — em
      `tests/Denarius.Application.Tests/UseCases/Transactions/Update/UpdateTransactionUseCaseTests.cs`.
- [x] T013 [P] [US1] Testes do `DeleteTransactionUseCase` — proteção de não encontrada e, fora
      isso, exclusão incondicional — em
      `tests/Denarius.Application.Tests/UseCases/Transactions/Delete/DeleteTransactionUseCaseTests.cs`.
- [x] T014 [P] [US1] Testes do `GetTransactionByIdUseCase` em
      `tests/Denarius.Application.Tests/UseCases/Transactions/GetById/GetTransactionByIdUseCaseTests.cs`.

### Implementação da história de usuário 1

- [x] T015 [P] [US1] DTOs `CreateTransactionInput`/`UpdateTransactionInput` (`description`,
      `date`, `value`, `categoryId`) em
      `src/Denarius.Application/IO/Transactions/CreateTransactionInput.cs` e
      `UpdateTransactionInput.cs`.
- [x] T016 [P] [US1] DTO `TransactionOutput` em
      `src/Denarius.Application/IO/Transactions/TransactionOutput.cs`.
- [x] T017 [US1] `ICreateTransactionUseCase`/`CreateTransactionUseCase` — verifica se a `Category`
      referenciada existe via `ICategoryRepository.GetByIdAsync` antes de criar a `Transaction` —
      em `src/Denarius.Application/UseCases/Transactions/Create/` (depende de T015, T016).
- [x] T018 [US1] `IUpdateTransactionUseCase`/`UpdateTransactionUseCase` — 404 via
      `NotFoundException` quando a transação não existe, e de novo quando a categoria referenciada
      não existe — em `src/Denarius.Application/UseCases/Transactions/Update/` (depende de T015,
      T016).
- [x] T019 [US1] `IDeleteTransactionUseCase`/`DeleteTransactionUseCase` — 404 quando não existe;
      fora isso, exclui incondicionalmente (sem proteção de dados dependentes, ao contrário da
      proteção de exclusão da T021 de [001-category-management](../001-category-management/tasks.md))
      — em `src/Denarius.Application/UseCases/Transactions/Delete/` (depende de T016).
- [x] T020 [US1] `IGetTransactionByIdUseCase`/`GetTransactionByIdUseCase` — 404 quando não existe
      — em `src/Denarius.Application/UseCases/Transactions/GetById/` (depende de T016).
- [x] T021 [US1] Ligar as actions `POST`, `PUT {id}`, `DELETE {id}` e `GET {id}` em
      `src/Denarius.WebAPI/Controllers/TransactionsController.cs` (depende de T017-T020).

**Ponto de controle**: História de usuário 1 totalmente funcional e testável de forma
independente.

---

## Fase 4: História de usuário 2 - Revisar as transações registradas (Prioridade: P2)

**Objetivo**: Ver todas as transações registradas numa única lista.

**Teste independente**: Registrar várias transações; confirmar que todas aparecem quando a lista
de transações é exibida, e que a lista fica vazia quando nenhuma foi registrada.

### Testes da história de usuário 2 (já existem)

- [x] T022 [US2] Testes do `ListTransactionsUseCase`
      (`Execute_TransactionsExist_ReturnsAllTransactionsMappedToOutput`,
      `Execute_NoTransactions_ReturnsEmpty`) em
      `tests/Denarius.Application.Tests/UseCases/Transactions/List/ListTransactionsUseCaseTests.cs`.

### Implementação da história de usuário 2

- [x] T023 [US2] `IListTransactionsUseCase`/`ListTransactionsUseCase` — chama
      `ITransactionRepository.GetAllAsync()` e mapeia cada resultado para `TransactionOutput` —
      em `src/Denarius.Application/UseCases/Transactions/List/ListTransactionsUseCase.cs`
      (depende de T016, T006). Originalmente não recebia entrada nem aplicava filtro/ordenação;
      estendido pela T031.
- [x] T024 [US2] Ligar o `GET /api/transactions` em
      `src/Denarius.WebAPI/Controllers/TransactionsController.cs` (depende de T023).
      Originalmente não recebia parâmetros de query; estendido pela T032.

**Ponto de controle**: As duas histórias de usuário funcionando de forma independente.

---

## Fase 5: História de usuário 3 - Filtrar e reordenar a lista de transações (Prioridade: P3)

**Objetivo**: Filtrar a lista de transações por descrição, mês, tipo e categoria, e ordená-la por
data, descrição, valor ou nome da categoria.

**Teste independente**: Registrar várias transações com descrições, datas, sinais e categorias
variados; confirmar que cada filtro e cada opção de ordenação, por si só, filtra ou reordena a
lista corretamente, e que os filtros combinados mantêm só as transações que atendem a todos eles.

### Testes da história de usuário 3

- [x] T027 [P] [US3] Testes do `ListTransactionsUseCase` — ordem padrão por data decrescente,
      busca na descrição sem diferenciar maiúsculas de minúsculas e com trim (e busca em branco
      ignorada), limites do mês do `DateRef`, `Type` `All`/`In`/`Out`, `CategoryId`, filtros
      combinados, cada campo de ordenação nas duas direções e categorias carregadas só na
      ordenação por `CategoryName` — em
      `tests/Denarius.Application.Tests/UseCases/Transactions/List/ListTransactionsUseCaseTests.cs`.
- [x] T028 [P] [US3] Testes de listagem do `TransactionsController` — cada parâmetro de query com
      binding e repassado ao caso de uso (inclusive `type=out` em minúsculas), os padrões quando
      nenhum é informado e `400`, sem chamar o caso de uso, para `type`, `orderBy` ou `categoryId`
      inválidos — em `tests/Denarius.WebAPI.Tests/Transactions/TransactionsControllerTests.cs`.

### Implementação da história de usuário 3

- [x] T029 [P] [US3] Enum `TransactionType` (`All`, `In`, `Out`) e enum `TransactionOrderField`
      (`Date`, `Description`, `Value`, `CategoryName`) em
      `src/Denarius.Application/IO/Transactions/TransactionType.cs` e
      `TransactionOrderField.cs`.
- [x] T030 [US3] Record `ListTransactionsInput` (`Description`, `DateRef`, `Type`,
      `CategoryId`, `OrderBy`, `Ascending`; padrões: sem filtros, `Date` decrescente) em
      `src/Denarius.Application/IO/Transactions/ListTransactionsInput.cs` (depende de T029).
- [x] T031 [US3] Mudar a entrada do `IListTransactionsUseCase` de `object?` para
      `ListTransactionsInput`; aplicar os quatro filtros e o switch de ordenação `orderBy`/`asc` no
      `ListTransactionsUseCase`, injetando o `ICategoryRepository` para a ordenação por
      `CategoryName` — em `src/Denarius.Application/UseCases/Transactions/List/` (depende de
      T030).
- [x] T032 [US3] Ligar os parâmetros de query `description`, `dateRef`, `categoryId`, `type`,
      `orderBy` e `asc` no `GET /api/transactions` em
      `src/Denarius.WebAPI/Controllers/TransactionsController.cs` (depende de T031).
- [x] T033 [US3] Atualizar `spec.md` (US3, FR-013…FR-018, SC-006), `plan.md`, `research.md`,
      `data-model.md`, `contracts/transactions-api.yaml` (1.1.0), `quickstart.md` e
      `checklists/requirements.md` para documentar o comportamento entregue (depende de T032).

**Ponto de controle**: As três histórias de usuário funcionando de forma independente.

---

## Fase 6: Acabamento e aspectos transversais

- [x] T025 [P] Adicionar testes de integração do `TransactionsController` cobrindo `POST` (201 +
      cabeçalho `Location`), `GET`/`GET {id}`, `PUT {id}`, `DELETE {id}` e os mapeamentos 404/400
      (transação inexistente, categoria inexistente, valor zero) — em
      `tests/Denarius.WebAPI.Tests/Transactions/TransactionsControllerTests.cs` (11 testes).
      Segue o padrão existente de `HostBuilder`/`TestServer` montado à mão de
      `tests/Denarius.WebAPI.Tests/Categories/CategoriesControllerTests.cs` (registra o
      controller via `AddApplicationPart` em vez da composition root completa do `Program`, sem
      precisar de banco), com fakes escritos à mão para as cinco interfaces de casos de uso — este
      projeto de testes não usa biblioteca de mock. Fecha a lacuna apontada em `research.md` →
      Lacuna de cobertura de testes.
- [x] T026 Rodada a validação do `quickstart.md` de ponta a ponta em 2026-09-22 com o .NET SDK
      10.0.401: `dotnet test` — as três suítes, 101/101 passando (Domain.Tests 35,
      Application.Tests 34, WebAPI.Tests 32, incluindo os 11 testes novos da T025) — e depois o
      smoke test manual completo com curl (passos 1-10) contra
      `dotnet run --project src/Denarius.WebAPI` no banco local de desenvolvimento, incluindo os
      passos de descrição em branco, exclusão, não encontrada, validação de valor zero/data
      ausente e referência a categoria desconhecida. Todas as respostas bateram com o esperado
      documentado no `quickstart.md`; a categoria/transação criadas para a execução manual foram
      excluídas depois.
- [x] T034 Rodada de novo a validação do `quickstart.md` para a História de usuário 3 em
      2026-09-24: `dotnet test` — as três suítes, 126/126 passando (Domain.Tests 35,
      Application.Tests 54, WebAPI.Tests 37) — e depois os curls de filtro/ordenação do passo 4
      contra `dotnet run --project src/Denarius.WebAPI` no banco local de desenvolvimento, com
      três transações temporárias numa categoria existente (descrição + mês + tipo + categoria
      combinados, `type=in`, outro mês, uma transação às `23:59:59Z` do último dia do mês,
      ordenações por `Value`/`Description`/`CategoryName` e `400` para
      `type`/`orderBy`/`categoryId` inválidos). Todas as respostas bateram; as transações
      temporárias foram excluídas depois (depende de T033).

---

## Fase 7: Alteração de 2026-10-11 — filtros e ordenação no banco (preparação e fundação)

Pedido e decisões na [spec](./spec.md) (nota de alteração; FR-013 e FR-018 a FR-021, história 4, SC-007 a SC-010) e no
[plan](./plan.md#alteração-de-2026-10-11-filtros-e-ordenação-no-banco). As tarefas T027 a T034 acima descrevem a lista
em memória e continuam como histórico do que foi entregue; a partir daqui, a lista passa a ser uma consulta SQL.

**Testes**: Obrigatórios (princípio I). Em cada história, os testes vêm primeiro e precisam falhar antes do código. Os
testes de repositório usam o `PostgreSqlFixture` (Testcontainers), por isso o Docker precisa estar em execução.

**Organização**: A US3 (busca sem acentos, que já existe como história) e a US4 (desempenho e ordem alfabética)
compartilham o mesmo `ListAsync`. A US3 monta a consulta com a busca, os filtros e a ordenação por data e valor; a US4
acrescenta a ordenação alfabética pt-BR (que exige a migration da collation) e prova o desempenho. **A alteração só é
entregue com as duas**: até a US4 terminar, o `ListAsync` não aceita ordenar por descrição nem por nome da categoria.

### Tabela dos testes atuais de `ListTransactionsUseCaseTests`

Torna rastreável o princípio I: o que cada teste atual verifica passa a ser verificado onde a regra passa a acontecer.
Nenhum é apagado sem substituto.

| Teste atual (`ListTransactionsUseCaseTests`) | Passa a ser |
| --- | --- |
| `Execute_TransactionsExist_ReturnsAllTransactionsMappedToOutput`, `Execute_NoTransactions_ReturnsEmpty` | Caso de uso (adaptado a `ListAsync`) |
| `Execute_NoFilters_ReturnsAllTransactionsOrderedByDateDescending` | Caso de uso: consulta padrão (sem filtros, `Date`, decrescente); repositório: ordem padrão |
| `Execute_WithDescription_ReturnsOnlyMatchingTransactionsIgnoringCase` | Repositório (busca); caso de uso: repassa o texto aparado |
| `Execute_WithBlankDescription_DoesNotFilter` | Caso de uso: branco vira `null` na consulta (mantém a Theory) |
| `Execute_WithDateRef_ReturnsOnlyTransactionsWithinTheMonth` | Repositório (mês); caso de uso: `From`/`To` em UTC |
| `Execute_WithType_...` (Theory), `Execute_WithCategoryId_...`, `Execute_WithCombinedFilters_...` | Repositório; caso de uso: mapeia tipo e categoria |
| `Execute_OrderByDateAscending_...`, `Execute_OrderByValue_...`, `Execute_OrderByDateWithTies_...`, `Execute_OrderByAnotherFieldWithTies_...` | Repositório; caso de uso: mapeia campo e direção |
| `Execute_OrderByDescription_OrdersAlphabetically`, `Execute_OrderByCategoryName_OrdersByTheCategoryName` | Repositório (US4), agora com a regra pt-BR |
| `Execute_NotOrderingByCategoryName_DoesNotLoadCategories` | **Substituído** (aprovado pelo usuário em 2026-10-11): o caso de uso deixa de depender do `ICategoryRepository`; o repositório prova (SQL) que só faz o join ao ordenar por nome da categoria |

### Preparação

**Objetivo**: Ponto de partida e linha de base de desempenho, antes de qualquer mudança de código (SC-008).

- [X] T035 Conferir que o Docker está em execução (`docker version`) e rodar `dotnet test` na solução intocada,
      registrando os totais nesta tarefa. Se o Docker não estiver ligado, parar e pedir ao usuário.
      **Resultado (2026-10-11)**: Docker 29.6.1; `dotnet test` com 51 + 140 + 63 + 10 = 264 testes passando; o build já
      tinha 1 aviso CS8765 em `ValueObject.cs` (preexistente).
- [X] T036 Medir a linha de base do SC-008 com o código atual, conforme [quickstart.md](./quickstart.md) (seção
      "Alteração de 2026-10-11"): base descartável com as migrations atuais, 10.000 transações (200 no mês de teste, 200
      na categoria de teste) geradas por `generate_series`, 3 requisições de aquecimento descartadas e 20 medidas por caso, mediana,
      e `EXPLAIN (ANALYZE)` do mês e da categoria. Preencher a coluna "antes" da tabela em `research.md`. Nada de
      segredo no repositório: a connection string da base descartável vai pela variável de ambiente
      `ConnectionStrings__DenariusDb`, que sobrescreve a do `appsettings.Development.json`.
      **Resultado (2026-10-11)**: mês 32,3 ms e categoria 34,9 ms (rodada 2: 32,0 e 31,8); tabela e plano de execução em
      `research.md`. Os scripts de carga e de medição ficaram fora do repositório.

### Fundação

**Objetivo**: Só declarações, sem comportamento, para os testes das duas histórias compilarem e falharem.

- [X] T037 [P] Criar `src/Denarius.Domain/Repositories/TransactionListQuery.cs` com o record `TransactionListQuery`
      (`Description` já aparada e opcional, `From` inclusive e `To` exclusive como `DateTime?`, `Type` do tipo
      `TransactionKind`, `CategoryId?`, `SortBy`, `Ascending`; o campo se chama `Type`, como o parâmetro `type` da API, e
      não `Kind`, para não se confundir com o `DateTime.Kind` dos testes de UTC) e os enums `TransactionKind` (`All`, `In`, `Out`) e `TransactionSortField` (`Date`,
      `Description`, `Value`, `CategoryName`), conforme [data-model.md](./data-model.md).
- [X] T038 Em `src/Denarius.Domain/Repositories/ITransactionRepository.cs`, declarar
      `Task<IEnumerable<Transaction>> ListAsync(TransactionListQuery query);` mantendo o `GetAllAsync()` (o
      `ListCategoriesUseCase` o usa). Em `src/Denarius.Infrastructure/Repositories/TransactionRepository.cs`, um
      `ListAsync` provisório que lança `NotImplementedException`, para o projeto compilar com os testes falhando.

**Checkpoint**: a solução compila; nenhum comportamento mudou.

---

## Fase 8: História de usuário 3 (alteração) — achar uma transação sem acertar os acentos 🎯

**Objetivo**: A busca por descrição ignora acentos, maiúsculas e espaços das pontas, e a consulta (busca, mês, tipo,
categoria, ordenação por data e valor) acontece no banco.

**Teste independente**: cenários 1 e 7 a 10 da história 3 contra o PostgreSQL descartável, e a combinação de filtros do
cenário 1 da história 4.

### Testes (escrever primeiro; precisam falhar)

- [X] T039 [P] [US3] Criar `tests/Denarius.Infrastructure.Tests/Repositories/TransactionRepositoryTests.cs` (com uma
      fixture que semeia categorias e transações, como o `CategoryRepositoryTests`) cobrindo `ListAsync`: busca com
      "farmacia sao", "FARMÁCIA", "são joão" e "padariá" (cenário 7), "farma" só acha "Farmácia" (8), descrição nula
      nunca aparece, `%`, `_` e `\` como texto (10), "acai"/"AÇAÍ" para "Açaí" (9); mês com transações às 00:00 do dia
      1 e às 23:59:59,999 do último dia, e a primeira fora do mês anterior (cenário 6 da história 4); tipo (`In`:
      `Value > 0`, `Out`: `Value < 0`); categoria, inclusive inexistente (lista vazia); combinação de todos os filtros;
      ordem padrão (`Date` decrescente, depois `CreatedAt`); `Date` crescente com desempates crescentes; `Value` com
      desempates decrescentes; e que o SQL de pelo menos um caso tem os filtros e o `ORDER BY` no banco. O `ListAsync`
      devolve a lista já materializada, então o SQL é capturado do que o contexto de teste executa (`LogTo` ou um
      `DbCommandInterceptor` no `CreateContext` do teste), e as afirmações são sobre o texto capturado. Os dados e as
      expectativas reaproveitam os do teste atual correspondente da tabela acima (SC-009).
- [X] T040 [P] [US3] Reescrever `tests/Denarius.Application.Tests/UseCases/Transactions/List/ListTransactionsUseCaseTests.cs`
      seguindo a tabela acima: o caso de uso sem `ICategoryRepository`, simulando `ListAsync`; afirmar a
      `TransactionListQuery` capturada (descrição aparada; em branco ou só espaços vira `null`; `dateRef` vira `From` no
      dia 1 e `To` no dia 1 do mês seguinte, ambos com `Kind == DateTimeKind.Utc`, mesmo com `dateRef` `Unspecified`,
      `Utc` ou `Local`; tipo, categoria, campo e direção mapeados) e o mapeamento do resultado para `TransactionOutput`.
- [X] T041 [US3] Em `tests/Denarius.Infrastructure.Tests/Repositories/TransactionRepositoryTests.cs`, acrescentar o
      teste do caso de uso real (`ListTransactionsUseCase` com o `TransactionRepository` do PostgreSQL) para `dateRef`
      `Unspecified`, `Utc` e `Local`: nenhum responde erro e todos devolvem o mesmo mês. Depende de T039.

### Implementação

- [X] T042 [US3] Implementar `ListAsync` em `src/Denarius.Infrastructure/Repositories/TransactionRepository.cs`:
      `AsNoTracking()`; busca por `EF.Functions.Unaccent(Description).ToLower().Contains(EF.Functions.Unaccent(texto).ToLower())`
      (o mesmo filtro de `CategoryRepository.cs:14-15`); mês `Date >= From && Date < To`; tipo; categoria; ordenação por
      `Date` e `Value` (direção do `Ascending`) com os desempates `Date` e `CreatedAt` (decrescentes, exceto em `Date`
      crescente). `Description` e `CategoryName` lançam `NotImplementedException` até a US4. Faz T039 e T041 passarem.
- [X] T043 [US3] Em `src/Denarius.Application/UseCases/Transactions/List/ListTransactionsUseCase.cs`: remover o
      `ICategoryRepository` e a filtragem em memória; aparar a descrição (em branco vira `null`), montar `From` e `To`
      em UTC a partir do ano e do mês do `dateRef`, converter `TransactionType`/`TransactionOrderField` nos tipos do
      `Domain`, chamar `ListAsync` e mapear para `TransactionOutput`. Faz T040 passar. Conferir a injeção em
      `src/Denarius.Application` e em `src/Denarius.WebAPI` (nenhum registro novo é esperado).

**Checkpoint**: a busca ignora acentos de ponta a ponta; a ordenação por descrição e nome da categoria falta (US4).

---

## Fase 9: História de usuário 4 — lista que não fica mais lenta com o histórico

**Objetivo**: Ordem alfabética pt-BR igual em qualquer ambiente, a mesma ordem de antes nos demais campos e o ganho de
desempenho do SC-008.

**Teste independente**: cenários 2 a 6 da história 4, a migration aplicada, revertida e reaplicada, e a tabela de
medição preenchida.

### Testes (escrever primeiro; precisam falhar)

- [X] T044 [P] [US4] Criar `tests/Denarius.Infrastructure.Tests/Migrations/PortugueseCollationMigrationTests.cs`: com o
      `IMigrator`, migrar até a migration anterior, aplicar a `AddPortugueseCollation`, conferir em `pg_collation` que
      `pt_br_icu` existe (provedor `icu`), reverter e conferir que sumiu, e reaplicar. Também confere a saída de
      `SELECT collname, collprovider, colllocale FROM pg_collation WHERE collname = 'pt_br_icu'`.
- [X] T045 [P] [US4] Em `tests/Denarius.Infrastructure.Tests/Repositories/TransactionRepositoryTests.cs`, acrescentar
      os testes de ordenação por `Description`: "Banana", "Água", "agua", "água" e "Agua" dão "agua, Agua, água, Água,
      Banana" no crescente e a inversa no decrescente (cenário 5); um caso com espaço, hífen e dígito ("Água de coco",
      "Aguardente", "Água-viva", "Agua 2") que registra a regra efetiva da ICU; transações sem descrição no início do
      crescente e no fim do decrescente (cenário 3); descrições idênticas caem nos desempates de data e criação. E por
      `CategoryName`: usa o nome atual da categoria (renomeada depois da transação), nas duas direções, com os
      desempates; o SQL capturado do `ListAsync` (do mesmo modo da T039) só tem o join com `Categories` quando `SortBy` é
      `CategoryName`. Os dados e as expectativas das ordenações por `Description` e `CategoryName` reaproveitam os dos
      testes atuais correspondentes da tabela acima, acrescidos dos casos novos pt-BR (SC-009).

### Implementação

- [X] T046 [US4] Em `src/Denarius.Infrastructure/Persistence/DenariusDbContext.cs`, declarar
      `modelBuilder.HasCollation("pt_br_icu", locale: "pt-BR", provider: "icu", deterministic: true)` e gerar a migration
      com `dotnet ef migrations add AddPortugueseCollation --project src/Denarius.Infrastructure --startup-project src/Denarius.WebAPI`.
      Conferir o SQL gerado (`dotnet ef migrations script`). Faz T044 passar.
      **Resultado (2026-10-11)**: migration `20261011184255_AddPortugueseCollation`. O `Up()` gera `CREATE COLLATION
      pt_br_icu (LOCALE = 'pt-BR', PROVIDER = icu, DETERMINISTIC = True)` e o `Down()` gera `DROP COLLATION pt_br_icu`,
      ambos numa transação; o SQL explícito previsto no plan não foi preciso (o revisor do plan supôs que o Npgsql não
      geraria o `DROP COLLATION`, e a conferência mostrou que gera). Os dois testes da T044 passam.
- [X] T047 [US4] Em `src/Denarius.Infrastructure/Repositories/TransactionRepository.cs`, implementar as ordenações
      `Description` (`OrderBy(d != null)` seguido de `EF.Functions.Collate(Description, "pt_br_icu")`, de modo que os
      sem descrição fiquem no início do crescente e no fim do decrescente) e `CategoryName` (join com `Categories` só
      nesse caso, ordenando por `EF.Functions.Collate(Name, "pt_br_icu")`), ambas com os desempates do FR-018. Conferir
      o SQL gerado. Faz T045 passar.
- [X] T048 [US4] Medir de novo, com o código novo e a mesma carga da T036, e preencher a coluna "depois" e a razão em
      `research.md`, junto do `EXPLAIN (ANALYZE)`. Se a razão do mês ou da categoria ficar abaixo de 5, parar e levar o
      resultado ao usuário (SC-008).
      **Resultado (2026-10-11)**: mês 31,5 → 4,4 ms (7,2 vezes) e categoria 32,3 → 4,3 ms (7,5 vezes), no estado aquecido;
      nas duas primeiras rodadas após a partida da API nova (ainda aquecendo), 4,4 a 6,0 vezes. Meta de 5 vezes atingida;
      detalhes, método e planos de execução em `research.md`. Os scripts e a base descartável ficaram fora do repositório.

**Checkpoint**: as duas histórias funcionam; a migration é reversível; o SC-008 está medido.

---

## Fase 10: Fechamento da alteração de 2026-10-11

- [X] T049 [P] Conferir que o contrato `contracts/transactions-api.yaml` (1.3.0) continua igual ao que o código faz
      (busca, ordem e mês) e que nenhum parâmetro ou resposta mudou: `Denarius.WebAPI.Tests` passa sem alteração.
      **Resultado (2026-10-11)**: o controller e os 63 testes de `Denarius.WebAPI.Tests` não mudaram (`git status` limpo
      nesses caminhos) e passam; o contrato 1.3.0 descreve a busca (`unaccent`), a ordem (collation `pt_br_icu`, nulos) e o
      mês (`[início, início do seguinte)`) como o código faz.
- [X] T050 [P] Conferir por busca que não há `EnableSensitiveDataLogging` em `src/` e que nenhum log novo registra o
      texto buscado nem descrições (FR-021).
      **Resultado (2026-10-11)**: nenhum `EnableSensitiveDataLogging` nem `EnableDetailedErrors` em `src/`, e nenhuma
      chamada de log (`ILogger`, `Console`) no código; o teste `ListAsync_RunsTheFiltersAndTheOrderingInTheDatabase` afirma
      que o SQL registrado não traz o texto buscado (os parâmetros aparecem como `'?'`).
- [X] T051 A partir de `back/`, rodar `dotnet build` (sem aviso novo) e `dotnet test` (as quatro suítes) e registrar os
      totais nesta tarefa. Depois, a revisão de fim do implement (agente `reviewer`).
      **Resultado (2026-10-11)**: `dotnet build --no-incremental` com 0 avisos novos (só o CS8765 de `ValueObject.cs`,
      preexistente); `dotnet test` com 51 + 137 + 63 + 53 = 304 testes passando (eram 264 antes da alteração). Revisão
      de fim do implement (agente `reviewer`): aprovada, sem achado CRITICAL, HIGH nem MEDIUM. Quatro LOW, todos aceitos:
      os desempates dos testes dependem de `Task.Delay(5)` para separar o `CreatedAt` (o construtor de `Transaction` fixa
      `DateTime.UtcNow`); o nome da migration anterior está fixo no teste de migration (uma renomeação falharia de forma
      clara); a ordenação não termina em `Id` como último desempate (o comportamento anterior também não definia a ordem
      de linhas idênticas); a T051 estava por marcar.
- [X] T052 Só com o aval do usuário: aplicar as migrations pendentes na base de dev (`dotnet ef database update`, que
      aplica a `AddUnaccentExtension` e a `AddPortugueseCollation`) antes de reiniciar a API; sem isso a lista
      responde `500`.
      **Resultado (2026-10-11, com o aval do usuário)**: as duas migrations pendentes foram aplicadas na base de dev
      (`AddUnaccentExtension` e `AddPortugueseCollation`); `dotnet ef migrations list` mostra as cinco aplicadas. Teste de
      fumaça só de leitura com a API contra a base de dev: lista sem filtro, ordenações por descrição (nas duas
      direções), nome da categoria e valor, e a combinação de busca, mês e tipo responderam `200` (298 transações).

- [X] T053 Só com o aval do usuário e depois da T051: pela `/speckit-constitution`, atualizar a restrição de performance do
      `back/AGENTS.md`, que hoje diz que "as listagens de transações e de categorias ainda filtram em memória": passa a
      valer só para a de categorias, e a versão da constituição é atualizada (PATCH ou MINOR, conforme a Governança).
      **Resultado (2026-10-11, com o aval do usuário)**: `back/AGENTS.md` (Restrições técnicas, Performance) passa a dizer que a
      lista de transações filtra e ordena no banco e que só a listagem de categorias ainda filtra em memória; constituição
      3.0.1 → 3.0.2 (PATCH: redação, sem mudar o que é exigido), última alteração 2026-10-11.

### Depois da alteração (fora deste `tasks.md`, com o aval do usuário)

- No front, atualizar pelo fluxo do front o FR de busca da spec 003, que diz que a busca diferencia acentos.

### Dependências da alteração

- T053 depende de T051 e do aval do usuário. T035 e T036 antes de qualquer mudança de código; T037 antes de T038; T038 antes dos testes (T039, T040).
- US3: T039 e T040 são independentes ([P]); T041 depende de T039; T042 depende de T039 e T041; T043 depende de T040 e de T042.
- US4 depende da US3 (o mesmo `ListAsync`). T044 e T045 são independentes ([P]); T046 depende de T044; T047 depende de
  T045 e T046; T048 depende de T047.
- Fechamento depois da US4: T049 e T050 em paralelo; T051 depois deles; T052 e T053 por último e só com aval.
- Não publicar entre a US3 e a US4: até a T047, ordenar por descrição ou por nome da categoria lança
  `NotImplementedException`.

---

## Dependências e ordem de execução

### Dependências entre fases

- **Preparação (Fase 1)**: Sem dependências — preexistente, compartilhada.
- **Fundação (Fase 2)**: Depende da Preparação — bloqueia a história de usuário. Preexistente;
  também depende da existência da entidade/tabela `Category` de
  [001-category-management](../001-category-management/tasks.md) (T003, T006 de lá) como alvo da
  FK.
- **Histórias de usuário (Fases 3-5)**: Todas dependem da Fundação. Construídas na ordem de
  prioridade (P1 → P2 → P3); cada uma continua testável de forma independente.
- **Acabamento (Fase 6)**: Depende das histórias de usuário que cobre. Concluída.

### Dependências entre histórias de usuário

- **História de usuário 1 (P1)**: Sem dependência da US2. Depende da US1 de
  [001-category-management](../001-category-management/tasks.md) (precisa existir uma categoria
  para vincular a transação).
- **História de usuário 2 (P2)**: Se apoia na estrutura do `List`, que a US1 não toca (a US1
  nunca chama o `ListTransactionsUseCase`); testável de forma independente.
- **História de usuário 3 (P3)**: Estende o caso de uso e o endpoint `List` da US2 (T023/T024);
  sem parâmetros, a lista se comporta como a US2 descreve. A ordenação por `CategoryName` também
  lê as categorias do repositório de [001-category-management](../001-category-management/tasks.md).

### Oportunidades de paralelismo

- T003/T004/T005/T006 (Fundação): arquivos diferentes; a T004 depende só da T003, e a T005/T006
  dependem só da T003-T005, respectivamente.
- T010-T014 (testes da US1): cinco arquivos diferentes.
- T015/T016 (DTOs da US1): arquivos diferentes.
- T027/T028 (testes da US3) e T029 (enums da US3): arquivos diferentes.
- A T025 não depende da T026, e vice-versa (a mesma relação das T032/T033 de
  001-category-management).

---

## Exemplo de paralelismo: história de usuário 1

```bash
# Os cinco arquivos de teste da US1 não dependem uns dos outros:
Task: "Testes de validação/trim/update de Transaction em tests/Denarius.Domain.Tests/Entities/TransactionTests.cs"
Task: "Testes do CreateTransactionUseCase em tests/Denarius.Application.Tests/UseCases/Transactions/Create/CreateTransactionUseCaseTests.cs"
Task: "Testes do UpdateTransactionUseCase em tests/Denarius.Application.Tests/UseCases/Transactions/Update/UpdateTransactionUseCaseTests.cs"
Task: "Testes do DeleteTransactionUseCase em tests/Denarius.Application.Tests/UseCases/Transactions/Delete/DeleteTransactionUseCaseTests.cs"
Task: "Testes do GetTransactionByIdUseCase em tests/Denarius.Application.Tests/UseCases/Transactions/GetById/GetTransactionByIdUseCaseTests.cs"
```

---

## Estratégia de implementação

### MVP primeiro (só a história de usuário 1)

Foi assim que a feature foi de fato entregue: Preparação + Fundação e depois a US1 (CRUD), entregue
como um incremento utilizável antes de existir a visão completa da lista (US2).

### Entrega incremental (como aconteceu)

1. Preparação + Fundação → `Transaction`, persistência, controller vazio (entregues junto com a
   fundação de [001-category-management](../001-category-management/tasks.md), no mesmo
   intervalo de commits).
2. US1 → CRUD de transações com a proteção de existência da categoria — implantável sozinho.
3. US2 → visão da lista sem filtros, adicionada na mesma mudança.
4. Acabamento (T025/T026) → fechado pelo `/speckit-implement` em 2026-09-22, como
   001-category-management fechou a lacuna equivalente.
5. US3 (T027-T033) → filtros por descrição/mês/tipo/categoria e quatro campos de ordenação
   adicionados em 2026-09-24, como mudança aditiva ao `GET /api/transactions`; validada pela T034.

### O que falta

A Fase 6 (a lacuna de testes da camada HTTP do `TransactionsController` e uma execução
registrada do quickstart) foi fechada pelo `/speckit-implement` em 2026-09-22, e a História de
usuário 3 foi concluída em 2026-09-24. Resta a alteração de 2026-10-11 (Fases 7 a 10, T035 a T053): filtros e
ordenação no banco, busca sem acentos e ordem alfabética pt-BR.

---

## Observações

- `[x]` = implementada e (quando se aplica) testada; as T001 a T034 estão concluídas, e as T035 a T053 (alteração de
  2026-10-11) estão abertas.
- Os caminhos de arquivo são exatos — esta lista também serve de mapa de rastreabilidade de
  requisito para código dos FR-001…FR-021 do `spec.md`.
- As T027-T034 foram numeradas depois das tarefas originais de Acabamento (T025/T026) porque foram
  adicionadas mais tarde; a ordem das fases, e não a dos IDs, reflete a prioridade das histórias.

---

## Fase 11: Convergência (2026-10-11)

Achado do `/speckit-converge` sobre a alteração de 2026-10-11: os testes do repositório cobrem a semântica dos testes
antigos de `ListTransactionsUseCaseTests`, mas com dados próprios, e não de forma literal como a T039 e a T045 pediam
(SC-009). Os dados e as expectativas antigos estão em `git show HEAD:back/tests/Denarius.Application.Tests/UseCases/Transactions/List/ListTransactionsUseCaseTests.cs`.

- [X] T054 Em `tests/Denarius.Infrastructure.Tests/Repositories/TransactionRepositoryTests.cs`, alinhar aos testes antigos
      os dados e as expectativas dos testes de mês (agosto de 2026: "Início do mês", "Fim do mês" às 23:59:59, "Mês
      seguinte", "Mês anterior"), tipo, categoria (Lazer, Mercado; "Show" antes de "Cinema" e afirmação do `CategoryId`
      de cada resultado), combinação (inclusive "Cinema reembolso", positivo), valor (Farmácia, Salário, Aluguel),
      descrição (Aluguel, Farmácia, Mercado, somados aos casos pt-BR) e nome da categoria (Lazer, Mercado, Trabalho;
      "Cinema", "Feira", "Salário"), mantendo os testes novos já existentes. per SC-009 (partial)
- [X] T055 No mesmo arquivo, trocar os `await Task.Delay(5)` do helper `AddAsync` por `CreatedAt` explícito e crescente
      (`new Transaction(...) { CreatedAt = ... }`, o `init` é público), como faziam os testes antigos, para os desempates
      não dependerem do relógio. Depende da T054. per SC-009 (partial)

**Resultado (2026-10-11, T054 e T055)**: os testes de repositório de mês, tipo, categoria (com a afirmação do `CategoryId`),
combinação ("Cinema reembolso"), valor, descrição, nome da categoria e desempates passaram a usar os dados e as expectativas
dos testes antigos (os testes anteriores, com dados próprios, foram mantidos quando cobriam algo a mais: limites do mês
com milissegundos, acentos na combinação, renomeação da categoria, pt-BR). O helper `AddAsync` fixa o `CreatedAt`
(crescente, por minuto) em vez de `Task.Delay(5)`. `dotnet test`: 51 + 62 + 63 + 137 = 313 testes passando, estável em
três execuções da suíte de infraestrutura.

- [X] T056 Pedido do usuário em 2026-10-11, fora do escopo da alteração mas dentro de `TransactionRepository.cs`: fazer o
      `GetAllAsync()` ler sem rastreamento (`AsNoTracking()`), como pede a restrição de performance do `back/AGENTS.md`.
      Primeiro o teste em `tests/Denarius.Infrastructure.Tests/Repositories/TransactionRepositoryTests.cs` (as entidades
      devolvidas não ficam no `ChangeTracker`), que precisa falhar; depois a correção. O único consumidor é o
      `ListCategoriesUseCase`, que só lê. per Constitution (back/AGENTS.md, Performance) (contradicts)
      **Resultado (2026-10-11)**: teste `GetAllAsync_ReturnsTheTransactionsWithoutTrackingThem` falhou antes e passou depois
      de `AsNoTracking()` em `TransactionRepository.cs:13`; `dotnet test` com 51 + 63 + 63 + 137 = 314 testes passando. A
      `CategoryRepository.GetAllAsync(name)` também não usa `AsNoTracking()` (mesma restrição), mas é da listagem de
      categorias e ficou de fora.
