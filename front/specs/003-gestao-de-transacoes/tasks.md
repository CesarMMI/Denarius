---
description: 'Lista de tarefas da feature Gestão de transações (retroativa)'
---

# Tarefas: Gestão de transações

**Entrada**: Documentos de design em `front/specs/003-gestao-de-transacoes/`

**Pré-requisitos**: plan.md (presente), spec.md (presente), research.md (presente), data-model.md (presente),
contracts/ (presente: `transactions-ui.md`), quickstart.md (presente)

**Feature retroativa**: o código de `front/src/app/transactions/` já implementa a feature. As tarefas que correspondem
ao que já existe estão marcadas `[X]`, com o caminho real do arquivo. As tarefas abertas `[ ]` são só os ajustes de
dados de teste A1 a A3, os testes de caracterização C1 a C13 (`plan.md` → Trabalho previsto; detalhe em
`research.md` → Decisões do usuário) e o fechamento. **Nenhuma tarefa aberta altera código de produção**: só arquivos
`*.spec.ts`.

**Testes**: Exigidos pela constituição (princípio I do `AGENTS.md`; `front/AGENTS.md`). Os C são testes de
caracterização de um comportamento existente e aceito: passam desde o início e não acompanham código novo. Por isso,
cada C segue a **regra de caracterização (RC)** abaixo, no lugar da fase vermelha do ciclo TDD. Escreva os testes no
modelo da skill `write-front-tests` (`HttpTestingController` com `verify()`, `resourceFromSnapshots` na tabela,
harnesses do Material nos filtros, `MatDialogRef`/`MAT_DIALOG_DATA` falsos no formulário).

**Regra de caracterização (RC)**, obrigatória em toda tarefa C:

1. Escreva o teste e rode o arquivo dele (`npx ng test --watch=false --include='<arquivo>'`). Ele deve passar.
2. **Se falhar sem nenhuma quebra proposital** (o comportamento real diverge da spec): pare o implement, não corrija
   código de produção, não enfraqueça o teste e leve o caso ao usuário (em geral, por `/speckit-bug-assess`).
3. Quebre de propósito, **sem commit**, o comportamento coberto no código de produção (a quebra sugerida está em cada
   tarefa), rode o arquivo de novo e confira que o teste novo falha pelo motivo esperado.
4. Desfaça a quebra (`git diff` sem nenhuma mudança fora de `*.spec.ts`), rode o arquivo de novo e confira que ele volta
   a passar.
5. Registre no relatório do implement: o teste, a quebra usada e o resultado (falhou → voltou a passar).

**Ajustes A**: trocam só dados de teste; nenhuma asserção de comportamento muda, e nenhum teste é apagado ou
enfraquecido. Depois de cada A, o arquivo afetado continua passando.

**Organização**: As tarefas estão agrupadas por história de usuário, na ordem de prioridade do `spec.md` (P1–P5).

## Formato: `[ID] [P?] [História] Descrição`

- **[P]**: Arquivos diferentes, sem dependência de uma tarefa incompleta
- **[História]**: A história de usuário a que a tarefa pertence (US1–US5)
- Os caminhos de arquivo são exatos, relativos a `front/`

---

## Fase 1: Preparação (infraestrutura compartilhada)

**Objetivo**: Rota, tipos e fixture da feature (já entregues).

- [X] T001 Rota lazy da página em `src/app/transactions/transactions.routes.ts` (`loadComponent` da
      `TransactionsPage`), carregada por `loadChildren` em `src/app/app.routes.ts` (`path: 'transactions'`), com
      `''` → `redirectTo: 'transactions'` (`pathMatch: 'full'`) (FR-001, FR-002).
- [X] T002 [P] Tipos `Transaction` e `TransactionInput` em `src/app/transactions/types/transaction.ts` e
      `TransactionFilters` (`description`, `type: '' | 'in' | 'out'`, `categoryId`, `month`) em
      `src/app/transactions/types/transaction-filters.ts`, espelhando o contrato (`data-model.md`).
- [X] T003 [P] Fixture `buildTransaction` em `src/app/transactions/testing/transaction-fixture.ts`.

