---

description: "Lista de tarefas da feature Gestão de categorias"
---

# Tarefas: Gestão de categorias

**Entrada**: Documentos de design em `/specs/001-category-management/`

**Pré-requisitos**: plan.md (presente), spec.md (presente), research.md (presente),
data-model.md (presente), contracts/ (presente)

**Status**: Esta é uma lista de tarefas **retroativa** — a gestão de categorias já está
implementada. `[x]` marca uma tarefa já atendida pelo código/testes existentes (com o caminho do
arquivo, para que o mapeamento de requisito para implementação seja rastreável). O
`/speckit-implement` rodou em 2026-09-21 e fechou os dois itens que ainda estavam pendentes (T032,
T033) — as 33 tarefas agora estão `[x]`.

**Testes**: As tarefas de teste abaixo refletem os testes automatizados que já existem por
história; nenhuma tarefa de teste nova foi adicionada além da única lacuna de cobertura explícita
(T032), que o `research.md` já tinha apontado, em vez de um genérico "adicionar mais testes".

**Organização**: As tarefas estão agrupadas por história de usuário, conforme as prioridades do
`spec.md` (P1/P2/P3).

## Formato: `[ID] [P?] [História] Descrição`

- **[P]**: Arquivos diferentes, sem dependência de uma tarefa incompleta
- **[História]**: A história de usuário a que a tarefa pertence (US1, US2, US3)
- Os caminhos de arquivo são exatos, relativos à raiz do repositório

## Fase 1: Preparação (infraestrutura compartilhada)

**Objetivo**: Inicialização do projeto. É compartilhada por todas as features do backend, e não
foi criada especificamente para Category — está aqui só para que esta lista siga a estrutura de
fases exigida.

- [x] T001 Confirmar que a solução de Clean Architecture com quatro projetos existe, conforme o
      `Denarius.slnx` (`Denarius.Domain`, `Denarius.Application`, `Denarius.Infrastructure`,
      `Denarius.WebAPI`, com os projetos `*.Tests` correspondentes) — preexistente.
- [x] T002 [P] Confirmar que as referências de pacote do EF Core +
      `Npgsql.EntityFrameworkCore.PostgreSQL` (em
      `src/Denarius.Infrastructure/Denarius.Infrastructure.csproj`) e do xUnit + NSubstitute /
      `Microsoft.AspNetCore.TestHost` (nos três `tests/*.csproj`) estão presentes —
      preexistente.

---

## Fase 2: Fundação (pré-requisitos bloqueantes)

**Objetivo**: A entidade, a persistência e a estrutura do controller de que todas as histórias de
usuário abaixo dependem.

**⚠️ CRÍTICO**: Nenhuma história de usuário funciona sem esta fase.

- [x] T003 [P] Entidade `Category` — `Name`: "obrigatório; com trim; 1-100 caracteres depois do
      trim; lança DomainException caso contrário" — em `src/Denarius.Domain/Entities/Category.cs`.
- [x] T004 [P] Value object `Color` — "normalizado para `#RRGGBB` em maiúsculas; aceita entrada com
      ou sem `#` inicial e a forma abreviada de 3 dígitos; qualquer valor que não case com
      `^#[0-9A-F]{6}$` depois da normalização lança DomainException" — em
      `src/Denarius.Domain/ValueObjects/Color.cs`.
- [x] T005 `ICategoryRepository` (estende `IRepository<Category>` e adiciona
      `GetAllAsync(string? name)`) em `src/Denarius.Domain/Repositories/ICategoryRepository.cs`
      (depende de T003).
- [x] T006 [P] Mapeamento EF `CategoryConfiguration` — `Name` `character varying(100)` not null,
      `Color` `character varying(7)` not null via value converter — em
      `src/Denarius.Infrastructure/Persistence/Configurations/CategoryConfiguration.cs`
      (depende de T003, T004).
- [x] T007 [P] `CategoryRepository.GetAllAsync(string? name)` (filtro `Name.Contains(name)`) em
      `src/Denarius.Infrastructure/Repositories/CategoryRepository.cs` (depende de T005).
- [x] T008 Migration da tabela `Categories` com um `Down()` limpo e reversível (`DropTable`) em
      `src/Denarius.Infrastructure/Migrations/20260813225006_InitialCreate.cs` (depende de
      T006) — atende ao Princípio III da constituição.
