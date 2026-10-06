---

description: "Lista de tarefas da feature Gestão de categorias (retroativa)"
---

# Tarefas: Gestão de categorias

**Entrada**: Artefatos de design em `front/specs/002-gestao-de-categorias/`

**Pré-requisitos**: plan.md, spec.md, research.md, data-model.md, contracts/categories-ui.md, quickstart.md

**Natureza retroativa**: o código em `front/src/app/categories/` (e as peças de `front/src/app/shared/` que a página
usa) já implementa a feature. As tarefas que correspondem ao que já existe estão marcadas `[X]`, com o arquivo real. As
únicas tarefas abertas são os testes de caracterização TC-01 a TC-10 do plan ([Trabalho desta
feature](./plan.md#trabalho-desta-feature)) e o fechamento: **só mudam arquivos `*.spec.ts`; nenhum código de produção
muda**.

**Testes**: pedidos pelo plan (decisões D1 a D16 do `research.md`). Em cada história, as tarefas de teste vêm antes das
de implementação, que aqui já estão concluídas.

**Organização**: tarefas agrupadas por história de usuário (US1 a US6 da spec). Caminhos relativos à raiz do repositório.

## Formato: `[ID] [P?] [Story] Descrição`

- **[P]**: pode rodar em paralelo (arquivo diferente, sem dependência de tarefa aberta)
- **[Story]**: a história de usuário da tarefa (US1 a US6)

## Regras das tarefas de caracterização (plan, "Trabalho desta feature")

Valem para toda tarefa de teste aberta (TC-01 a TC-10) e para a sua tarefa de verificação:

1. **Escrever e rodar.** O teste novo descreve um comportamento que já existe e foi aceito na spec, então deve passar
   na primeira execução (`npx ng test --watch=false --include <arquivo>` em `front/`).
2. **TC que falha para o implement.** Se o teste falhar na primeira execução, o comportamento atual diverge da spec: o
   `/speckit-implement` **para**, **não** corrige código de produção e leva o caso ao usuário (em geral, para um
   `/speckit-bug-assess`). O teste não é enfraquecido para passar.
3. **Ver falhar uma vez (tarefa de verificação).** Depois de passar, quebra-se de propósito o comportamento coberto no
   código de produção, **sem commit**, roda-se o spec e confere-se que o teste novo falha pelo motivo esperado; depois a
   quebra é desfeita (`git diff` do arquivo de produção vazio) e o teste volta a passar. As quebras sugeridas em cada
   tarefa são exemplos; vale qualquer quebra mínima do comportamento coberto.
4. **Registro.** Ao marcar `[X]` a tarefa de verificação, anote nela (ou no relatório do implement) a quebra usada e o
   teste que falhou.

---

## Fase 1: Setup (infraestrutura compartilhada)

**Objetivo**: rota, menu e assets da página.

- [X] T001 Rota `/categories` carregada sob demanda (`loadChildren`) em `front/src/app/app.routes.ts`, com o `loadComponent` da página em `front/src/app/categories/categories.routes.ts`
- [X] T002 Item "Categorias" (terceiro do menu, ícone `sell`) em `front/src/app/app.ts` e `front/src/app/app.html`, coberto por `front/src/app/app.spec.ts`
- [X] T003 [P] Paleta padrão de 11 cores em `front/public/data/default-colors.json`
- [X] T004 [P] `LOCALE_ID` pt-BR, aparência dos campos e atraso das dicas em `front/src/app/app.config.ts`

---

## Fase 2: Fundacional (pré-requisitos bloqueantes)

**Objetivo**: tipos, service, fixture e peças de `shared/` usadas por todas as histórias; e o ajuste dos dados de teste
da página (TC-08), que muda o fixture `mercado` de `categories-page.spec.ts` usado pelos TC-02 e TC-03.

- [X] T005 [P] Tipos `Category` (oito campos de `CategoryOutput`) e `CategoryInput` em `front/src/app/categories/types/category.ts`, e `CategoryFilters` (só do front) em `front/src/app/categories/types/category-filters.ts`
- [X] T006 [P] `CategoriesService` (`list` como requisição para `httpResource`, `create`, `update`, `delete`; URL e parâmetros `name`, `withTransaction`, `dateRef`, `sort`) em `front/src/app/categories/services/categories.service.ts`, com `categories.service.spec.ts`
- [X] T007 [P] Fixture `buildCategory` em `front/src/app/categories/testing/category-fixture.ts`
- [X] T008 [P] Peças compartilhadas com os seus specs: `front/src/app/shared/page-header/`, `front/src/app/shared/sort-menu/`, `front/src/app/shared/month-field/`, `front/src/app/shared/mat-chip-color/` (contraste ≥ 7:1) e `front/src/app/shared/date-utils/` (`toDateKey`)
- [ ] T009 TC-08 (FR-014, Casos-limite): ajustar os dados de teste em `front/src/app/categories/pages/categories-page/categories-page.spec.ts`, sem mudar o comportamento conferido (só os textos das respostas da API e, no item (b), o texto esperado na asserção): (a) no teste "should show the API error detail and not reload when saving fails", trocar o `409` ("Já existe uma categoria com esse nome.") por um nome só com espaços (`'   '`) recusado com `400` e o `detail` "O nome da categoria não pode ser vazio."; (b) no teste "should show why the API refuses to delete a category with transactions", usar a mensagem real "Não é possível excluir uma categoria que possui transações associadas." na resposta e na asserção; (c) no mesmo teste, acrescentar `canDelete: false` ao fixture `mercado` (que já tem 14 transações e `balance: -1842.55`) e excluir pelo `rowButton(1, 'Excluir')` o `educacao` (0 transações, saldo 0, `canDelete: true`), recusada pela API (lista desatualizada); (d) no teste "should show a fallback message when the error has no detail", trocar o 500 sem corpo por um erro de rede (status 0). Rodar o spec: tudo passa (regra 2 se não passar)
- [ ] T010 Verificar o TC-08 (regra 3): em `front/src/app/categories/pages/categories-page/categories-page.ts`, quebrar de propósito, por exemplo, `showError` usando sempre o `fallback` (os testes de (a) e (b/c) falham) e, em seguida, trocar o texto "Não foi possível salvar a categoria." (o teste de (d) falha); desfazer as quebras e confirmar o spec verde (depende de T009)

**Checkpoint**: base pronta; as histórias podem seguir.

---

## Fase 3: História de usuário 1 - Ver as categorias e o uso de cada uma (Prioridade: P1) 🎯 MVP

**Objetivo**: lista com nome no chip da cor, quantidade de transações e saldo em BRL; estados de carregamento, vazio e
erro; "Recarregar"; menu e título.

**Teste independente**: com categorias com e sem transações e saldos negativo, zero e positivo, abrir `/categories` pelo
menu e conferir as três colunas, o item "Categorias" marcado, o spinner só no primeiro carregamento e os estados de
vazio e erro (quickstart.md).

### Testes da história 1 (caracterização)

- [ ] T011 [P] [US1] TC-06 (FR-001): em `front/src/app/app.spec.ts`, navegar para `/categories` e conferir que o item "Categorias" é o marcado como página atual (`aria-current="page"`) e nenhum outro
- [ ] T012 [US1] Verificar o TC-06 (regra 3): em `front/src/app/app.html`, quebrar a marcação do item ativo (por exemplo, remover `[activated]="active.isActive"`), ver o T011 falhar, desfazer e confirmar verde (depende de T011)
- [ ] T013 [P] [US1] TC-10 (FR-021, Casos-limite): em `front/src/app/categories/components/categories-table/categories-table.spec.ts`, uma categoria com o nome `<b>teste</b>` aparece literalmente no chip (o texto do chip é `<b>teste</b>` e não há elemento `b` dentro dele). Só o template do projeto entra no teste; o snack bar fica como garantia documentada do Material, sem teste
- [ ] T014 [US1] Verificar o TC-10 (regra 3): em `front/src/app/categories/components/categories-table/categories-table.html`, trocar temporariamente a interpolação do chip por `[innerHTML]="category.name"`, ver o T013 falhar, desfazer e confirmar verde (depende de T013)
- [ ] T015 [US1] TC-05 (FR-006, Casos-limite): em `front/src/app/categories/pages/categories-page/categories-page.spec.ts`, com a lista carregada, "Recarregar" (e a recarga depois de salvar) mantém as linhas visíveis, sem spinner, até a nova lista chegar; depois da resposta, as linhas novas aparecem (depende de T009, mesmo arquivo)
- [ ] T016 [US1] Verificar o TC-05 (regra 3): em `front/src/app/categories/components/categories-table/categories-table.ts`, fazer `rows()` devolver `[]` enquanto `isLoading()`, ver o T015 falhar, desfazer e confirmar verde (depende de T015)
- [ ] T017 [US1] TC-09, parte da página (FR-001, FR-020): em `front/src/app/categories/pages/categories-page/categories-page.spec.ts`, o título visível é "Categorias" e a dica (tooltip) do botão "Recarregar" é "Recarregar" (depende de T015, mesmo arquivo)
- [ ] T018 [US1] Verificar o TC-09 da página/US1 (regra 3): em `front/src/app/categories/pages/categories-page/categories-page.html`, trocar o `text` do `app-page-header` e o `matTooltip` de "Recarregar", ver as asserções do T017 falharem, desfazer e confirmar verde (depende de T017)

### Implementação da história 1 (já existente)

- [X] T019 [US1] Estado da página (`filters`, `filtersVisible`, `sort`) e `httpResource` da lista em `front/src/app/categories/pages/categories-page/categories-page.ts`, com cabeçalho ("Categorias", "Recarregar") em `categories-page.html` e `categories-page.scss`
- [X] T020 [US1] Tabela com chip na cor (`appMatChipColor`, nome por interpolação), "Transações", "Saldo" em BRL (negativo destacado) e linha de spinner/erro/vazio em `front/src/app/categories/components/categories-table/categories-table.ts`, `categories-table.html` e `categories-table.scss`

**Checkpoint**: US1 caracterizada; specs de `app.spec.ts`, `categories-table.spec.ts` e `categories-page.spec.ts` verdes.

---

## Fase 4: História de usuário 2 - Criar e editar categorias (Prioridade: P2)

**Objetivo**: "Nova categoria" e "Editar" abrem o diálogo com "Nome" (obrigatório, até 100 caracteres, contador `N/100`),
paleta padrão e "Cor personalizada"; mensagens de sucesso e de erro.

**Teste independente**: criar por "Nova categoria" e conferir a mensagem e a linha nova; editar nome e cor; conferir o
limite de 100 e uma recusa da API (quickstart.md).

### Testes da história 2 (caracterização)

- [ ] T021 [P] [US2] TC-01 (FR-008, Casos-limite): em `front/src/app/categories/components/category-form/category-form.spec.ts`, o "Nome" tem `maxlength="100"`, e o contador mostra "0/100" ao abrir uma categoria nova, "N/100" ao digitar e "100/100" com 100 caracteres (no jsdom o `maxlength` não corta valor atribuído por código: conferir o atributo e o contador)
- [ ] T022 [US2] Verificar o TC-01 (regra 3): em `front/src/app/categories/components/category-form/category-form.ts`, usar `nameMaxLength = 99`, ver o T021 falhar, desfazer e confirmar verde (depende de T021)
- [ ] T023 [US2] TC-07 (Casos-limite, cor fora da paleta): em `front/src/app/categories/components/category-form/category-form.spec.ts`, editar uma categoria com cor fora da paleta não marca nenhuma cor (`aria-pressed="false"` em todas) e mostra a cor dela na "Cor personalizada" (depende de T021, mesmo arquivo)
- [ ] T024 [US2] Verificar o TC-07 (regra 3): em `front/src/app/categories/components/category-form/category-form.ts`, tirar a condição `!this.category` do `effect` (a primeira cor da paleta sobrescreve a da categoria), ver o T023 falhar, desfazer e confirmar verde (depende de T023)
- [ ] T025 [US2] TC-09, parte do diálogo (FR-020): em `front/src/app/categories/components/category-form/category-form.spec.ts`, as cores ficam num grupo `role="group"` com `aria-label="Cor"`, e o seletor livre tem o nome acessível "Cor personalizada" (depende de T023, mesmo arquivo)
- [ ] T026 [US2] Verificar o TC-09 do diálogo (regra 3): em `front/src/app/categories/components/category-form/category-form.html`, trocar o `aria-label` do grupo e o do `input type="color"`, ver o T025 falhar, desfazer e confirmar verde (depende de T025)
- [ ] T027 [US2] TC-09, parte da tabela (FR-020): em `front/src/app/categories/components/categories-table/categories-table.spec.ts`, a dica do botão "Editar" é "Editar", usando o helper `tooltip()` já existente no spec (depende de T013, mesmo arquivo)
- [ ] T028 [US2] Verificar o TC-09 da tabela (regra 3): em `front/src/app/categories/components/categories-table/categories-table.html`, trocar o `matTooltip` de "Editar", ver o T027 falhar, desfazer e confirmar verde (depende de T027)

### Implementação da história 2 (já existente)

- [X] T029 [P] [US2] `ColorsService` com o `httpResource` de `data/default-colors.json`, `providedIn: 'root'` (paleta buscada uma vez) em `front/src/app/categories/services/colors.service.ts`, com `colors.service.spec.ts`
- [X] T030 [US2] Diálogo `CategoryForm` (Reactive Forms; "Nome" com `Validators.required` e `nameMaxLength = 100`; paleta com `aria-pressed`; "Cor personalizada"; primeira cor da paleta numa categoria nova) em `front/src/app/categories/components/category-form/category-form.ts`, `category-form.html` e `category-form.scss`. Injeta o `ColorsService`: exceção F1 aprovada pelo usuário em 2026-10-05 (plan, Acompanhamento de complexidade)
- [X] T031 [US2] "Nova categoria" e `openForm`/`save` ("Categoria criada."/"Categoria salva." por 3 s; erro com o `detail` da API ou "Não foi possível salvar a categoria." com "Fechar" por 5 s) em `front/src/app/categories/pages/categories-page/categories-page.ts` e `categories-page.html`; botão "Editar" em `front/src/app/categories/components/categories-table/categories-table.html`

**Checkpoint**: US2 caracterizada; `category-form.spec.ts` e `categories-table.spec.ts` verdes.

---

## Fase 5: História de usuário 3 - Excluir categorias sem uso e desfazer (Prioridade: P3)

**Objetivo**: "Excluir" imediato, desabilitado (com dica) para categorias com transações; "Categoria excluída." com
"Desfazer"; mensagens de erro.

**Teste independente**: excluir uma categoria sem transações, conferir a mensagem com "Desfazer" e desfazer; tentar
excluir uma com transações; conferir as falhas (quickstart.md).

### Testes da história 3 (caracterização)

- [ ] T032 [US3] TC-02 (FR-014): em `front/src/app/categories/pages/categories-page/categories-page.spec.ts`, uma exclusão que falha sem `ProblemDetails` (erro de rede, status 0) mostra "Não foi possível excluir a categoria." com "Fechar" por 5 s, sem recarregar a lista (depende de T009, mesmo arquivo)
- [ ] T033 [US3] Verificar o TC-02 (regra 3): em `front/src/app/categories/pages/categories-page/categories-page.ts`, trocar o texto "Não foi possível excluir a categoria.", ver o T032 falhar, desfazer e confirmar verde (depende de T032)
- [ ] T034 [US3] TC-03 (FR-013, FR-014): em `front/src/app/categories/pages/categories-page/categories-page.spec.ts`, um "Desfazer" recusado em dois casos com dados reais: um `500` com o `detail` "Ocorreu um erro inesperado." mostra esse `detail`; um erro de rede (status 0) mostra "Não foi possível salvar a categoria."; nos dois, "Fechar" por 5 s e sem recarregar a lista (depende de T032, mesmo arquivo)
- [ ] T035 [US3] Verificar o TC-03 (regra 3): em `front/src/app/categories/pages/categories-page/categories-page.ts`, primeiro fazer `showError` ignorar o `detail` (o caso do `500` falha), depois trocar o texto "Não foi possível salvar a categoria." (o caso de rede falha); desfazer e confirmar verde (depende de T034)

### Implementação da história 3 (já existente)

- [X] T036 [US3] `delete` com recarga, "Categoria excluída." com "Desfazer" por 5 s (recria a categoria; "Categoria restaurada.") e erros em `front/src/app/categories/pages/categories-page/categories-page.ts`; botão "Excluir" desabilitado com `disabledInteractive` e dica para categorias com transações em `front/src/app/categories/components/categories-table/categories-table.html`

**Checkpoint**: US3 caracterizada; `categories-page.spec.ts` verde.

---

## Fase 6: História de usuário 4 - Ver o uso de um mês (Prioridade: P4)

**Objetivo**: filtro "Mês" (MM/aaaa) que limita quantidade, saldo, "Transações no período" e ordenação por quantidade
ao mês escolhido.

**Teste independente**: com transações em meses diferentes, exibir os filtros, escolher um mês e conferir quantidade e
saldo do mês; limpar o mês (quickstart.md). Sem tarefa aberta: "Limpar mês" já está coberto em
`front/src/app/shared/month-field/month-field.spec.ts`.

### Implementação da história 4 (já existente)

- [X] T037 [US4] Campo "Mês" (`app-month-field`, visão do ano, "Limpar mês") no painel de filtros em `front/src/app/categories/components/categories-filters/categories-filters.html` e `categories-filters.ts`; `dateRef` (`DateUtils.toDateKey`, `YYYY-MM-DD`) enviado por `front/src/app/categories/services/categories.service.ts`

**Checkpoint**: US4 já coberta pelos specs existentes.

---

## Fase 7: História de usuário 5 - Encontrar e ordenar categorias (Prioridade: P5)

**Objetivo**: "Exibir filtros"/"Ocultar filtros", filtros "Nome" e "Transações no período" e menu "Ordenar: …".

**Teste independente**: buscar por parte de um nome, filtrar por ter ou não transações, ocultar e exibir os filtros e
trocar a ordenação (quickstart.md).

### Testes da história 5 (caracterização)

- [ ] T038 [US5] TC-04 (FR-015): em `front/src/app/categories/pages/categories-page/categories-page.spec.ts`, com filtros preenchidos, "Ocultar filtros" não refaz a consulta nem limpa os filtros, e "Exibir filtros" mostra os mesmos valores (depende de T034, mesmo arquivo)
- [ ] T039 [US5] Verificar o TC-04 (regra 3): em `front/src/app/categories/pages/categories-page/categories-page.html`, fazer o botão de filtros também limpar `filters` ao ocultar, ver o T038 falhar, desfazer e confirmar verde (depende de T038)
- [ ] T040 [US5] TC-09, dicas dos filtros (FR-020): em `front/src/app/categories/pages/categories-page/categories-page.spec.ts`, a dica do botão de filtros é "Exibir filtros" com os filtros ocultos e "Ocultar filtros" com eles visíveis (depende de T038, mesmo arquivo)
- [ ] T041 [US5] Verificar o TC-09 das dicas dos filtros (regra 3): em `front/src/app/categories/pages/categories-page/categories-page.html`, inverter os textos do `matTooltip` do botão de filtros, ver o T040 falhar, desfazer e confirmar verde (depende de T040)
- [ ] T042 [P] [US5] TC-09, `sort-menu` compartilhado (FR-020): em `front/src/app/shared/sort-menu/sort-menu.spec.ts`, a dica do botão é o mesmo `label()` do nome acessível, "Ordenar: …" (hoje o spec só confere o `aria-label`)
- [ ] T043 [US5] Verificar o TC-09 do `sort-menu` (regra 3): em `front/src/app/shared/sort-menu/sort-menu.html`, trocar `[matTooltip]="label()"` por um texto fixo, ver o T042 falhar, desfazer e confirmar verde (depende de T042)

### Implementação da história 5 (já existente)

- [X] T044 [US5] Filtros "Nome" (Signal Forms, debounce de 300 ms, "Limpar nome") e "Transações no período" ("Todas", "Com transações", "Sem transações") em `front/src/app/categories/components/categories-filters/categories-filters.ts`, `categories-filters.html` e `categories-filters.scss`, com `categories-filters.spec.ts`
- [X] T045 [US5] Botão "Exibir filtros"/"Ocultar filtros" e menu de ordenação (seis opções, padrão "Nome (A–Z)") em `front/src/app/categories/pages/categories-page/categories-page.ts` e `categories-page.html`, usando `front/src/app/shared/sort-menu/sort-menu.ts`

**Checkpoint**: US5 caracterizada; `categories-page.spec.ts` e `sort-menu.spec.ts` verdes.

---

## Fase 8: História de usuário 6 - Ir às transações de uma categoria (Prioridade: P6)

**Objetivo**: a quantidade de transações é um link para `/transactions?categoryId=<id>`.

**Teste independente**: escolher a quantidade de uma categoria e conferir a página de transações filtrada por ela
(quickstart.md). Sem tarefa aberta (coberto por `categories-table.spec.ts`).

### Implementação da história 6 (já existente)

- [X] T046 [US6] Link da quantidade (`routerLink="/transactions"`, `queryParams` com `categoryId`) em `front/src/app/categories/components/categories-table/categories-table.html`

**Checkpoint**: todas as histórias caracterizadas.

---

## Fase 9: Fechamento

**Objetivo**: confirmar que só specs mudaram e que a suíte, o lint e o build do front passam.

- [ ] T047 Conferir que `git diff --name-only -- front/src front/public` só lista `*.spec.ts` (`front/src/app/app.spec.ts`, `front/src/app/categories/**/*.spec.ts`, `front/src/app/shared/sort-menu/sort-menu.spec.ts`); os artefatos em `front/specs/002-gestao-de-categorias/` podem mudar (marcações e registro das verificações); que todas as quebras propositais foram desfeitas (nenhum arquivo de produção alterado), que nenhum teste existente foi apagado, ignorado ou enfraquecido, e que cada tarefa de verificação (T010, T012, T014, T016, T018, T022, T024, T026, T028, T033, T035, T039, T041, T043) tem a quebra e o teste que falhou registrados
- [ ] T048 Rodar, de dentro de `front/`, `npx ng test --watch=false` (suíte completa verde), `npm run lint` ("All files pass linting.") e `npm run build` (sem erro de budget); registrar os números no relatório do implement (depende de T047)

---

## Dependência de entrega (fora do tasks.md)

Decisão do usuário (2026-10-06, plan, "Dependências e riscos"): a feature só é **entregue**, no fim do
`/speckit-converge`, depois de três bug-fix do front, feitos pelo fluxo de bugs e **não** por tarefas deste arquivo:

| Defeito | Requisitos | Fluxo |
| --- | --- | --- |
| Consulta repetida ao escolher de novo a ordenação em uso, o mesmo mês, ou ao terminar a digitação com o texto já aplicado | FR-023, SC-006 (princípio III) | `/speckit-bug-assess` → `/speckit-bug-fix` → `/speckit-bug-test` em `front/bugs/<slug>/` |
| Calendário do "Mês" em inglês | FR-020 (Idioma) | idem |
| Clique repetido em "Excluir" (outro `DELETE`, sem retorno visual) | FR-012, FR-013 (princípio III) | idem |

**Critério de "concluído" de cada um**: `front/bugs/<slug>/test.md` com o resultado `verified`, o teste de reprodução e
a suíte completa do front passando; e o `/speckit-converge` desta feature reavaliando contra o código os requisitos
afetados (FR-023 e SC-006; FR-020; FR-012 e FR-013) antes de declarar a entrega. As tarefas T001 a T048 podem ser
implementadas antes desses bug-fix. Os bug-fix rodam os specs de `categories/`, incluindo os TC-01 a TC-10.

**Desvios conhecidos que não bloqueiam a entrega** (plan, "Desvios conhecidos"): também ficam fora deste arquivo e não viram tarefa no `/speckit-converge`: o link da quantidade sem o mês (FR-004), o diálogo que fecha antes da resposta da API (FR-010) e o dia escolhido na visão de dias do "Mês" (FR-016), pelo fluxo de bugs do front; a busca por nome que diferencia maiúsculas (FR-017) e o `dateRef` documentado como `date-time` no contrato, no back. O converge os registra como desvios conhecidos, sem anexar tarefas.

---

## Dependências e ordem de execução

### Dependências entre fases

- **Setup (Fase 1)** e **implementação já existente**: concluídas.
- **Fundacional (Fase 2)**: T009 e T010 vêm antes de qualquer tarefa aberta em `categories-page.spec.ts` (T015, T017,
  T032, T034, T038, T040), porque mudam o fixture `mercado` e os testes de erro desse arquivo.
- **Histórias (Fases 3 a 8)**: independentes entre si, exceto pelo arquivo compartilhado: as tarefas em
  `categories-page.spec.ts` rodam em sequência (T009 → T015 → T017 → T032 → T034 → T038 → T040), as de
  `category-form.spec.ts` também (T021 → T023 → T025), e as de `categories-table.spec.ts` (T013 → T027).
- **Fechamento (Fase 9)**: depois de todas as tarefas abertas.

### Dentro de cada TC

- Escrever o teste → rodar (passa; se falhar, parar e levar ao usuário) → tarefa de verificação (quebra proposital, vê
  falhar, desfaz, vê passar).
- As verificações que mexem no mesmo arquivo de produção (por exemplo, `categories-page.ts` em T010, T033 e T035) rodam
  uma de cada vez, sempre desfazendo a quebra anterior antes da próxima.

### Oportunidades de paralelismo

- Em arquivos diferentes: T011 (`app.spec.ts`), T013 (`categories-table.spec.ts`), T021 (`category-form.spec.ts`) e
  T042 (`sort-menu.spec.ts`) podem começar juntos, em paralelo à sequência de `categories-page.spec.ts`.
- As tarefas de verificação não rodam em paralelo entre si: cada uma quebra o código de produção e precisa da suíte do
  arquivo afetado isolada da quebra das outras.

---

## Exemplo de paralelismo

```bash
# Testes em arquivos diferentes, juntos:
Tarefa: "T011 [US1] TC-06 em front/src/app/app.spec.ts"
Tarefa: "T013 [US1] TC-10 em front/src/app/categories/components/categories-table/categories-table.spec.ts"
Tarefa: "T021 [US2] TC-01 em front/src/app/categories/components/category-form/category-form.spec.ts"
Tarefa: "T042 [US5] TC-09 em front/src/app/shared/sort-menu/sort-menu.spec.ts"
```

---

## Estratégia de implementação

### MVP (só a história 1)

1. Fase 2: T009 e T010 (ajuste dos dados de teste da página).
2. Fase 3: T011 a T018.
3. **Parar e validar**: `npx ng test --watch=false` verde.

### Entrega incremental

1. Fundacional → US1 → US2 → US3 → US5 (US4 e US6 sem tarefa aberta), cada uma com o spec do arquivo verde.
2. Fechamento: T047 e T048.
3. `/speckit-converge` confere contra a spec; a entrega só é declarada depois dos três bug-fix (Dependência de entrega).

---

## Notas

- `[P]` = arquivo diferente, sem dependência de tarefa aberta.
- Nenhuma tarefa altera código de produção; as quebras das tarefas de verificação são temporárias e nunca vão para
  commit.
- Um TC que falha na primeira execução para o implement e vai ao usuário (regra 2).
- Ficam só documentados, sem tarefa: "Limpar mês" no painel de categorias (já coberto em
  `front/src/app/shared/month-field/month-field.spec.ts`) e a "Cor personalizada" sobrescrita pela paleta que chega
  atrasada (comportamento aceito, frágil de testar).