---

## Fase 2: Fundação (pré-requisitos bloqueantes)

**Objetivo**: Service, peças compartilhadas e os ajustes de dados de teste, para que os testes C já nasçam com UUIDs.

**⚠️ CRÍTICO**: As tarefas C das histórias só começam depois de T007–T013 (A1 a A3), porque elas mexem nos mesmos
arquivos de spec.

- [X] T004 `TransactionsService` — `list(filters, sort)` devolvendo `{ url, params }` com os filtros vazios omitidos e
      `orderBy`/`direction`, mais `create` (`POST`), `update` (`PUT /{id}`) e `delete` (`DELETE /{id}`) — e o seu spec em
      `src/app/transactions/services/transactions.service.ts` e `transactions.service.spec.ts` (FR-031).
- [X] T005 [P] Peças de `src/app/shared/` usadas pela feature, com os seus specs: `date-utils/` (dia do calendário ↔
      meia-noite UTC; `fromMonthKey`), `month-field/`, `sort-menu/`, `page-header/` e `mat-chip-color/`.
- [X] T006 [P] Uso de `src/app/categories/services/categories.service.ts` (`list()`), de
      `src/app/categories/types/category.ts` (`Category`) e, nos specs, de
      `src/app/categories/testing/category-fixture.ts` (`buildCategory`), sob a exceção à F1 aprovada pelo usuário em
      2026-10-05 (`plan.md` → Acompanhamento de complexidade).
- [ ] T007 [P] A1: em `src/app/transactions/services/transactions.service.spec.ts`, trocar os ids de categoria e de
      transação que não são UUID (`'mercado'`, `'salario'`, `'removida'`, `'transporte'`, `'t1'`…`'t10'`) por UUIDs
      (constantes no topo do arquivo), sem mudar nenhuma asserção; o arquivo continua passando.
- [ ] T008 [P] A1: o mesmo ajuste de T007 em
      `src/app/transactions/components/transactions-table/transactions-table.spec.ts` (o `buildCategory` não muda; os
      UUIDs entram como overrides).
- [ ] T009 [P] A1: o mesmo ajuste de T007 em
      `src/app/transactions/components/transactions-filters/transactions-filters.spec.ts`.
- [ ] T010 [P] A1: o mesmo ajuste de T007 em
      `src/app/transactions/components/transaction-form/transaction-form.spec.ts`.
- [ ] T011 [P] A1: o mesmo ajuste de T007 em `src/app/transactions/pages/transactions-page/transactions-page.spec.ts`.
- [ ] T012 A2: em `src/app/transactions/pages/transactions-page/transactions-page.spec.ts` (teste do `categoryId` no
      endereço, hoje em `:232-245`, com `categoryId: 'salario'`), usar o UUID da categoria no `queryParamMap` e na
      consulta esperada, de modo que a resposta `200` seja a que a API real daria; as asserções continuam as mesmas
      (depende de T011).
- [ ] T013 A3: em `src/app/transactions/pages/transactions-page/transactions-page.spec.ts` (teste da mensagem padrão do
      salvamento, hoje em `:357-361`, com `500` sem corpo), responder com um erro de rede (`req.error(new
      ProgressEvent('error'))`, status 0) no lugar do `500`; a asserção de "Não foi possível salvar a transação."
      continua a mesma (depende de T012).

**Checkpoint**: Specs da feature com UUIDs e passando (`npx ng test --watch=false
--include='src/app/transactions/**/*.spec.ts'`); os testes C podem começar.

---

## Fase 3: História de usuário 1 - Revisar as transações registradas (Prioridade: P1) 🎯 MVP

**Objetivo**: A página inicial lista as transações das mais recentes para as mais antigas, com data, descrição,
etiqueta da categoria e valor, agrupadas por dia, com os estados de carga, vazio e erro.

**Teste independente**: Com transações em categorias diferentes, abrir a aplicação e conferir as linhas, a ordem, o
agrupamento por data, as cores e os estados (`quickstart.md` → passo 1).