- [x] T009 FK restrict-on-delete `Transactions.CategoryId → Categories.Id` em
      `src/Denarius.Infrastructure/Persistence/Configurations/TransactionConfiguration.cs` e
      `src/Denarius.Infrastructure/Migrations/20260814132713_AddTransaction.cs` (depende de
      T008) — a garantia no banco para o FR-009.
- [x] T010 Registrar `ICategoryRepository → CategoryRepository` em
      `src/Denarius.Infrastructure/DependencyInjection.cs` (depende de T007).
- [x] T011 Criar a estrutura do `CategoriesController`, com o `GlobalExceptionHandler` mapeando
      `NotFoundException → 404` e `DomainException`/`AppException → 400`, em
      `src/Denarius.WebAPI/Controllers/CategoriesController.cs` e
      `src/Denarius.WebAPI/Middleware/GlobalExceptionHandler.cs` (depende de T010).

**Ponto de controle**: Fundação pronta — as três histórias de usuário se apoiam nela.

---

## Fase 3: História de usuário 1 - Montar e manter uma lista de categorias (Prioridade: P1) 🎯 MVP

**Objetivo**: Criar, visualizar, renomear/mudar a cor e excluir categorias; bloquear a exclusão de
uma categoria que ainda tem transações.

**Teste independente**: Criar uma categoria, confirmar que ela aparece com o nome/cor escolhidos,
editá-la e excluí-la — sem precisar de transações.

### Testes da história de usuário 1 (já existem)

- [x] T012 [P] [US1] Testes de validação/trim/update de `Category` em
      `tests/Denarius.Domain.Tests/Entities/CategoryTests.cs`.
- [x] T013 [P] [US1] Testes do `CreateCategoryUseCase` em
      `tests/Denarius.Application.Tests/UseCases/Categories/Create/CreateCategoryUseCaseTests.cs`.
- [x] T014 [P] [US1] Testes do `UpdateCategoryUseCase` em
      `tests/Denarius.Application.Tests/UseCases/Categories/Update/UpdateCategoryUseCaseTests.cs`.
- [x] T015 [P] [US1] Testes do `DeleteCategoryUseCase` — categoria não encontrada e proteção de
      categoria com transações — em
      `tests/Denarius.Application.Tests/UseCases/Categories/Delete/DeleteCategoryUseCaseTests.cs`.
- [x] T016 [P] [US1] Testes do `GetCategoryByIdUseCase` em
      `tests/Denarius.Application.Tests/UseCases/Categories/GetById/GetCategoryByIdUseCaseTests.cs`.

### Implementação da história de usuário 1

- [x] T017 [P] [US1] DTOs `CreateCategoryInput`/`UpdateCategoryInput` (`name`, `color`) em
      `src/Denarius.Application/IO/Categories/CreateCategoryInput.cs` e
      `UpdateCategoryInput.cs`.
- [x] T018 [P] [US1] DTO `CategoryOutput` em
      `src/Denarius.Application/IO/Categories/CategoryOutput.cs`.
- [x] T019 [US1] `ICreateCategoryUseCase`/`CreateCategoryUseCase` em
      `src/Denarius.Application/UseCases/Categories/Create/` (depende de T017, T018).
- [x] T020 [US1] `IUpdateCategoryUseCase`/`UpdateCategoryUseCase` — 404 via `NotFoundException`
      quando a categoria não existe — em `src/Denarius.Application/UseCases/Categories/Update/`
      (depende de T017, T018).
- [x] T021 [US1] `IDeleteCategoryUseCase`/`DeleteCategoryUseCase` — "DEVE recusar a exclusão de
      uma categoria que tenha uma ou mais transações associadas" via
      `ITransactionRepository.ExistsByCategoryIdAsync` — em
      `src/Denarius.Application/UseCases/Categories/Delete/` (depende de T018).
- [x] T022 [US1] `IGetCategoryByIdUseCase`/`GetCategoryByIdUseCase` — 404 quando não existe — em
      `src/Denarius.Application/UseCases/Categories/GetById/` (depende de T018).
- [x] T023 [US1] Ligar as actions `POST`, `PUT {id}`, `DELETE {id}` e `GET {id}` em
      `src/Denarius.WebAPI/Controllers/CategoriesController.cs` (depende de T019-T022).

**Ponto de controle**: História de usuário 1 totalmente funcional e testável de forma
independente.

---

## Fase 4: História de usuário 2 - Ver quanto cada categoria é usada (Prioridade: P2)

