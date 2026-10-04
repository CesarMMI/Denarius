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

Nada. A Fase 6 (a lacuna de testes da camada HTTP do `TransactionsController` e uma execução
registrada do quickstart) foi fechada pelo `/speckit-implement` em 2026-09-22, e a História de
usuário 3 foi concluída em 2026-09-24.

---

## Observações

- `[x]` = implementada e (quando se aplica) testada; as 34 tarefas estão concluídas.
- Os caminhos de arquivo são exatos — esta lista também serve de mapa de rastreabilidade de
  requisito para código dos FR-001…FR-018 do `spec.md`.
- As T027-T034 foram numeradas depois das tarefas originais de Acabamento (T025/T026) porque foram
  adicionadas mais tarde; a ordem das fases, e não a dos IDs, reflete a prioridade das histórias.