### Testes da história 1 (caracterização, regra RC) ⚠️

- [ ] T014 [P] [US1] C3 (FR-002): criar `src/app/app.routes.spec.ts` que configura `provideRouter(routes)` com as rotas
      reais de `src/app/app.routes.ts`, navega para `''` (por exemplo, com `RouterTestingHarness` ou
      `Router.navigateByUrl('')`) e confere que a URL final é `/transactions`. Se a carga lazy da página exigir
      providers (HTTP), use `provideHttpClient()` e `provideHttpClientTesting()` e descarte as requisições sem
      afirmar sobre elas. Quebra sugerida (RC): trocar o `redirectTo` para `'categories'`.
- [ ] T015 [US1] C4, parte da tabela (FR-008): em
      `src/app/transactions/components/transactions-table/transactions-table.spec.ts`, com `resourceFromSnapshots`:
      (a) numa recarga (`reloading`) com linhas, as linhas continuam à vista e o `mat-progress-spinner` não aparece;
      (b) numa recarga da lista vazia e (c) numa recarga depois de erro, o spinner ocupa o lugar da mensagem ("Nenhuma
      transação encontrada." / "Não foi possível carregar as transações." não aparecem). Quebras sugeridas (RC): para (a), em
      `rows()` de `transactions-table.ts`, devolver `[]` quando `transactions.isLoading()`; para (b) e (c), trocar a
      condição do primeiro `@if` de `transactions-table.html` (`transactions().isLoading() ||
      categories().isLoading()`) por `false`, para que a mensagem apareça durante a recarga.
- [ ] T016 [US1] C4, parte da página (FR-008): em
      `src/app/transactions/pages/transactions-page/transactions-page.spec.ts`, com a lista carregada, escolher
      "Recarregar" e conferir, antes de responder às novas requisições de transações e de categorias, que as linhas
      continuam à vista; depois responder e conferir a lista atualizada. Quebra sugerida (RC): a mesma de T015.
- [ ] T017 [US1] C5 (FR-011): em `src/app/transactions/pages/transactions-page/transactions-page.spec.ts`, conferir
      que `app-page-header` e o `mat-card.filters` (com os filtros à vista) não estão dentro do `mat-card.table`, e,
      pelo `getComputedStyle`, que o `mat-card.table` tem `overflow: auto` e `flex: 1` (ou `flex-grow: 1`, conforme o
      jsdom resolver o atalho) e o host tem `max-height: 100%`. A rolagem real fica no passo 1 do `quickstart.md`. Se
      os estilos do componente não chegarem ao jsdom, pare e leve a dúvida ao usuário, sem enfraquecer a asserção.
      Quebra sugerida (RC): remover `overflow: auto` de `transactions-page.scss`.
- [ ] T018 [US1] C13, parte da tabela (SC-008, FR-032): em
      `src/app/transactions/components/transactions-table/transactions-table.spec.ts`, uma transação com descrição
      `<b>teste</b>` e uma categoria com nome `<b>teste</b>`: a célula "Descrição" e a etiqueta da categoria mostram o
      texto literal (`textContent` contém `<b>teste</b>`) e não há nenhum elemento `b` na célula nem no `mat-chip`.
      Quebra sugerida (RC): trocar a interpolação da descrição em `transactions-table.html` por
      `[innerHTML]="transaction.description"` (depende de T015, mesmo arquivo).

### Implementação da história 1 (entregue)

- [X] T019 [US1] `TransactionsTable` — colunas "Data" (`date: 'shortDate' : 'UTC'`), "Descrição", "Categoria"
      (`mat-chip` com `appMatChipColor`), "Valor" (`currency: 'BRL'`, classe `negative` nas saídas) e ações;
      `rows()` que espera as duas listas; `sameDate()` para o agrupamento por dia; linha sem dados com spinner, erro
      ("Não foi possível carregar as transações.") e vazio ("Nenhuma transação encontrada.") — e o seu spec em
      `src/app/transactions/components/transactions-table/` (FR-003 a FR-009; FR-030: "Editar" e "Excluir" com nome acessível e dica).
- [X] T020 [US1] `TransactionsPage` — signals `filters`, `filtersVisible` e `sort` (padrão `date`/`desc`),
      `httpResource` de transações dependente de `filters` e `sort`, `httpResource` de categorias sem signals,
      `reload()` das duas listas, card da tabela com `flex: 1` e `overflow: auto` e host com `max-height: 100%` — e o
      seu spec em `src/app/transactions/pages/transactions-page/` (FR-001, FR-008 a FR-011, SC-007).
- [X] T021 [P] [US1] Item "Transações" no menu lateral, marcado na página atual, em `src/app/app.html` e
      `src/app/app.ts`, com o teste em `src/app/app.spec.ts` (FR-001).

**Checkpoint**: História 1 coberta pelos testes C3, C4, C5 e C13 (tabela).

---

## Fase 4: História de usuário 2 - Registrar e corrigir transações (Prioridade: P2)

**Objetivo**: "Nova transação" e "Editar" abrem o diálogo, que valida os campos e fecha com o `TransactionInput`; a
página salva e mostra o resultado.

**Teste independente**: Criar e editar uma transação e conferir a linha, o sinal, a cor e as mensagens
(`quickstart.md` → passos 2 e 3).

### Testes da história 2 (caracterização, regra RC) ⚠️

Todas em `src/app/transactions/components/transaction-form/transaction-form.spec.ts`, em sequência (mesmo arquivo).
Nenhuma digita texto no campo "Data", usa o valor zero ou valores com mais de 13 algarismos inteiros: esses são
desvios conhecidos, cujos testes nascem nos bug-fix.

- [ ] T022 [US2] C6 (FR-014): esvaziar o input do campo "Data" (input vazio → `null` → `required`), escolher "Salvar"
      e conferir "Informe uma data válida" e que `dialogRef.close` não foi chamado. Quebra sugerida (RC): remover
      `Validators.required` do controle `date` em `transaction-form.ts`.
- [ ] T023 [US2] C7 (FR-014): formulário isolado com `MAT_DIALOG_DATA` `{ categories: [] }`; "Salvar" mostra
      "Escolha uma categoria" e o diálogo continua aberto. Afirma só a validação do formulário, não a abertura do
      diálogo pela página (desvio da FR-019). Quebra sugerida (RC): remover `Validators.required` do controle
      `categoryId`.
- [ ] T024 [US2] C8 (FR-014, caso-limite): `12.50` é aceito e fecha com `value: -12.5` (saída); `8.600,00` e `-12`
      mostram "Informe um valor válido" e não fecham. Quebra sugerida (RC): trocar o padrão
      `/^\d+([.,]\d{1,2})?$/` por `/^-?[\d.,]+$/` em `transaction-form.ts`.
- [ ] T025 [US2] C9 (FR-014): o input "Descrição" tem `maxlength` 255 e o hint mostra `N/255` com a quantidade de
      caracteres digitados (por exemplo, `4/255` com "Pão " e `255/255` com 255 caracteres). Quebra sugerida (RC):
      mudar `descriptionMaxLength` para 256.
- [ ] T026 [US2] C13, parte do formulário (SC-008, FR-032): com uma categoria de nome `<b>teste</b>`, as opções do
      campo "Categoria" e o valor escolhido mostram o texto literal, sem elemento `b`. Quebra sugerida (RC): renderizar
      o nome da opção com `[innerHTML]` em `transaction-form.html`.

### Implementação da história 2 (entregue)

- [X] T027 [US2] `TransactionForm` — Reactive Forms com `type` (padrão "Saída" ou o sinal da transação editada),
      `value` (`Validators.required`, `Validators.pattern(/^\d+([.,]\d{1,2})?$/)`), `date` (`Validators.required`,
      `provideNativeDateAdapter()`), `categoryId` (`Validators.required`, padrão a primeira categoria) e `description`
      (`maxlength` 255 com contador `N/255`); `submit()` que fecha com o valor negativo para saída, a descrição
      aparada ou `null` e a data por `DateUtils.toApiDate` — e o seu spec em
      `src/app/transactions/components/transaction-form/` (FR-012 a FR-015).
- [X] T028 [US2] `openForm()` e `save()` em `src/app/transactions/pages/transactions-page/transactions-page.ts`:
      bloqueio com "Não foi possível carregar as categorias. Tente novamente." quando as categorias estão em erro,
      `create`/`update`, "Transação criada."/"Transação salva." por 3 s com recarga da lista, e erro com o `detail` da
      API ou "Não foi possível salvar a transação." por 5 s com "Fechar" (FR-012; FR-016 (só a parte entregue: fechar sem `input` não salva nem mostra
      mensagem); FR-017 a FR-019).

**Checkpoint**: História 2 coberta pelos testes C6 a C9 e C13 (formulário).

---

## Fase 5: História de usuário 3 - Excluir uma transação, com a opção de desfazer (Prioridade: P3)

**Objetivo**: "Excluir" remove na hora, mostra "Transação excluída." com "Desfazer", e "Desfazer" recria a transação.

**Teste independente**: Excluir, desfazer e conferir os dados e as mensagens (`quickstart.md` → passo 4).

### Testes da história 3 (caracterização, regra RC) ⚠️

Ambas em `src/app/transactions/pages/transactions-page/transactions-page.spec.ts`, em sequência.

- [ ] T029 [US3] C10 (FR-022): a exclusão falha com erro de rede (status 0, sem `detail`); aparece "Não foi possível
      excluir a transação." por 5 s com "Fechar", e nenhuma nova requisição de transações é feita (o `verify()` do
      `afterEach` confirma). Quebra sugerida (RC): trocar a mensagem padrão em `delete()` de `transactions-page.ts`.
