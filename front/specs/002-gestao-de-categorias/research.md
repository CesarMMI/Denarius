# Pesquisa da Fase 0: Gestão de categorias

Este é um plano **retroativo**: "pesquisa" aqui significa confirmar as decisões já embutidas no código e nos testes de
`front/src/app/categories/` (e das peças de `front/src/app/shared/` que a página usa), e não explorar opções para código
ainda não escrito. Cada item resolve o que, de outro modo, seria um `NEEDS CLARIFICATION` no Contexto técnico. As
citações `arquivo:linha` são relativas a `front/src/app/`, salvo indicação. Os pontos levados ao usuário foram decididos
em 2026-10-05 e 2026-10-06 e estão em [Decisões do usuário](#decisões-do-usuário), no fim.

Por ser retroativo, o código não registra as alternativas que os autores avaliaram. Onde há uma alternativa óbvia, ela
aparece abaixo com o motivo por que o código atual a evita, verificável no próprio código ou na constituição; onde não
há, o item diz isso explicitamente.

## Estrutura da feature e carregamento sob demanda

- **Decisão**: Uma pasta `categories/` com `pages/`, `components/`, `services/`, `types/` e `testing/`, carregada sob
  demanda: `app.routes.ts:13-16` usa `loadChildren` para `categories.routes.ts`, que por sua vez usa `loadComponent`
  para a `CategoriesPage` (`categories.routes.ts:3-8`). O item "Categorias" (ícone `sell`) é o terceiro do menu
  (`app.ts:24-26`), marcado como página atual pelo `routerLinkActive` de `app.html:9-15`.
- **Justificativa**: É o padrão que a F1 descreve e que `transactions/` e `reports/` seguem; atende à FR-001 e à
  SC-009 (a página só é baixada quando aberta).
- **Alternativas consideradas**: Nenhuma registrada (retroativo); o padrão é do projeto. A alternativa óbvia, uma rota
  carregada junto com a aplicação, aumentaria o bundle inicial e iria contra a restrição de performance do
  `front/AGENTS.md` e a SC-009.

## Quem faz HTTP e quem guarda o estado

- **Decisão**: O `CategoriesService` (`categories/services/categories.service.ts`) concentra a URL e os parâmetros:
  `list()` devolve só a requisição `{ url, params }` para um `httpResource` (`:17-24`), e `create`/`update`/`delete`
  devolvem `Observable`s do `HttpClient` (`:26-36`). A `CategoriesPage` guarda o estado: os signals `filters`,
  `filtersVisible` e `sort` (`categories-page.ts:40-43`) e o `httpResource` da lista, que depende de `filters()` e
  `sort()` (`:53-55`). Ela também orquestra o diálogo e as mensagens (`:57-95`). A tabela recebe o `Resource` como
  input e emite `edit`/`delete` (`categories-table.ts:30-32`); os filtros recebem e devolvem `CategoryFilters` por um
  `model` (`categories-filters.ts:17`).
- **Justificativa**: F1 (HTTP só nos services; páginas com o estado; componentes de apresentação) e o padrão das outras
  features.
- **Alternativas consideradas**: Nenhuma registrada (retroativo). As óbvias — a página chamar o `HttpClient`
  diretamente, ou a tabela ter o seu próprio `httpResource` — são proibidas pela F1 (URLs só nos services; componentes
  sem HTTP). A exceção à fronteira está no item seguinte.

## Paleta padrão: arquivo estático, buscada uma vez

- **Decisão**: A paleta é o asset estático `front/public/data/default-colors.json` (as 11 cores da Premissa da spec, na
  mesma ordem), lido por um `httpResource` dentro do `ColorsService` (`colors.service.ts:4,10-13`), `providedIn: 'root'`.
  O `CategoryForm` injeta o service (`category-form.ts:21,25`); por isso, a primeira abertura do diálogo dispara
  `GET data/default-colors.json`, e as seguintes reaproveitam o resultado, até a aplicação ser recarregada. Sem a
  paleta, `colors()` fica vazia (`:13`), e o diálogo oferece só a "Cor personalizada".
- **Justificativa**: Atende à SC-009 (a paleta só é baixada quando o diálogo abre pela primeira vez) e ao caso-limite
  da paleta reaproveitada; a paleta vem com a aplicação, e não da API (Premissa), então mudá-la não muda o back.
- **Alternativas consideradas**: A página carregar a paleta e passá-la ao diálogo em `data` — obrigaria a buscá-la
  antes de o diálogo abrir, sem ganho para o usuário. O efeito colateral da decisão é que um componente de
  `components/` provoca uma requisição HTTP (indiretamente, pelo service), contra a letra da F1: o usuário aprovou isso
  como exceção em 2026-10-05 (D4), registrada no Acompanhamento de complexidade do plan.
- **Observação**: `ColorsService.reload()` (`colors.service.ts:15-17`) só é chamado pelo seu spec
  (`categories/services/colors.service.spec.ts:50-58`); nenhum código de produção o usa, o que é coerente com o
  caso-limite "a paleta só é buscada de novo quando a aplicação é recarregada". Será removido por um ciclo de
  refatoração próprio (D6), e não nesta feature.

## Formulários: Reactive Forms no diálogo, Signal Forms nos filtros

- **Decisão**: O diálogo usa `FormGroup`/`FormControl` com `Validators.required` (`category-form.ts:27-30`), como o
  `transaction-form` de `transactions/`; os filtros usam Signal Forms (`form(this.filters, …)` com `debounce` de 300 ms
  no nome, `categories-filters.ts:19`), como os `transactions-filters`.
- **Justificativa**: O debounce declarativo do Signal Forms sobre o `model` atende à FR-017 (pausa de 300 ms ou saída
  do campo); o diálogo fecha devolvendo `getRawValue()` (`category-form.ts:40-42`). É o padrão que as duas features
  seguem hoje.
- **Alternativas consideradas**: Nenhuma registrada (retroativo). A óbvia é usar Signal Forms também no diálogo,
  unificando a API de formulários da feature; o código não diz por que não, e os dois diálogos de formulário do projeto
  (`category-form` e `transaction-form`) usam Reactive Forms. Fica documentado como está, sem proposta de unificação,
  que seria refatoração por ciclo próprio.

## Validação do nome e da cor

- **Decisão**: No front, o nome é obrigatório (`Validators.required`, mensagem "Informe um nome" em
  `category-form.html:9`) e limitado a 100 caracteres pelo atributo `maxlength` (`category-form.html:7`, com
  `nameMaxLength = 100` em `category-form.ts:23`), com o contador `N/100` (`category-form.html:8`). A cor começa em
  `#000000` (`category-form.ts:29`) e, numa categoria nova, passa à primeira cor da paleta quando ela chega (efeito em
  `:34-37`). O back valida de novo: nome vazio ou só com espaços e mais de 100 caracteres
  (`back/src/Denarius.Domain/Entities/Category.cs:35,40`) e cor vazia ou inválida (`back/src/Denarius.Domain/ValueObjects/Color.cs:13,18`).
- **Justificativa**: Princípio II (a validação do front serve à experiência; a da API é a autoridade) e FR-008/FR-022.
- **Alternativas consideradas**: Recusar só espaços no front — o usuário viu e não pediu mudança (Esclarecimentos,
  2026-10-04); fica como comportamento atual.
- **Lacuna**: o limite de 100 caracteres e o contador não têm teste; vira o teste de caracterização TC-01 (D1).

## Mensagens de sucesso e de erro

- **Decisão**: `save()` mostra a mensagem de sucesso por 3 s e recarrega a lista (`categories-page.ts:83-88`); a
  exclusão recarrega e oferece "Desfazer" por 5 s (`:69-77`), que chama `create` com o mesmo nome e cor (`:77`). Os
  erros mostram `error.error?.detail` ou um texto da página, com "Fechar", por 5 s, sem recarregar (`:79,89,93-95`).
- **Justificativa**: FR-011, FR-013, FR-014 e SC-007. O `detail` vem do `ProblemDetails` do `GlobalExceptionHandler`
  do back, que mapeia `NotFoundException` → 404, `DomainException`/`AppException` → 400 e o resto → 500, com o
  `detail` genérico "Ocorreu um erro inesperado." no 500
  (`back/src/Denarius.WebAPI/Middleware/GlobalExceptionHandler.cs:25,30-36`). Por isso, os textos da página só
  aparecem quando a resposta não traz `ProblemDetails` (por exemplo, sem conexão), como diz o caso-limite da spec.
- **Idioma das mensagens da API**: os `detail` do back são sempre em português (as mensagens de domínio, "Categoria
  não encontrada." e o genérico "Ocorreu um erro inesperado." do 500); o 400 automático de validação do ASP.NET, cujo
  título é em inglês, vem sem `detail` e cai no texto da página.
- **`detail` vazio**: o `??` de `categories-page.ts:94` só usa o texto da página quando o `detail` não existe
  (`null`/`undefined`); um `detail` vazio (`""`) mostraria uma mensagem vazia. É comportamento atual, que o back de
  hoje nunca produz (os seus `detail` são textos fixos não vazios); fica registrado, sem trabalho novo.
- **Alternativas consideradas**: Confirmação antes de excluir — o usuário viu e não pediu mudança.
- **Lacunas**: alguns dados de teste não correspondem ao que a API produz (ajuste TC-08, D2 e D9), e o fallback da
  exclusão e a falha do "Desfazer" não têm teste (TC-02 e TC-03, D1).

## Exibição como texto (segurança)

- **Decisão**: Nomes e mensagens são interpolados (`{{ category.name }}` em `categories-table.html:5-7`) ou passados
  como texto ao `MatSnackBar.open` (`categories-page.ts:94`); não há `innerHTML` nem `bypassSecurityTrust*` em
  `categories/` ou `shared/` (busca no código).
- **Justificativa**: FR-021, princípio II e a restrição de segurança do `front/AGENTS.md`.
- **Alternativas consideradas**: Nenhuma registrada (retroativo). A óbvia, `[innerHTML]` (por exemplo, para destacar
  o trecho buscado no nome), é rejeitada pela restrição de segurança do `front/AGENTS.md` (nada de `innerHTML` com
  conteúdo dinâmico) e pela FR-021.
- **Verificação**: o TC-10 (D12) confere que um nome `<b>teste</b>` aparece literalmente no chip. Só os templates do
  projeto entram no teste; o texto do snack bar é exibido como texto pelo próprio Material (`MatSnackBar.open` recebe
  uma string, que o `SimpleSnackBar` mostra por interpolação), garantia documentada sem teste.

## Rótulo da categoria com contraste garantido

- **Decisão**: A diretiva compartilhada `MatChipColor` (`shared/mat-chip-color/mat-chip-color.ts:3-29`) pinta o chip
  com os tons 90 e 30 da cor (tons do Material), trocados no tema escuro por `light-dark(…)`; o `mat-chip` da tabela
  tem `pointer-events: none` (`categories-table.scss:7-9`) para ser só um rótulo.
- **Justificativa**: FR-003 e SC-005 (7:1 em qualquer cor, inclusive preto e branco), verificado por
  `shared/mat-chip-color/mat-chip-color.spec.ts:37-45` para as cores críticas da paleta e os extremos. É código próprio
  (F4), justificado porque o Material não deriva tons legíveis de uma cor arbitrária.
- **Alternativas consideradas**: Usar a cor exata como fundo — falha no contraste em cores médias (registrado no
  comentário da diretiva).

## Filtros, mês e ordenação

- **Decisão**: Os filtros começam vazios e ocultos (`categories-page.ts:40-41`); o painel é um `@if`
  (`categories-page.html:24-28`), mas o estado fica no signal da página, então ocultar não limpa (FR-015). O
  `CategoriesService.list` omite os filtros vazios e envia `name`, `withTransaction`, `dateRef` (o dia 1 do mês como
  `YYYY-MM-DD`, por `DateUtils.toDateKey`, `shared/date-utils/date-utils.ts:16-18`) e `orderBy`/`asc`
  (`categories.service.ts:19-22`). O "Mês" é o `MonthField` compartilhado (`shared/month-field/`), aberto na visão do
  ano, exibido como `MM/aaaa` (`month-field.ts:16-19`), com "Limpar mês" quando `clearable`
  (`month-field.html:11-15`). A ordenação é o `SortMenu` compartilhado, com o rótulo "Ordenar: …" e a opção marcada por
  `aria-checked` (`shared/sort-menu/`), e as seis opções da página (`categories-page.ts:44-51`).
- **Justificativa**: FR-015 a FR-019; filtro e ordenação na fonte dos dados (princípio III).
- **Alternativas consideradas**: Filtros na URL — fora do escopo (Premissa: valem só enquanto a página está aberta).
- **Desvios conhecidos** (fora desta feature; ver o Constitution Check do plan): a consulta repetida
  (`sort-menu.ts:30-32` e `month-field.html:17` sempre gravam um objeto novo, e o `httpResource` de
  `categories-page.ts:53-55` refaz a requisição; no "Nome", deduzido de `categories-filters.ts:19`), o dia escolhido
  na visão de dias que não chega ao `value` (só o `monthSelected` o grava, `month-field.html:17`) e o calendário sem
  rótulos em português (nenhum `MatDatepickerIntl` é provido no código).

## Lista, estados e link para transações

- **Decisão**: A tabela usa `rows()` com `hasValue()` (`categories-table.ts:36-40`), porque `value()` lança no erro; a
  linha "sem dados" mostra o spinner, o erro ou "Nenhuma categoria encontrada." (`categories-table.html:51-61`). Como o
  `httpResource` mantém o valor no `reload()`, as linhas continuam visíveis ao recarregar, e o spinner só aparece sem
  linhas. O `reload()` não faz nada enquanto o resource está em `loading` (a primeira carga ou depois de uma mudança
  de filtro), e, numa recarga já em andamento, a reinicia: cada clique em "Recarregar" envia uma nova requisição e cancela a
  anterior (comportamento atual; o "Recarregar" sem indicador foi visto e mantido pelo usuário). A
  quantidade é um `routerLink` para `/transactions` com `categoryId` (`categories-table.html:14`); o saldo
  usa `currency: 'BRL'` sob o `LOCALE_ID` `pt-BR` (`app.config.ts:13`) e a classe global `.negative`
  (`categories-table.html:20`). O "Excluir" usa `disabledInteractive` para manter a dica e protege o clique com
  `category.canDelete &&` (`categories-table.html:32-44`).
- **Justificativa**: FR-002 a FR-006 e FR-012; o comportamento de recarga é o do caso-limite da spec.
- **Alternativas consideradas**: Nenhuma registrada (retroativo). As óbvias e por que o código as evita: ler
  `value()` em vez de `hasValue()` — lança no estado de erro (comentário em `categories-table.ts:36`); um `[disabled]`
  simples no "Excluir" — o botão desabilitado não mostraria a dica do motivo (comentário em `categories-table.html:32`);
  um spinner a cada recarga — esconderia as linhas atuais sem necessidade, o que o caso-limite da spec descarta.
- **Desvio conhecido**: o link não leva o mês (FR-004) — fora desta feature.

## Contrato consumido

- **Decisão**: A página consome só `GET/POST /api/categories` e `PUT/DELETE /api/categories/{id}` de
  `back/specs/001-category-management/contracts/categories-api.yaml`; `types/category.ts` espelha os oito campos de
  `CategoryOutput`, e `CategoryInput` (`Pick<Category, 'name' | 'color'>`) espelha `Create/UpdateCategoryInput`.
- **Justificativa**: Princípio IV e F2.
- **Divergências**: o contrato declara `dateRef` como `format: date-time` (`categories-api.yaml:26-32`; o mesmo em
  `back/specs/002-transaction-management/contracts/transactions-api.yaml:27-33`), mas o front envia só a data
  (`YYYY-MM-DD`), e o back a aceita — o próprio teste do controller usa `dateRef=2026-09-01`
  (`back/tests/Denarius.WebAPI.Tests/Categories/CategoriesControllerTests.cs:211`). O usuário decidiu (D3) que é um
  desvio conhecido do back, só de documentação do contrato, a corrigir pelo fluxo de bugs no back, fora desta spec;
  o front não muda, e esta feature não é bloqueada. A busca por nome usa `Contains` (`back/src/Denarius.Infrastructure/Repositories/CategoryRepository.cs:14-15`),
  cuja diferenciação de maiúsculas o contrato deixa "a cargo do banco": é o desvio da FR-017, destinado ao back.

## Abordagem de testes

- **Decisão**: Specs ao lado de cada arquivo, como na skill `write-front-tests`: o service com
  `HttpTestingController`; a tabela com `resourceFromSnapshots`; a página com `HttpTestingController` e dublês de
  `MatDialog` e `MatSnackBar`; os filtros com harnesses do Material; o diálogo com dublês de `MatDialogRef` e
  `MAT_DIALOG_DATA`. Em 2026-10-05, `npx ng test --watch=false` com os specs de `categories/`, `shared/` e
  `app.spec.ts` passou: 12 arquivos, 95 testes.
- **Justificativa**: Princípio I e "Testes e verificação" do `front/AGENTS.md`.
- **Cobertura por requisito**: está no [quickstart.md](./quickstart.md#1-validação-automatizada).

## Decisões do usuário

Os pontos levados ao usuário depois da primeira versão deste plan (D1 a D7 em 2026-10-05; D8 a D10 em 2026-10-06, depois
da revisão independente; D11 a D16 em 2026-10-06, depois da revisão dos checklists), com as decisões tomadas. As tarefas TC-01 a TC-10 estão detalhadas em [Trabalho desta feature](./plan.md#trabalho-desta-feature), no plan.

- **D1 — Lacunas de teste: testes de caracterização nesta feature.**
  - Constatado: não há teste do `maxlength` nem do contador `N/100`; a busca por `100`, `maxlength` e `nameMaxLength`
    nos specs de `categories/` não encontra nada. Também faltavam testes de outros comportamentos aceitos.
  - Decisão: viram testes de caracterização desta feature: limite de 100 com o contador (TC-01); fallback "Não foi
    possível excluir a categoria." (TC-02; hoje só o caso com `detail` é testado, em `categories-page.spec.ts:285-300`);
    falha do "Desfazer", com um 500 real ("Ocorreu um erro inesperado.") e um erro de rede (TC-03); filtros mantidos ao ocultar e exibir de novo (TC-04; `:152-162` só confere que o
    painel some); linhas mantidas, sem spinner, na recarga (TC-05); item "Categorias" marcado como atual (TC-06; o
    `app.spec.ts` confere "Relatórios" (`:39-45`) e "Transações" (`:47-53`), mas não "Categorias"); edição com cor fora da paleta (TC-07).
  - Ficam só documentados: "Limpar mês" no painel de categorias, já coberto em `shared/month-field/month-field.spec.ts:73-85`,
    e a "Cor personalizada" sobrescrita pela paleta que chega atrasada (comportamento aceito, frágil de testar).
  - Justificativa: são comportamentos existentes e aceitos na spec, e não comportamento novo; pela natureza retroativa,
    os testes passam desde a primeira execução. É o precedente do back 001, que fechou a lacuna do controller no
    implement da feature retroativa.
- **D2 — Dados de teste que a API não produz: ajustar nesta feature (TC-08).**
  - Constatado: `categories-page.spec.ts:226-240` simula um `409 Conflict` com "Já existe uma categoria com esse
    nome.", mas o back não tem regra de unicidade (não há índice único em `CategoryConfiguration.cs`, e a spec diz que
    os nomes não precisam ser únicos) nem produz 409 (o handler só gera 400, 404 e 500); `:291` (com a asserção em
    `:295`) usa "A categoria possui transações vinculadas.", enquanto o back responde "Não é possível excluir uma
    categoria que possui transações associadas." (`back/src/Denarius.Application/UseCases/Categories/Delete/DeleteCategoryUseCase.cs:15`);
    e `:246` simula um 500 sem corpo, quando o back responde 500 com o `detail` "Ocorreu um erro inesperado.".
  - Decisão: no lugar do 409, um nome só com espaços (`'   '`) recusado com `400` e "O nome da categoria não pode ser
    vazio." (`back/src/Denarius.Domain/Entities/Category.cs:35`), o que caracteriza a parte da página do caso-limite
    do nome só com espaços (a mensagem da API); a parte do diálogo, em que o `Validators.required` aceita espaços e o
    nome passa, fica sem teste, coerente com o escopo decidido; a mensagem real da exclusão; e um erro de rede (status 0) no lugar do 500 sem corpo, para o
    fallback. As asserções de comportamento ficam iguais, e nenhum teste
    fica mais fraco.
- **D3 — `dateRef` documentado como `date-time`: desvio conhecido do back.**
  - Decisão: só de documentação do contrato (`categories-api.yaml` e `transactions-api.yaml`), a corrigir pelo fluxo de
    bugs no back, fora desta spec. Não bloqueia esta feature e não vira tarefa; está na tabela de desvios do plan, ao
    lado da busca que diferencia maiúsculas.
- **D4 — `CategoryForm` provoca HTTP pelo `ColorsService`: exceção à F1 aprovada.**
  - Decisão: aprovada pelo usuário em 2026-10-05 e registrada no Acompanhamento de complexidade do plan. Justificativa:
    a paleta é um asset estático, e não a API; o service `root` funciona como cache; e assim a paleta só é baixada na
    primeira abertura do diálogo (SC-009).
- **D5 — Imports de `categories/` por `transactions/` (decidido no plan da 003).**
  - Decisão: exceção registrada no plan da [003-gestao-de-transacoes](../003-gestao-de-transacoes/plan.md), válida até
    um ciclo próprio de refatoração mover `Category`, a fixture e a requisição da lista de categorias para `shared/`;
    esse ciclo também tocará `categories/`. Aqui, só a referência.
- **D6 — `ColorsService.reload()` sem uso em produção: remover depois.**
  - Decisão: remoção por um ciclo de refatoração próprio (princípio V), e não nesta feature.
- **D7 — Teste vermelho fora do escopo (`reports/services/chart-theme.service.spec.ts`).**
  - Decisão: o usuário abriu um `/speckit-bug-assess` no front para `reports/`, antes do implement destas features. É
    dependência desta feature: o `/speckit-implement` só começa com a suíte completa verde. Não vira tarefa daqui.
- **D8 — Nomes acessíveis e textos sem teste: novo TC-09 (2026-10-06).**
  - Constatado: o grupo "Cor" (`role="group" aria-label="Cor"`, `category-form.html:12`), o nome "Cor personalizada"
    (`:26`), o título "Categorias" (`categories-page.html:1`) e as dicas de "Recarregar" e de "Exibir filtros"/"Ocultar
    filtros" (`categories-page.html:2,8`) não têm teste; os specs só usam os `aria-label` desses botões como seletores.
  - Também sem teste: a dica de "Editar" (`categories-table.html:29`; o `categories-table.spec.ts` só confere a dica do
    "Excluir") e a dica "Ordenar: …" do `sort-menu` (`sort-menu.html:1`; o `sort-menu.spec.ts:46,51` só confere o
    `aria-label`).
  - Decisão: teste de caracterização TC-09, em `category-form.spec.ts`, `categories-page.spec.ts`,
    `categories-table.spec.ts` (com o helper `tooltip()`, `:64-69`) e `shared/sort-menu/sort-menu.spec.ts`; as duas
    últimas dicas entraram por decisão do usuário em 2026-10-06. A asserção do `sort-menu` fica em `shared/`, como a do
    TC-06 fica no `app.spec.ts`.
- **D9 — Fixture `mercado` no teste de exclusão recusada: entra no TC-08 (2026-10-06).**
  - Constatado: `categories-page.spec.ts:285-300` exclui o `mercado` (`:23-29`), que tem 14 transações, saldo
    negativo e `canDelete: true` (o padrão do `buildCategory`), uma combinação que a API não produz.
  - Decisão (texto igual ao do TC-08(c) no plan): o fixture `mercado` (`:23-29`) fica como uma categoria com transações coerente — 14 transações, saldo negativo (`balance: -1842.55`) e `canDelete: false` —, e o teste passa a excluir uma categoria com 0 transações, saldo 0 e `canDelete: true` (como o `educacao`, linha 1 da lista); a API a recusa com a mensagem real, o cenário de uma lista desatualizada (a categoria ganhou transações depois de a lista carregar). As asserções continuam as mesmas.
- **D10 — Violações do princípio III e de Idioma bloqueiam a entrega (2026-10-06; ampliado por D11).**
  - Decisão: a feature só é dada como entregue, no fim do `/speckit-converge`, depois de concluídos os bug-fix da
    consulta repetida (FR-023), do calendário em inglês (FR-020) e do clique repetido em "Excluir" (FR-012, FR-013; D11). Até lá, o implement das tarefas de teste pode rodar,
    desde que a suíte completa esteja verde (D7).
- **D11 — "Excluir" sem proteção contra clique repetido: desvio conhecido do princípio III, que bloqueia a entrega
  (2026-10-06).**
  - Constatado: `CategoriesPage.delete()` (`categories-page.ts:69-81`) não guarda estado de exclusão em andamento, e o
    "Excluir" só depende de `canDelete` (`categories-table.html:33-44`). Um segundo clique na mesma linha, antes de a
    lista recarregar, envia outro `DELETE`, inútil, que a API recusa ("Categoria não encontrada.",
    `DeleteCategoryUseCase.cs:12`), e essa mensagem toma o lugar da que oferece "Desfazer". Também nada indica a
    exclusão nem a restauração em andamento.
  - Decisão: defeito, registrado na spec (FR-012, FR-013, Casos-limite e Esclarecimentos de 2026-10-06), a corrigir
    pelo fluxo de bugs do front; a forma da correção fica para o `/speckit-bug-assess`. Bloqueia a entrega desta
    feature e da 003, como a consulta repetida e o calendário em inglês (D10).
- **D12 — Exibição como texto: novo TC-10 (2026-10-06).**
  - Decisão: teste de caracterização em `categories-table.spec.ts`: um nome `<b>teste</b>` aparece literalmente no
    chip. Só os templates do projeto; o snack bar fica como garantia do Material, sem teste. Um exemplo entrou nos
    Casos-limite da spec.
- **D13 — Acessibilidade: documentar e deixar como trabalho futuro (2026-10-06).**
  - Decisão: o comportamento atual, apoiado no Material, fica nas Premissas e nos Casos-limite da spec; os pontos
    fracos (link da quantidade anunciado só pelo número, foco perdido depois de excluir, "Desfazer" com 5 s fixos,
    #F6BF26 e #C0CA33 com cerca de 1,7:1, título da aba fixo e estados da lista não anunciados) são trabalho futuro de
    uma feature própria de acessibilidade, para a 002 e a 003, sem código e sem bloqueio.
- **D14 — Metas de desempenho (2026-10-06).**
  - Decisão: não há meta de tempo (uso pessoal, dezenas de categorias; as metas de contagem SC-006 e SC-009 ocupam
    esse lugar); a falta de paginação é revista se uma medição mostrar lentidão. Registrado nas Premissas da spec.
- **D15 — Comportamentos atuais aceitos (2026-10-06).**
  - Decisão: documentados nos Casos-limite da spec: categorias com o mesmo nome e a mesma cor não se distinguem; as
    mensagens padrão não dizem o que fazer; a quantidade aparece sem separador de milhar ("1234", interpolada sem
    `number` pipe em `categories-table.html:14`; a FR-005 define o formato só do saldo, então não há conflito); a
    quantidade "0" continua link; e um nome longo fica numa linha só, sem dica.
  - Conferido no código e no CSS para o nome longo: o rótulo do chip do Material tem `white-space: nowrap`,
    `overflow: hidden` e `text-overflow: ellipsis`, e o chip, `max-width: 100%`; mas a tabela não fixa a largura da
    coluna "Nome" (nem `table-layout: fixed` nem `max-width` em `categories-table.scss` ou nos estilos globais), então,
    com um nome de 100 caracteres, a coluna tende a crescer até caber o nome, e o cartão da lista rola na horizontal
    (`overflow: auto`, `categories-page.scss:14-17`); as reticências só aparecem se a largura do chip for limitada.
    Deduzido do CSS, sem renderização; o quickstart manda conferir.
- **D16 — Critério de "concluído" dos bug-fix que bloqueiam a entrega (2026-10-06).**
  - Decisão: o `front/bugs/<slug>/test.md` de cada bug com o resultado `verified`, o teste de reprodução e a suíte
    completa passando, e o `/speckit-converge` reavaliando contra o código os FR/SC afetados (FR-023 e SC-006; FR-020;
    FR-012 e FR-013). Registrado em "Dependências e riscos" do plan.