**Objetivo**: Cada categoria informa a sua quantidade de transações e o seu saldo, opcionalmente
restritos a um mês do calendário.

**Teste independente**: Registrar um conjunto conhecido de transações numa categoria; confirmar
que a lista informa a quantidade/saldo corretos para ela e zero/zero para uma categoria sem uso.

### Testes da história de usuário 2 (já existem)

- [x] T024 [US2] Testes de agregação e de recorte por mês do `ListCategoriesUseCase`
      (`Execute_CalculatesTransactionCountAndBalancePerCategory`,
      `Execute_WithDateRef_OnlyConsidersTransactionsWithinTheMonth`) em
      `tests/Denarius.Application.Tests/UseCases/Categories/List/ListCategoriesUseCaseTests.cs`.

### Implementação da história de usuário 2

- [x] T025 [US2] Campos `TransactionCount`/`Balance` em `CategoryOutput`, em
      `src/Denarius.Application/IO/Categories/CategoryOutput.cs`.
- [x] T026 [US2] `ListCategoriesInput.DateRef` + agregação em
      `IListCategoriesUseCase`/`ListCategoriesUseCase` (`GroupBy(t => t.CategoryId)`, intervalo
      do mês derivado do `dateRef`) em
      `src/Denarius.Application/IO/Categories/ListCategoriesInput.cs` e
      `src/Denarius.Application/UseCases/Categories/List/ListCategoriesUseCase.cs` (depende de
      T025, T007).
- [x] T027 [US2] Ligar o `GET /api/categories` com o parâmetro de query `dateRef` em
      `src/Denarius.WebAPI/Controllers/CategoriesController.cs` (depende de T026).

**Ponto de controle**: Histórias de usuário 1 e 2 funcionando de forma independente.

---

## Fase 5: História de usuário 3 - Filtrar uma lista longa de categorias (Prioridade: P3)

**Objetivo**: Buscar por nome, filtrar por uso e ordenar por nome/quantidade/saldo.

**Teste independente**: Criar várias categorias com nomes/usos variados; confirmar que a busca, o
filtro em uso/sem uso e cada opção de ordenação, cada um por si, filtram ou reordenam a lista.

### Testes da história de usuário 3 (já existem)

- [x] T028 [US3] Testes do `ListCategoriesUseCase` para o filtro de nome, o filtro
      `withTransaction` e cada combinação de `orderBy`/`asc` em
      `tests/Denarius.Application.Tests/UseCases/Categories/List/ListCategoriesUseCaseTests.cs`.

### Implementação da história de usuário 3

- [x] T029 [US3] Enum `CategoryOrderField` (`Name`, `TransactionCount`, `Balance`) em
      `src/Denarius.Application/IO/Categories/CategoryOrderField.cs`.
- [x] T030 [US3] Filtro `withTransaction` e o switch de ordenação `orderBy`/`asc` em
      `src/Denarius.Application/UseCases/Categories/List/ListCategoriesUseCase.cs` (depende de
      T026, T029).
- [x] T031 [US3] Ligar os parâmetros de query `name`, `withTransaction`, `orderBy` e `asc` no
      `GET /api/categories` em `src/Denarius.WebAPI/Controllers/CategoriesController.cs`
      (depende de T030).

**Ponto de controle**: As três histórias de usuário funcionando de forma independente.

---

## Fase 6: Acabamento e aspectos transversais

- [x] T032 [P] Adicionar testes de integração do `CategoriesController` cobrindo `POST` (201 +
      cabeçalho `Location`), `GET`/`GET {id}`, `PUT {id}`, `DELETE {id}` e os mapeamentos
      404/400, além do binding dos parâmetros de query do `List`
      (`name`/`withTransaction`/`dateRef`/`orderBy`/`asc`) — em
      `tests/Denarius.WebAPI.Tests/Categories/CategoriesControllerTests.cs` (11 testes). Segue o
      padrão existente de `HostBuilder`/`TestServer` montado à mão de
      `tests/Denarius.WebAPI.Tests/Cors/` e `.../Middleware/` (registra o controller via
      `AddApplicationPart` em vez da composition root completa do `Program`, sem precisar de
      banco), com fakes escritos à mão para as cinco interfaces de casos de uso — este projeto de
      testes não usa biblioteca de mock. Fecha a lacuna apontada em `research.md` → Lacuna de
      cobertura de testes.