- [ ] T030 [US3] C11 (FR-021): depois da exclusão bem-sucedida, escolher "Desfazer"; (a) o `POST` recusado com
      `detail` mostra o `detail` da API; (b) o `POST` recusado com erro de rede mostra "Não foi possível salvar a
      transação."; nos dois casos, nenhuma recarga além da que segue a exclusão. Quebra sugerida (RC): chamar
      `this.transactions.reload()` também no `error` de `save()`.

### Implementação da história 3 (entregue)

- [X] T031 [US3] Botão "Excluir" (`delete.emit`) em
      `src/app/transactions/components/transactions-table/transactions-table.html` e `delete()` em
      `src/app/transactions/pages/transactions-page/transactions-page.ts`: exclusão sem confirmação, recarga,
      "Transação excluída." com "Desfazer" por 5 s, restauração por `create` com os mesmos dados e "Transação
      restaurada.", e erro com o `detail` ou "Não foi possível excluir a transação." (FR-020 a FR-022).

**Checkpoint**: História 3 coberta pelos testes C10 e C11.

---

## Fase 6: História de usuário 4 - Filtrar e ordenar a lista (Prioridade: P4)

**Objetivo**: Filtros Descrição, Tipo, Categoria e Mês, cada um com "Limpar", e o menu de ordenação, aplicados na API.

**Teste independente**: Exibir os filtros, aplicar cada filtro e cada ordenação e conferir a lista e as consultas
(`quickstart.md` → passos 5 e 8).

### Testes da história 4 (caracterização, regra RC) ⚠️

- [ ] T032 [US4] C13, parte dos filtros (SC-008, FR-032): em
      `src/app/transactions/components/transactions-filters/transactions-filters.spec.ts`, com uma categoria de nome
      `<b>teste</b>`, as opções do filtro "Categoria" e o valor escolhido mostram o texto literal, sem elemento `b`.
      Quebra sugerida (RC): renderizar o nome da opção com `[innerHTML]` em `transactions-filters.html`.

### Implementação da história 4 (entregue)

- [X] T033 [US4] `TransactionsFilters` — Signal Forms sobre o `model` `filters`, `debounce` de 300 ms na descrição,
      "Tipo" (Todos/Entradas/Saídas), "Categoria" (Todas + categorias), `app-month-field` e os botões "Limpar …" — e o
      seu spec em `src/app/transactions/components/transactions-filters/` (FR-024 a FR-026; FR-030: "Limpar …" com nome acessível, sem dica).
- [X] T034 [US4] Botão "Exibir filtros"/"Ocultar filtros" (filtros ocultos por padrão) e `app-sort-menu` com as oito
      opções ("Mais recentes primeiro" como padrão) em `src/app/transactions/pages/transactions-page/transactions-page.html`
      e `transactions-page.ts` (FR-023, FR-027; FR-030: "Recarregar", "Exibir/Ocultar filtros" e "Ordenar: …" com nome
      acessível e dica).