- [x] T033 Rodada a validação do `quickstart.md` de ponta a ponta em 2026-09-21 com o .NET SDK
      10.0.401: `dotnet test` — as três suítes, 90/90 passando (Domain.Tests 35,
      Application.Tests 34, WebAPI.Tests 21, incluindo os 11 testes novos da T032) — e depois o
      smoke test manual completo com curl (passos 1-10) contra
      `dotnet run --project src/Denarius.WebAPI` no banco local de desenvolvimento, incluindo os
      passos da proteção de exclusão e das estatísticas de uso por mês com um
      `POST /api/transactions` real. Todas as respostas bateram com o esperado documentado no
      `quickstart.md`; as categorias/transação criadas para a execução manual foram excluídas
      depois.

---

## Dependências e ordem de execução

### Dependências entre fases

- **Preparação (Fase 1)**: Sem dependências — preexistente.
- **Fundação (Fase 2)**: Depende da Preparação — bloqueia todas as histórias de usuário.
  Preexistente.
- **Histórias de usuário (Fases 3-5)**: Todas dependem da Fundação. Historicamente, foram
  construídas na ordem de prioridade (P1 → P2 → P3); cada uma continua testável de forma
  independente.
- **Acabamento (Fase 6)**: Depende das histórias de usuário que cobre. Concluída.

### Dependências entre histórias de usuário

- **História de usuário 1 (P1)**: Sem dependência de US2/US3.
- **História de usuário 2 (P2)**: Se apoia na estrutura do `List`, que a US1 não toca (a US1
  nunca chama o `ListCategoriesUseCase`); testável de forma independente, sem os
  filtros/ordenação da US3.
- **História de usuário 3 (P3)**: Estende o mesmo `ListCategoriesUseCase` da US2 (T026 → T030);
  depende da existência do `ListCategoriesInput`/`ListCategoriesUseCase` da US2, mas o seu próprio
  comportamento de filtro/ordenação é testável de forma independente.

### Oportunidades de paralelismo

- T003/T004 (Fundação): arquivos diferentes, sem dependência compartilhada.
- T006/T007 (Fundação): arquivos diferentes, as duas dependem só de T003-T005.
- T012-T016 (testes da US1): cinco arquivos diferentes.
- T017/T018 (DTOs da US1): arquivos diferentes.
- T032 (Acabamento) não depende de T033, e vice-versa.

---

## Exemplo de paralelismo: história de usuário 1

```bash
# Os cinco arquivos de teste da US1 não dependem uns dos outros:
Task: "Testes de validação/trim/update de Category em tests/Denarius.Domain.Tests/Entities/CategoryTests.cs"
Task: "Testes do CreateCategoryUseCase em tests/Denarius.Application.Tests/UseCases/Categories/Create/CreateCategoryUseCaseTests.cs"
Task: "Testes do UpdateCategoryUseCase em tests/Denarius.Application.Tests/UseCases/Categories/Update/UpdateCategoryUseCaseTests.cs"
Task: "Testes do DeleteCategoryUseCase em tests/Denarius.Application.Tests/UseCases/Categories/Delete/DeleteCategoryUseCaseTests.cs"
Task: "Testes do GetCategoryByIdUseCase em tests/Denarius.Application.Tests/UseCases/Categories/GetById/GetCategoryByIdUseCaseTests.cs"
```

---

## Estratégia de implementação

### MVP primeiro (só a história de usuário 1)

Foi assim que a feature foi de fato entregue: Preparação + Fundação e depois a US1 (CRUD), entregue
como um incremento utilizável antes de existirem as estatísticas de uso (US2) ou a
busca/ordenação (US3).

### Entrega incremental (como aconteceu)

1. Preparação + Fundação → `Category`/`Color`, persistência, controller vazio.
2. US1 → CRUD básico de categorias com a proteção de exclusão — implantável sozinho.
3. US2 → quantidade de transações/saldo incorporados (`feat(api): add category transaction count,
   balance, filters and sorting`).
4. US3 → busca, filtro de uso e ordenação adicionados na mesma mudança da US2.

### O que falta

Nada. A Fase 6 (a lacuna de testes da camada HTTP do `CategoriesController` e uma execução
registrada do quickstart) foi fechada pelo `/speckit-implement` em 2026-09-21.

---

## Observações

- `[x]` = implementada e (quando se aplica) testada; as 33 tarefas estão concluídas.
- Os caminhos de arquivo são exatos — esta lista também serve de mapa de rastreabilidade de
  requisito para código dos FR-001…FR-015 do `spec.md`.