- [X] T035 [US4] Filtros e ordenação levados à API por `TransactionsService.list` em
      `src/app/transactions/services/transactions.service.ts`, com uma consulta de transações por mudança e as
      categorias sem nova consulta (FR-031, SC-007; a consulta repetida é desvio conhecido que bloqueia a entrega).

**Checkpoint**: História 4 coberta pelo teste C13 (filtros).

---

## Fase 7: História de usuário 5 - Chegar à lista já filtrada a partir de outra página (Prioridade: P5)

**Objetivo**: `?categoryId=` e `?month=YYYY-MM` no endereço abrem a página com os filtros à vista e aplicados.

**Teste independente**: Abrir os endereços com categoria, mês e os dois juntos (`quickstart.md` → passo 6).

### Testes da história 5 (caracterização, regra RC) ⚠️

Todas em `src/app/transactions/pages/transactions-page/transactions-page.spec.ts`, em sequência, com o
`ActivatedRoute` falso.

- [ ] T036 [US5] C1 (caso-limite, FR-028): `?categoryId=nao-e-uuid`; a consulta leva `categoryId=nao-e-uuid` e a API
      responde `400` com `ValidationProblemDetails` (sem `detail`); a tabela mostra "Não foi possível carregar as
      transações.", os filtros ficam à vista e o filtro "Categoria" não mostra nenhum nome. Quebra sugerida (RC):
      iniciar `filtersVisible` com `false` em `transactions-page.ts`.
- [ ] T037 [US5] C2 (caso-limite, FR-028): `?categoryId=<UUID de categoria inexistente>`; a API responde `200 []`; a
      tabela mostra "Nenhuma transação encontrada.". Quebra sugerida (RC): ignorar o `categoryId` do endereço
      (iniciar o filtro com `''`) e responder a consulta sem ele, para ver o teste falhar na consulta esperada.
- [ ] T038 [US5] C12 (FR-028): `?categoryId=<UUID>&month=2026-09` juntos; a consulta de transações leva `categoryId` e
      `dateRef=2026-09-01`, e os filtros abrem à vista com a categoria e o mês preenchidos. Quebra sugerida (RC): ler
      só um dos parâmetros (por exemplo, iniciar `month` com `null`).

### Implementação da história 5 (entregue)

- [X] T039 [US5] Leitura única de `categoryId` e `month` do `snapshot.queryParamMap` em
      `src/app/transactions/pages/transactions-page/transactions-page.ts`, com o mês por `DateUtils.fromMonthKey`
      (formato inválido ignorado) e os filtros à vista quando algum vem do endereço (FR-028, FR-029).

**Checkpoint**: História 5 coberta pelos testes C1, C2 e C12.

---

## Fase 8: Fechamento

**Objetivo**: Confirmar que o trabalho desta feature não quebrou nada e não tocou código de produção.

- [ ] T040 A partir de `front/`, rodar `npx ng test --watch=false` (suíte completa: todos os testes passam, incluindo
      C1–C13 e os ajustes A1–A3), `npm run lint` (limpo) e `npm run build` (sem erro nem aviso de budget); conferir com
      `git diff --name-only -- front/src front/public` que só mudaram arquivos `*.spec.ts` (de `src/app/transactions/`
      e `src/app/app.routes.spec.ts`; os artefatos em `front/specs/003-gestao-de-transacoes/` podem mudar, com as
      marcações das tarefas e o registro das verificações);
      fazer a conferência manual da rolagem real (C5) pelo passo 1 do `quickstart.md` (`npm start`, janela de
      1366×768, muitas transações e filtros à vista: só a lista rola, e o cabeçalho e os filtros continuam à vista);
      registrar no relatório do implement os totais da suíte, o "Initial total", a conferência RC de cada C e o
      resultado da conferência manual da rolagem (depende de T007–T038).

---

## Dependência de entrega (fora das tarefas desta feature)

Pela Governança do `AGENTS.md`, a feature **só é dada como entregue no fim do `/speckit-converge`** depois que os cinco
desvios bloqueantes abaixo forem corrigidos pelo fluxo de bugs do front. Nenhum deles vira tarefa deste `tasks.md`, e
nenhuma tarefa acima corrige código de produção.

| Desvio bloqueante | Requisito / princípio | Fluxo |
| --- | --- | --- |
| Consulta repetida ao escolher de novo a mesma ordenação, o mesmo mês ou o mesmo texto | FR-031, SC-007 / III | `/speckit-bug-assess` → `/speckit-bug-fix` → `/speckit-bug-test` em `front/bugs/<slug>/` |
| Clique repetido em "Excluir", sem retorno visual durante a exclusão e a restauração | FR-020 / III | idem |
| Calendários anunciados em inglês | FR-030 / Idioma | idem |
| Data digitada lida como mês/dia/ano (ou ISO em UTC) | FR-014 / II | idem |
| Valores com 14 ou mais algarismos inteiros alterados pelo `parseFloat` | FR-014 / II | idem |

Um bug-fix está **concluído** quando: o `front/bugs/<slug>/test.md` registra `verified`; o teste que reproduz o bug e
a suíte completa do front passam; e o `/speckit-converge` desta feature reavalia contra o código os FR/SC afetados
(cenário 8 da História 4, partes da SC-007 sobre repetição, FR-020, FR-030 e FR-014). Os testes e ajustes desta lista
(T007–T040) podem ser implementados antes desses bug-fix. Os demais desvios conhecidos do `plan.md` (zero, FR-016,
FR-017, FR-019, FR-024, link da 002, `dateRef` e validação do valor no back) não bloqueiam a entrega.

**Desvios conhecidos que não bloqueiam a entrega** (`plan.md` → Desvios conhecidos e fluxo de destino). Ficam fora
deste arquivo: o `/speckit-converge` só os registra como desvios conhecidos, sem anexar tarefas.

| Desvio | Requisito | Destino |
| --- | --- | --- |
| Valor zero aceito pelo formulário, que fecha o diálogo antes de a API o recusar com `400` | FR-014 | Fluxo de bugs do front |
| API sem validação de faixa e de escala do valor (só recusa o zero, arredonda mais de duas casas sem aviso e responde `500` a partir de 10^16) | caso-limite do valor; Premissas | Fluxo de bugs do back (validar faixa e escala com `400`) |
| Diálogo que fecha antes da resposta da API e perde o que foi digitado | FR-016 | Fluxo de bugs do front |
| Sem aviso quando a transação salva fica fora dos filtros em uso | FR-017 | Fluxo de bugs do front |
| "Nova transação" sem categorias (sem mensagem nem "Ver categorias"; botão habilitado antes de a primeira carga terminar) | FR-019 | Fluxo de bugs do front |
| Dia escolhido no calendário do "Mês" que não muda o filtro (não reproduzido) | FR-024 | Fluxo de bugs do front, começando por um teste que reproduz |
| Link da quantidade de transações sem o mês | FR-004 da 002 (desvio); FR-028 desta (recebe, sem mudança) | Fluxo de bugs do front, registrado na 002 |
| `dateRef` declarado `format: date-time` no contrato do back, embora a API aceite a data sem hora | — (documentação do contrato) | Fluxo de bugs do back, só na documentação do contrato |

---

## Dependências e ordem de execução

### Dependências entre fases

- **Preparação (Fase 1)** e **Fundação (T004–T006)**: entregues.
- **Ajustes A (T007–T013)**: primeiro trabalho aberto; bloqueiam os C, que mexem nos mesmos specs e já nascem com UUIDs.
- **Histórias (Fases 3–7)**: dependem dos ajustes A; podem seguir na ordem de prioridade (P1 → P5).
- **Fechamento (T040)**: depende de todas as tarefas abertas.

### Dependências entre histórias

- As histórias são independentes entre si, mas **US1 (T016, T017), US3 (T029, T030) e US5 (T036–T038) escrevem no
  mesmo arquivo** (`transactions-page.spec.ts`): essas tarefas são sequenciais entre si, assim como T011–T013.
- US1 (T015, T018) e T008 escrevem em `transactions-table.spec.ts`; US2 (T022–T026) e T010 em
  `transaction-form.spec.ts`; US4 (T032) e T009 em `transactions-filters.spec.ts`.

### Dentro de cada história

- Testes antes da implementação (aqui, a implementação já está entregue e marcada `[X]`).
- Cada C segue a regra RC: passa → quebra proposital falha → desfaz → passa → registro.
- Um C que falha sem quebra para o implement e vai ao usuário.

### Oportunidades de paralelismo

- T007, T008, T009 e T010 (A1 em quatro arquivos diferentes), em paralelo com T011.
- Depois dos ajustes A, os grupos por arquivo podem correr em paralelo: `app.routes.spec.ts` (T014);
  `transactions-table.spec.ts` (T015, T018); `transaction-form.spec.ts` (T022–T026);
  `transactions-filters.spec.ts` (T032); `transactions-page.spec.ts` (T016, T017, T029, T030, T036–T038).
- As quebras propositais da RC mexem em código de produção sem commit: em paralelo, cada agente usa a sua cópia de
  trabalho (worktree), para que uma quebra não derrube o teste de outro grupo.

---

## Exemplo de paralelismo

```bash
# Ajustes A1 em arquivos diferentes:
Task: "A1 em src/app/transactions/services/transactions.service.spec.ts"
Task: "A1 em src/app/transactions/components/transactions-table/transactions-table.spec.ts"
Task: "A1 em src/app/transactions/components/transactions-filters/transactions-filters.spec.ts"
Task: "A1 em src/app/transactions/components/transaction-form/transaction-form.spec.ts"

# Depois dos ajustes, um grupo por arquivo de spec:
Task: "C3 em src/app/app.routes.spec.ts"
Task: "C4 (tabela) e C13 (tabela) em transactions-table.spec.ts"
Task: "C6–C9 e C13 (formulário) em transaction-form.spec.ts"
Task: "C13 (filtros) em transactions-filters.spec.ts"
```

---

## Estratégia de implementação

### MVP primeiro (história 1)

1. Ajustes A1–A3 (T007–T013) e checkpoint da Fundação.
2. Testes da história 1 (T014–T018), cada um com a regra RC.
3. **Parar e validar**: specs da feature e suíte completa passando.

### Entrega incremental

1. Ajustes A → specs com UUIDs.
2. História 1 → C3, C4, C5, C13 (tabela).
3. História 2 → C6–C9, C13 (formulário).
4. História 3 → C10, C11.
5. História 4 → C13 (filtros).
6. História 5 → C1, C2, C12.
7. Fechamento (T040); a entrega da feature ainda espera os cinco bug-fix bloqueantes e o `/speckit-converge`.

---

## Observações

- [P] = arquivos diferentes, sem dependência de tarefa incompleta.
- [História] liga a tarefa à história de usuário, para rastreabilidade.
- Nenhuma tarefa aberta muda código de produção; as quebras da RC são desfeitas antes de seguir e nunca vão para
  commit.
- Nenhum teste existente é apagado, ignorado ou enfraquecido; os ajustes A mudam só dados.
- Testes dos desvios conhecidos (data digitada, zero, valores com mais de 13 algarismos, clique repetido em
  "Excluir" etc.) não entram aqui: nascem no `/speckit-bug-fix` de cada correção.
- O bug-fix da FR-019 não deve inverter nem apagar o C7 (T023); se a correção da FR-014 mudar a validação do C6, C7 ou
  C8, o ajuste é feito no bug-fix, com aprovação do usuário.
