# Pesquisa da Fase 0: Gestão de transações

Este é um plano **retroativo**: cada decisão abaixo foi lida no código de `front/src/app/transactions/` (e no que ele
usa de `shared/`, de `categories/` e do shell em `app.*`) e nos seus `*.spec.ts`, e não escolhida agora. Onde o
comportamento atual difere do esperado pela spec, a decisão registra o desvio conhecido e o fluxo que vai corrigi-lo.
O único trabalho desta feature são testes de caracterização e ajustes de dados de teste, decididos pelo usuário em
2026-10-05 (ver [Decisões do usuário](#decisões-do-usuário-2026-10-05), no fim). Os caminhos são relativos a
`front/src/app/`.

O Contexto técnico do `plan.md` não tem nenhum `NEEDS CLARIFICATION`: tudo foi verificado no código, e não resta
pendência de decisão do usuário.

## Quem carrega os dados e quem é dono do estado

- **Decisão**: A `TransactionsPage` é dona de dois `httpResource`s: `transactions`, cuja requisição depende dos signals
  `filters` e `sort` (`transactions-page.ts:73-75`), e `categories`, cuja requisição não depende de nenhum signal e por
  isso só é feita ao abrir a página e em `reload()` (`transactions-page.ts:77`, `:80-83`). A tabela, os filtros e o
  formulário são de apresentação e recebem o que precisam por input ou por `MAT_DIALOG_DATA`.
- **Justificativa**: É a divisão da F1 (`front/AGENTS.md`): a página guarda o estado e orquestra diálogos e snackbars;
  os componentes não fazem HTTP. Uma requisição de categorias sem signals garante a parte da FR-031 e da SC-007 que diz
  que mudar filtro ou ordenação não busca as categorias de novo.
- **Alternativas consideradas**: Um `httpResource` dentro da tabela ou dos filtros — fere a F1; buscar as categorias
  junto com cada consulta de transações — repete consultas sem necessidade (princípio III).

## Service: requisição para `httpResource` e `Observable`s para as mutações

- **Decisão**: `TransactionsService.list(filters, sort)` não faz a chamada: devolve `{ url, params }` para o
  `httpResource` (`transactions.service.ts:16-25`). `create`, `update` e `delete` devolvem os `Observable`s do
  `HttpClient` (`:27-37`), assinados pela página. URL e parâmetros ficam só no service; a URL base vem de
  `environment.apiUrl` (`:14`).
- **Justificativa**: Concentra a superfície HTTP num lugar auditável (F1) e deixa o carregamento reativo aos signals sem
  código de assinatura na página. É o mesmo formato do `CategoriesService`
  (`categories/services/categories.service.ts:17-24`) e do `ReportsService`.
- **Alternativas consideradas**: Métodos que devolvem `Observable` também para a lista — exigiriam gerenciar assinatura,
  carregamento e erro à mão.

## Filtragem e ordenação na API (FR-031)

- **Decisão**: Cada filtro preenchido vira um parâmetro de query e os vazios são omitidos
  (`transactions.service.ts:19-23`): `description`, `type` (`in`/`out`), `categoryId`, `dateRef` (o primeiro dia do mês,
  como `YYYY-MM-DD`, via `DateUtils.toDateKey`) e, quando há direção, `orderBy` + `asc`. A página sempre tem uma
  ordenação, então a primeira consulta já leva `orderBy=date&asc=false` (`transactions-page.ts:61`; teste em
  `transactions-page.spec.ts:146-149`). A lista não é paginada e chega inteira (premissa da spec).
- **Justificativa**: Princípio III: filtros e ordenação acontecem na fonte dos dados. O backend já oferece todos os
  parâmetros (`back/specs/002-transaction-management/contracts/transactions-api.yaml:18-59`). Do lado do back, a API
  carrega todas as transações e filtra e ordena em memória (ver
  [Observação de desempenho fora do escopo do front](#observação-de-desempenho-fora-do-escopo-do-front)); para o front,
  a fonte dos dados é a API.
- **Alternativas consideradas**: Filtrar e ordenar no cliente — exigiria carregar tudo, contra o princípio III.
- **Violação corrigida (princípio III)**: escolher de novo a ordenação, o mesmo mês ou o mesmo texto refazia a
  consulta, porque o `SortMenu` e o `MonthField` gravavam sempre um valor novo e o `httpResource` compara a requisição
  por referência. Decidido como defeito nos Esclarecimentos, sem exceção aprovada, bloqueava a entrega desta feature
  (decisão do usuário, 2026-10-06). A correção (`front/bugs/consulta-repetida-sem-mudanca/`) compara o estado da
  página por conteúdo (`shared/shallow-equal`, `equal` dos signals `filters` e `sort`).

## Estados de carregamento, vazio e erro (FR-008, FR-009)

- **Decisão**: A tabela recebe os dois `Resource`s e só mostra linhas quando os dois têm valor
  (`transactions-table.ts:38-42`); senão, a linha `*matNoDataRow` mostra o spinner se algum está carregando, a mensagem
  de erro se algum falhou, e "Nenhuma transação encontrada." no resto (`transactions-table.html:47-57`). Numa recarga
  (`reload()`), o `httpResource` fica em `reloading` e mantém o valor anterior, então as linhas continuam à vista; ao
  mudar filtro ou ordenação, a requisição muda, o recurso vai para `loading` sem valor e o spinner ocupa o lugar das
  linhas. Comportamento atual do `httpResource`, aceito: quando a requisição muda com uma carga em andamento, a anterior
  é abortada e só a resposta da consulta mais recente aparece; "Recarregar" (`reload()`) é ignorado enquanto o recurso
  está em `loading`, e, em `reloading`, cada clique refaz a consulta e aborta a anterior.
- **Justificativa**: Cada linha depende do nome e da cor da sua categoria, então mostrar transações sem as categorias
  produziria linhas incompletas. `hasValue()` é usado no lugar de `value()` porque `value()` lança enquanto o recurso
  está em erro (comentário em `transactions-table.ts:38`).
- **Alternativas consideradas**: Mostrar as linhas sem etiqueta enquanto as categorias carregam — linhas que mudam de
  aparência depois de exibidas; um estado de erro separado para as categorias — a spec pede uma única mensagem.

## Agrupamento por data (FR-007)

- **Decisão**: `sameDate` guarda as transações seguidas por outra com o mesmo `date.slice(0, 10)`
  (`transactions-table.ts:43-47`), e a classe `same-date` torna transparente a borda da linha
  (`transactions-table.scss:1-4`, `transactions-table.html:46`). Vale em qualquer ordenação.
- **Justificativa**: Uma divisória só entre datas diferentes agrupa visualmente o mesmo dia com uma variável de tema do
  Material, sem cabeçalhos de grupo nem HTML próprio (F4).
- **Alternativas consideradas**: Linhas de cabeçalho por data — mais código, e sem sentido nas ordenações que não são
  por data.

## Datas como dia do calendário (FR-004)

- **Decisão**: A API guarda a data como meia-noite UTC. O front envia o dia local escolhido como
  `YYYY-MM-DDT00:00:00.000Z` (`DateUtils.toApiDate`, `shared/date-utils/date-utils.ts:6-8`), lê de volta só a parte da
  data (`fromApiDate`, `:10-13`) e exibe com o pipe `date` em UTC (`transactions-table.html:4`). O mês do filtro vai
  como o primeiro dia (`dateRef=YYYY-MM-01`).
- **Justificativa**: O dia exibido e o salvo são o mesmo em qualquer fuso horário (caso-limite da spec); os testes de
  `date-utils.spec.ts` cobrem a ida e a volta e os formatos aceitos.
- **Alternativas consideradas**: Enviar a `Date` local serializada — no fuso do Brasil, o dia mudaria para o anterior
  ou o seguinte conforme a hora.

## Formulário em diálogo (FR-012 a FR-016)

- **Decisão**: `TransactionForm` é um `MatDialog` com Reactive Forms (`transaction-form.ts:50-66`): tipo num
  `mat-button-toggle-group`, valor como texto com `Validators.pattern(/^\d{1,13}([.,]\d{1,2})?$/)`, data num
  `mat-datepicker` com o `PtBrDateAdapter` (dia/mês/ano), categoria num `mat-select` e descrição com `maxlength` 255
  e contador (`transaction-form.html:35-39`). O diálogo **fecha com o `TransactionInput`** pronto (`submit()`) — o
  sinal vem do tipo, o número de `parseFloat` com a vírgula trocada por ponto, a descrição com trim ou `null` — e a
  página faz a chamada depois de fechar (`openForm()` em `transactions-page.ts`). A edição preenche o valor com
  `Math.abs(value).toFixed(2)` e vírgula (`transaction-form.ts:52`) e o tipo pelo sinal (`:51`). O botão "Salvar" fica
  fora do `<form>` e o submete pelo atributo `form` (`transaction-form.html:45`).
- **Justificativa**: O formulário fica de apresentação, sem HTTP (F1). Os campos e as mensagens são os do Material
  (F4). O mesmo padrão é usado pelo `CategoryForm`.
- **Alternativas consideradas**: Signal Forms, como nos filtros — o formulário de diálogo das duas features usa
  Reactive Forms; trocar não está em escopo numa spec retroativa.
- **Desvios conhecidos ligados a esta decisão** (fluxo de bugs do front): o diálogo fecha antes da resposta da API
  (FR-016; causa: `dialogRef.close(...)` em `submit()` de `transaction-form.ts`, antes de qualquer chamada); o padrão do valor
  aceita zero (FR-014, não bloqueia); o diálogo recebe uma cópia da lista de categorias no momento em que abre, vazia
  se elas ainda não chegaram ou se não há nenhuma (FR-019; `openForm()` em `transactions-page.ts`). Corrigidos: o limite de 13
  algarismos inteiros no valor, que evita a alteração pelo `parseFloat` (`front/bugs/valor-com-muitos-algarismos/`), e
  a data digitada lida como dia/mês/ano pelo `PtBrDateAdapter` (`front/bugs/data-digitada-como-mes-dia-ano/`).

## Filtros (FR-023 a FR-026)

- **Decisão**: `TransactionsFilters` usa Signal Forms sobre o `model` `filters` (`transactions-filters.ts:18-21`), com
  `debounce` de 300 ms só na descrição; o `blur` aplica na hora (comportamento do `debounce` do Signal Forms, coberto em
  `transactions-filters.spec.ts:77-91`). Os botões "Limpar …" esvaziam o filtro e chamam `stopPropagation()` para não
  abrir o `mat-select` em volta (`:23-27`). O mês usa o `MonthField` compartilhado com `clearable`
  (`transactions-filters.html:39`). O painel some com `@if (filtersVisible())` na página, mas o signal `filters`
  continua com os valores (`transactions-page.html:24-28`), então ocultar não limpa (FR-023).
- **Justificativa**: Um único signal de filtros alimenta a requisição; os componentes compartilhados evitam repetir o
  campo de mês e o menu de ordenação nas três páginas.
- **Alternativas consideradas**: Destruir e limpar os filtros ao ocultar — contrário à FR-023, decidida como requisito.
- **Desvios conhecidos ligados a esta decisão**: dia escolhido na visão de dias do "Mês" muda o texto do campo sem
  mudar o filtro (FR-024; `shared/month-field/month-field.html:17` só trata `monthSelected`; deduzido, não
  reproduzido). Corrigidos: os calendários, em português desde `front/bugs/calendarios-em-ingles/`
  (`PtBrDatepickerIntl`, FR-030), e a consulta repetida (ver acima).

## Ordenação (FR-027)

- **Decisão**: As oito opções ficam na página (`transactions-page.ts:62-71`) e o `SortMenu` compartilhado mostra a opção
  em uso marcada e no rótulo "Ordenar: …" (`shared/sort-menu/sort-menu.ts:24-28`). `categoryName` é um campo da API,
  não do front.
- **Justificativa**: Ordenar por nome de categoria precisa do nome atual, que só a API tem junto da lista.
- **Alternativas consideradas**: Cabeçalhos clicáveis com `mat-sort` — a página usa um menu, como as outras.

## Abertura filtrada pelo endereço (FR-028, FR-029)

- **Decisão**: A página lê `categoryId` e `month` uma vez, do `snapshot.queryParamMap` (`transactions-page.ts:49-51`).
  O `categoryId` é usado como veio; o `month` passa por `DateUtils.fromMonthKey`, que só aceita `YYYY-MM` com mês de 01
  a 12 e devolve `null` para o resto (`shared/date-utils/date-utils.ts:25-29`). Os filtros abrem à vista quando algum
  dos dois foi aceito (`transactions-page.ts:59`). Os filtros escolhidos depois não voltam ao endereço.
- **Justificativa**: Os endereços são públicos (F2) e vêm da página de categorias e do painel de relatórios. Um
  `snapshot` basta porque a rota não reaproveita a página com outro endereço sem recriá-la (o caso-limite da spec sobre
  o menu com a página aberta é comportamento atual aceito).
- **Comportamento com `categoryId` inválido** (aceito pelo usuário como comportamento atual): um valor malformado vai
  para a API, que responde `400` (o parâmetro é `format: uuid` no contrato, linhas 39-45 e 69-72); o recurso fica em
  erro e a tabela mostra "Não foi possível carregar as transações."; o `mat-select` não acha a opção e fica sem nome. Um
  UUID válido de categoria inexistente traz `200` com a lista vazia. Nenhum dos dois casos tem teste hoje; os dois viram
  os testes de caracterização C1 e C2.
- **Alternativas consideradas**: Validar o `categoryId` no front — não pedido; o usuário viu e não pediu mudança.

## Recarga depois de cada mutação (FR-017, FR-020, FR-021)

- **Decisão**: Depois de cada criação, edição, exclusão e restauração bem-sucedida, a página recarrega a lista com a
  mesma requisição (`this.transactions.reload()`, `transactions-page.ts:104` e `:121`), em vez de inserir, trocar ou
  remover a linha a partir do corpo da resposta. É uma consulta de transações por mutação, e as categorias não são
  buscadas de novo.
- **Justificativa**: Princípio III: a consulta a mais é necessária, porque são os filtros e a ordenação da API que
  decidem se a transação aparece e em que posição, inclusive na ordenação por `categoryName` e nos desempates por data e
  criação do back; o corpo da resposta não basta para reproduzir isso no front sem duplicar regras do back. Na recarga,
  as linhas atuais continuam à vista (FR-008).
- **Alternativas consideradas**: Atualizar a lista localmente com o corpo da resposta — evitaria a consulta, mas
  repetiria no front a filtragem e a ordenação da API, contra a FR-031, e poderia mostrar a transação fora da ordem ou
  dos filtros.

## Exclusão e "Desfazer" (FR-020 a FR-022)

- **Decisão**: "Excluir" chama a API sem confirmação; no sucesso, recarrega a lista e abre o snackbar "Transação
  excluída." com "Desfazer" por 5 s; a ação recria a transação com `create` usando a data, o valor, a categoria e a
  descrição da linha (`transactions-page.ts:101-115`).
- **Justificativa**: A API não tem "desexcluir" (comentário em `:105`); recriar é a forma de desfazer com o contrato que
  existe (princípio IV). A transação volta com outro `id`, `createdAt` e `updatedAt`.
- **Alternativas consideradas**: Confirmar antes de excluir — mais um clique em todo caso, quando o desfazer já protege
  contra o engano.
- **Desvio corrigido (princípio III; bloqueava a entrega, decisão do usuário, 2026-10-06)**: "Excluir" não tinha
  proteção contra clique repetido nem retorno visual; o segundo `DELETE` era inútil, a API o recusava com `404`, e o
  erro tomava o lugar do snack bar com "Desfazer". Corrigido em `front/bugs/clique-repetido-em-excluir/`, decidido
  também para a 002: o `delete()` ignora a linha já em exclusão, que mostra um spinner, e a restauração mostra uma
  barra de progresso.

## Mensagens (FR-017, FR-018, FR-022, SC-006)

- **Decisão**: Sucessos abrem o snackbar sem ação por 3 s (`transactions-page.ts:120`); falhas mostram
  `error.error?.detail` ou a mensagem padrão, com "Fechar", por 5 s (`:127-129`). Nada mais do erro (status, corpo
  bruto) é exibido. Os textos são exibidos por interpolação, nunca como HTML (FR-032): nos templates do projeto, e
  também nos snack bars, onde a mensagem vai como texto para o `MatSnackBar.open` (`:128`) e o `SimpleSnackBar` do
  Material a exibe por interpolação; essa garantia vem do Material e não tem teste próprio (o C13 cobre só os templates
  do projeto). As mensagens do back (`detail`) são sempre em português; a recusa automática de um pedido malformado
  (`400` do model binding, `ValidationProblemDetails`) vem sem `detail` e cai na mensagem padrão. A falha das categorias
  ao abrir o formulário aparece por 5 s com "Fechar" (`:87`). Toda requisição assinada trata o erro (`:102-125`), e as
  falhas dos `httpResource`s ficam no estado do recurso, então nada chega ao `ErrorHandler` global, que registraria a
  URL no console.
- **Justificativa**: O backend devolve `ProblemDetails` com `detail` em português para as regras de negócio, e uma
  mensagem genérica para erro inesperado (`GlobalExceptionHandler`), o que cumpre o princípio II sem tratamento extra.
- **Alternativas consideradas**: Mostrar o `title` ou o status — expõe detalhes técnicos (SC-006).
- **Desvio conhecido**: a mensagem de sucesso não avisa quando a transação salva fica fora dos filtros em uso (FR-017;
  `save()` em `:117-125` só recebe a mensagem fixa). Fluxo de bugs do front.

## Etiqueta de categoria (FR-005)

- **Decisão**: A diretiva compartilhada `MatChipColor` pinta o `mat-chip` com os tons 90 e 30 da cor da categoria,
  trocados no modo escuro por `light-dark()` (`shared/mat-chip-color/mat-chip-color.ts:3-29`); o teste confere o
  contraste de pelo menos 7:1 para várias cores (`mat-chip-color.spec.ts:37`). A linha sem categoria carregada fica sem
  etiqueta (`transactions-table.html:15-20`).
- **Justificativa**: Legibilidade em qualquer cor escolhida pelo usuário, só com tokens do chip do Material (F4).

## Rotas, menu e layout (FR-001, FR-002, FR-011)

- **Decisão**: `''` redireciona para `transactions` (`app.routes.ts:4-8`), que carrega `transactions.routes.ts` com
  `loadChildren` (`:10-12`), e este carrega a página com `loadComponent` (`transactions.routes.ts:4-7`). O link
  "Transações" é o segundo do menu (`app.ts:23-27`) e é marcado pelo `routerLinkActive` (`app.html:9-18`). A página é
  uma coluna flexível de altura máxima 100% e só o card da tabela rola (`transactions-page.scss:1-18`, com
  `mat-sidenav-content` limitado em `app.scss:21-25`).
- **Justificativa**: Rotas sob demanda são exigência da F1 e das restrições de performance do `front/AGENTS.md`; o
  layout só com CSS mínimo segue a F4.

## Fronteiras entre features (F1) — exceção aprovada

- **Fato**: A feature de transações importa o interior de `categories/`:
  - `transactions-page.ts:12` — `CategoriesService` de `categories/services/categories.service`;
  - `transactions-page.ts:13`, `transaction-form.ts:11`, `transactions-filters.ts:7` e `transactions-table.ts:9` — o
    tipo `Category` de `categories/types/category`;
  - nos testes, `buildCategory` de `categories/testing/category-fixture` (`transactions-page.spec.ts:13`,
    `transaction-form.spec.ts:5`, `transactions-filters.spec.ts:10`, `transactions-table.spec.ts:6`) e o tipo `Category`
    (`transactions-table.spec.ts:7`).
- **Contra o texto da F1**: "Uma feature não importa o interior de outra; o que serve a mais de uma vai para `shared/`".
  `services/`, `types/` e `testing/` são pastas internas da feature `categories/`. O resto da F1 é cumprido: a URL de
  categorias continua só no `CategoriesService`, e a página de transações não monta requisições.
- **Decisão (usuário, 2026-10-05)**: Exceção aprovada, registrada no Acompanhamento de complexidade do `plan.md`. Ela
  vale até um ciclo próprio de refatoração, pelo Spec Kit, mover o tipo `Category`, o `buildCategory` (o fixture) e a
  requisição da lista de categorias para `shared/`. Esse ciclo é trabalho futuro de outro fluxo, e não tarefa desta
  feature; ele toca também a [002-gestao-de-categorias](../002-gestao-de-categorias/plan.md) e os relatórios.
- **Justificativa**: A página precisa da lista de categorias para nomear e colorir as linhas e para alimentar os filtros
  e o formulário. Mover essas peças agora mexeria em três features num plano retroativo, que não muda código de
  produção (princípio V).
- **Alternativas consideradas**: Refatorar dentro desta feature — sai do escopo de uma spec retroativa e cruza
  features; emendar a F1 por `/speckit-constitution` para permitir importar `types/` e `testing/` de outra feature —
  afrouxaria a regra para todas as features.

## Abordagem de testes

- **Decisão**: Vitest + jsdom via `@angular/build:unit-test`, com um `*.spec.ts` ao lado de cada arquivo:
  - `transactions.service.spec.ts`: parâmetros da lista (vazios omitidos, ordenação, todos juntos) e
    `POST`/`PUT`/`DELETE` com `HttpTestingController`;
  - `transactions-page.spec.ts`: a página com `HttpTestingController`, `MatDialog` e `MatSnackBar` falsos e um
    `ActivatedRoute` com `queryParamMap`; o `afterEach(() => httpTesting.verify())` (`:77`) faz qualquer requisição a
    mais quebrar o teste, o que cobre, de forma implícita, a parte da SC-007 sobre não buscar as categorias de novo;
  - `transactions-table.spec.ts`: a tabela com `resourceFromSnapshots`, nos estados `resolved`, `loading` e `error`
    das duas listas;
  - `transactions-filters.spec.ts`: os filtros num host com os harnesses do Material;
  - `transaction-form.spec.ts`: o diálogo com `MatDialogRef` e `MAT_DIALOG_DATA` falsos;
  - em `shared/`: `date-utils`, `month-field`, `sort-menu`, `page-header`, `mat-chip-color`; e o menu em `app.spec.ts`.
- **Justificativa**: É o padrão do projeto (`front/.claude/skills/write-front-tests`), e o mesmo do painel de
  relatórios.

### Lacunas de teste verificadas

Comportamentos da spec que hoje não têm teste. As duas primeiras foram apontadas pelo usuário; as outras foram achadas
nesta leitura. O destino de cada uma foi decidido pelo usuário em 2026-10-05 (ver [Decisões do
usuário](#decisões-do-usuário-2026-10-05)).

| Comportamento                                                                | Requisito           | Situação verificada                                                                                                                                                                                                                   | Destino                                                                          |
| ---------------------------------------------------------------------------- | ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------- |
| Data digitada no campo "Data" do formulário                                  | FR-014              | Nenhum teste digita no campo `date`; `transaction-form.spec.ts` usa só a data padrão (`:92-103`) e a da edição (`:133-146`). O "Mês" do filtro é `readonly` (`month-field.html:5`), então não aceita texto e não precisa desse teste. | `/speckit-bug-fix` da data (desvio conhecido)                                    |
| `categoryId` inválido no endereço                                            | caso-limite, FR-028 | O único teste usa `categoryId: 'salario'` e responde `200` (`transactions-page.spec.ts:232-245`); nem o malformado (`400` → erro) nem o inexistente (`200 []` → vazio) são testados.                                                  | C1, C2                                                                           |
| Mensagens "Informe uma data válida" e "Escolha uma categoria"                | FR-014              | Só "Informe um valor válido" é testada (`transaction-form.spec.ts:84-90`); o checklist da spec já registra isso.                                                                                                                      | C6, C7 (sem texto digitado na data)                                              |
| Outros formatos do valor (`12.50` aceito; `8.600,00` e `-12` recusados)      | FR-014, caso-limite | Entre os aceitos, `12,50`, `8600` e `200` já são testados (`transaction-form.spec.ts:93`, `:107`, `:137`); falta o ponto decimal (`12.50`). Entre os recusados, só `12,345` é testado (`:85`).                                        | C8 (sem o zero e sem valores com mais de 13 algarismos, que ficam com o bug-fix) |
| Limite de 255 caracteres e contador da descrição                             | FR-014              | Sem teste.                                                                                                                                                                                                                            | C9                                                                               |
| Recarga que mantém as linhas à vista                                         | FR-008              | Nenhum teste usa o estado `reloading` na tabela nem confere as linhas durante o "Recarregar" na página.                                                                                                                               | C4                                                                               |
| Endereço raiz que leva a `/transactions`                                     | FR-002              | `app.spec.ts` usa rotas próprias (`:15-19`); o redirecionamento de `app.routes.ts:4-8` não é testado.                                                                                                                                 | C3                                                                               |
| Categoria e mês juntos no endereço                                           | FR-028              | Os testes cobrem cada um sozinho (`transactions-page.spec.ts:232-257`).                                                                                                                                                               | C12                                                                              |
| Mensagem padrão "Não foi possível excluir a transação." e falha ao restaurar | FR-021, FR-022      | Só a exclusão com `detail` da API é testada (`:409-415`).                                                                                                                                                                             | C10, C11                                                                         |
| Rolagem só da lista                                                          | FR-011              | Sem teste; o checklist da spec já registra isso.                                                                                                                                                                                      | C5                                                                               |
| Exibição como texto                                                          | SC-008, FR-032      | Sem teste; o checklist da spec já registra isso.                                                                                                                                                                                      | C13                                                                              |

### Dados de teste que a API real não produziria

- Identificadores que não são UUID: categorias `'mercado'`, `'salario'`, `'removida'` e `'transporte'` e transações
  `'t1'`…`'t10'` em todos os specs da feature (por exemplo, `transactions-page.spec.ts:25-47`). O contrato declara
  `format: uuid` para `id` e `categoryId`. O caso mais relevante é `create({ categoryId: 'salario' })`
  (`transactions-page.spec.ts:233-236`), que responde `200` a um `categoryId` que a API real recusaria com `400`.
- Erro `500` sem corpo (`transactions-page.spec.ts:357-361`), usado para testar a mensagem padrão. O backend real
  sempre devolve `ProblemDetails` com `detail` "Ocorreu um erro inesperado." num 500
  (`back/src/Denarius.WebAPI/Middleware/GlobalExceptionHandler.cs:25`); a mensagem padrão do front aparece, na prática,
  num erro de rede (status 0) ou num `400` de model binding, cujo `ValidationProblemDetails` não tem `detail`.
- Os demais dados (datas com `Z`, valores com duas casas, `description: null`, `ProblemDetails` com `detail`) são
  coerentes com o contrato.
- **Decisão (usuário, 2026-10-05)**: corrigir por tarefas desta feature (A1 a A3), sem mudar nenhuma asserção de
  comportamento.

### Divergências entre o que o front envia e o contrato

- `dateRef`: o front envia uma data sem hora (`dateRef=2026-09-01`, `transactions.service.ts:22`), e o contrato declara
  `format: date-time` (`transactions-api.yaml:27-33`). A API aceita (o quickstart do backend usa o mesmo formato, em
  `back/specs/002-transaction-management/quickstart.md:62`), então não há defeito observável; a divergência é de
  documentação. O `categories-api.yaml` tem a mesma declaração, e a 002 registra o mesmo ponto.
- **Decisão (usuário, 2026-10-05)**: desvio conhecido do back, só de documentação do contrato (`transactions-api.yaml` e
  `categories-api.yaml`), corrigido pelo fluxo de bugs no back, fora desta spec. Não bloqueia esta feature e não vira
  tarefa.
- Nenhuma outra: `Transaction` e `TransactionInput` (`types/transaction.ts`) espelham `TransactionOutput` e
  `Create/UpdateTransactionInput`; `Category` espelha `CategoryOutput`; `type` só envia `in`/`out`; `orderBy` só envia
  `date`, `description`, `categoryName` e `value`.

### Observação de desempenho fora do escopo do front

- A página usa só `id`, `name` e `color` das categorias, mas o `GET /api/categories` sem parâmetros também calcula
  `transactionCount`, `balance` e `canDelete` de cada categoria.
- O `GET /api/transactions` carrega todas as transações e só depois filtra e ordena em memória
  (`back/src/Denarius.Application/UseCases/Transactions/List/ListTransactionsUseCase.cs:11`), postura já documentada
  no plan do back (`back/specs/002-transaction-management/plan.md:26` e `:53`). Para o front, a filtragem e a
  ordenação acontecem na fonte dos dados, que é a API.

O front consome os endpoints que o contrato oferece (princípio IV), então nada disso é violação do front. **Decisão
(usuário, 2026-10-06)**: as duas observações, como a lista sem paginação, ficam aceitas até uma medição mostrar lentidão
(princípio III: primeiro meça, depois otimize); se ela mostrar, a decisão volta ao usuário, num ciclo do back.

### Exposição da busca na query string (fora do escopo do front)

O texto da busca por descrição vai na query string do `GET /api/transactions` porque o contrato é do back
(`transactions-api.yaml:19-26`), e poderia aparecer em logs de acesso. Hoje o back não registra as URLs
(`back/src/Denarius.WebAPI/appsettings.json`: `Microsoft.AspNetCore` em `Warning`); manter isso, e o que a hospedagem
registrar, cabe ao back e à implantação. O front não grava a busca no endereço da página.

## Desvios conhecidos e onde estão no código

Referência para os `/speckit-bug-assess` que vão corrigi-los; esta feature não tem trabalho para eles.

| Desvio (spec)                                                                                                | Onde está no código hoje                                                                                                                                                                                                                                                                                                                     |
| ------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Data digitada lida como mês/dia/ano ou ISO em UTC (FR-014; princípio II): corrigido                          | Corrigido em `front/bugs/data-digitada-como-mes-dia-ano/`: o `PtBrDateAdapter` (`transaction-form/pt-br-date-adapter.ts`) lê só dia/mês/ano, com o ano em quatro algarismos, e recusa o resto e as datas inexistentes                                                                                                                        |
| Zero aceito (FR-014; não bloqueia); mais de 13 algarismos inteiros: corrigido                                | O zero continua aceito pelo padrão `Validators.pattern(/^\d{1,13}([.,]\d{1,2})?$/)` em `transaction-form.ts`; o limite de 13 algarismos inteiros foi corrigido em `front/bugs/valor-com-muitos-algarismos/`                                                                                                                                  |
| Valores com 14 ou mais algarismos inteiros gravados diferentes do digitado (FR-014; princípio II): corrigido | Corrigido em `front/bugs/valor-com-muitos-algarismos/`: o padrão do valor recusa mais de 13 algarismos inteiros, e o `parseFloat` fica exato até 9.999.999.999.999,99                                                                                                                                                                        |
| "Excluir" sem proteção contra clique repetido nem retorno visual (FR-020; princípio III): corrigido          | Corrigido em `front/bugs/clique-repetido-em-excluir/`: `delete()` em `transactions-page.ts` ignora o id já em `deleting`; a linha mostra um spinner no "Excluir", e a restauração, uma `mat-progress-bar`                                                                                                                                    |
| API sem validação de faixa e de escala do valor (back; princípio II, não bloqueia)                           | `Transaction.ValidateValue` só recusa o zero (`back/src/Denarius.Domain/Entities/Transaction.cs:60-66`); coluna `numeric(18,2)` (`back/src/Denarius.Infrastructure/Persistence/Configurations/TransactionConfiguration.cs:23-25`): mais de duas casas são arredondadas sem aviso, e a partir de 10^16 o banco estoura e a API responde `500` |
| Diálogo fecha antes da resposta da API (FR-016)                                                              | `dialogRef.close(...)` em `submit()` de `transaction-form.ts`; chamada à API em `openForm()` de `transactions-page.ts`                                                                                                                                                                                                                       |
| Sem aviso da transação salva fora dos filtros (FR-017)                                                       | `save()` em `transactions-page.ts`                                                                                                                                                                                                                                                                                                           |
| "Nova transação" sem categorias (FR-019)                                                                     | `openForm()` só trata `categories.error()` em `transactions-page.ts`; o botão não tem `disabled` (`transactions-page.html:18`)                                                                                                                                                                                                               |
| Dia escolhido no calendário do "Mês" não muda o filtro (FR-024, não reproduzido)                             | `shared/month-field/month-field.html:17` só trata `monthSelected`                                                                                                                                                                                                                                                                            |
| Calendários anunciados em inglês (FR-030): corrigido                                                         | Corrigido em `front/bugs/calendarios-em-ingles/`: `PtBrDatepickerIntl` (`shared/datepicker-intl/`) provido no `MonthField` e no `TransactionForm`                                                                                                                                                                                            |
| Consulta repetida com a mesma ordenação, o mesmo mês ou o mesmo texto (FR-031): corrigido                    | Corrigido em `front/bugs/consulta-repetida-sem-mudanca/`: `shallowEqual` (`shared/shallow-equal/`) no `equal` dos signals `filters` e `sort` de `transactions-page.ts`                                                                                                                                                                       |
| `dateRef` declarado `date-time` no contrato do back (documentação)                                           | `back/specs/002-transaction-management/contracts/transactions-api.yaml:27-33` e o `dateRef` de `back/specs/001-category-management/contracts/categories-api.yaml`                                                                                                                                                                            |

## Decisões do usuário (2026-10-05)

As pendências levantadas na primeira versão deste plano foram decididas assim. Nenhuma muda o código de produção.

- **Data digitada (antiga P1)**: o teste nasce no `/speckit-bug-fix` da correção da data, como o teste que reproduz o
  bug. Não vira tarefa desta feature.
- **`categoryId` inválido (antiga P2) e demais lacunas (antiga P4)**: viram tarefas de teste de caracterização desta
  feature (C1 a C13 abaixo), abertas no `tasks.md` e feitas no `/speckit-implement` e no `/speckit-converge`. Ficam
  fora os testes de comportamento marcado como desvio conhecido, que nascem no bug-fix correspondente.
- **Dados de teste irreais (antiga P5)**: viram tarefas de ajuste desta feature (A1 a A3), sem mudar as asserções de
  comportamento.
- **Divergência do `dateRef` (antiga P6)**: desvio conhecido do back, só de documentação do contrato, corrigido pelo
  fluxo de bugs no back, fora desta spec.
- **Desvio da F1 (antiga P3)**: exceção aprovada até o ciclo de refatoração descrito em
  [Fronteiras entre features](#fronteiras-entre-features-f1--exceção-aprovada).
- **Governança (2026-10-06)**: as violações conhecidas sem exceção aprovada bloqueiam a entrega: a consulta repetida
  (III, FR-031), o clique repetido em "Excluir" (III, FR-020), os calendários em inglês (Idioma, FR-030), a data
  digitada (II, FR-014) e os valores com 14 ou mais algarismos (II, FR-014). A feature só é dada como entregue, no fim
  do `/speckit-converge`, depois desses cinco bug-fix, cada um concluído quando o `front/bugs/<slug>/test.md` registra
  `verified`, o teste de reprodução e a suíte completa passam e o `/speckit-converge` reavalia contra o código os FR/SC
  afetados. O implement dos testes pode rodar antes, com a suíte completa verde. Se a reprodução de um desvio deduzido
  do código não o confirmar, o requisito continua e a nota de desvio sai da spec. Os cinco bug-fix foram concluídos e
  reavaliados no `/speckit-converge` final (2026-10-08).
- **Integridade (2026-10-06)**: a data digitada e os valores com 14 ou mais algarismos ferem o princípio II; o zero não,
  porque a API o recusa com `400` (continua defeito, sem bloquear).
- **Validação do valor no back (2026-10-06)**: desvio conhecido do back, como o `dateRef`, corrigido pelo fluxo de bugs
  do back (validar faixa e escala com `400`); não bloqueia esta feature.
- **Exibição como texto (2026-10-06)**: a SC-008 passa a listar os snack bars, cuja garantia vem do Material, sem teste
  novo; o C13 continua só com os templates do projeto.
- **Acessibilidade (2026-10-06)**: o comportamento atual, apoiado no Material, fica nas Premissas da spec; as melhorias
  ficam para uma feature própria de acessibilidade, para a 002 e a 003, sem bloqueio.
- **Desempenho (2026-10-06)**: sem meta de tempo, pelo uso pessoal; a lista sem paginação e as observações de desempenho
  do back ficam aceitas até uma medição mostrar lentidão.
- **C5 (2026-10-06)**: teste estrutural e de estilo, sem simular a rolagem (detalhe na tabela abaixo).

### Testes de caracterização previstos (C)

São testes de um comportamento existente e aceito: passam desde o início, por natureza retroativa, e não acompanham
código de produção novo. Os novos testes já usam ids em formato UUID (A1). Duas regras valem para todos:

1. Se um C falhar, o implement para, não corrige código de produção e leva o caso ao usuário (em geral, por
   `/speckit-bug-assess`).
2. Cada C é conferido uma vez, sem commit, quebrando de propósito o comportamento coberto para vê-lo falhar; a
   conferência fica registrada no relatório do implement.

| Id  | Arquivo                                                                                  | O que o teste fixa                                                                                                                                                                                                                                                                                                                                                                                                                                  | Requisito           |
| --- | ---------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------- |
| C1  | `transactions-page.spec.ts`                                                              | `?categoryId=` malformado: a API responde `400` com `ValidationProblemDetails`; a tabela mostra "Não foi possível carregar as transações.", os filtros ficam à vista e o filtro "Categoria" fica sem nome escolhido                                                                                                                                                                                                                                 | caso-limite, FR-028 |
| C2  | `transactions-page.spec.ts`                                                              | `?categoryId=` com um UUID de categoria inexistente: a API responde `200 []`; a tabela mostra "Nenhuma transação encontrada."                                                                                                                                                                                                                                                                                                                       | caso-limite, FR-028 |
| C3  | `app.spec.ts` (ou um spec das rotas de `app.routes.ts`)                                  | O endereço raiz leva a `/transactions`                                                                                                                                                                                                                                                                                                                                                                                                              | FR-002              |
| C4  | `transactions-table.spec.ts` e/ou `transactions-page.spec.ts`                            | Numa recarga (`reloading`) com linhas, as linhas continuam à vista e o spinner não aparece; numa recarga da lista vazia ou depois de erro, o spinner ocupa o lugar da mensagem                                                                                                                                                                                                                                                                      | FR-008              |
| C5  | `transactions-page.spec.ts`                                                              | Teste estrutural e de estilo (decisão do usuário, 2026-10-06): o cabeçalho e os filtros ficam fora do `mat-card.table`; pelo `getComputedStyle`, o card tem `overflow: auto` e `flex: 1`, e o host tem `max-height: 100%`. A rolagem real e a altura limitada pelo `app.scss` continuam conferidas no passo manual do `quickstart.md`. Se os estilos do componente não chegarem ao jsdom, a dúvida volta ao usuário e a asserção não é enfraquecida | FR-011              |
| C6  | `transaction-form.spec.ts`                                                               | Com o campo "Data" esvaziado (input vazio → `null` → required), "Salvar" mostra "Informe uma data válida" e não fecha. Sem texto digitado, que é o desvio da data e fica com o bug-fix                                                                                                                                                                                                                                                              | FR-014              |
| C7  | `transaction-form.spec.ts`                                                               | Formulário isolado com `MAT_DIALOG_DATA` de `categories: []`: "Salvar" mostra "Escolha uma categoria" e o diálogo continua aberto. Afirma só a validação; a abertura do diálogo sem categorias é o desvio da FR-019 e não é testada aqui. O bug-fix da FR-019, que mexe na página, não deve inverter nem apagar o C7                                                                                                                                | FR-014              |
| C8  | `transaction-form.spec.ts`                                                               | `12.50` é aceito e salvo como 12,50; `8.600,00` e `-12` mostram "Informe um valor válido". Sem o zero e sem valores com mais de 13 algarismos inteiros, que ficam com o bug-fix                                                                                                                                                                                                                                                                     | FR-014, caso-limite |
| C9  | `transaction-form.spec.ts`                                                               | A descrição para de aceitar texto aos 255 caracteres, e o contador mostra os caracteres usados                                                                                                                                                                                                                                                                                                                                                      | FR-014              |
| C10 | `transactions-page.spec.ts`                                                              | Uma exclusão que falha sem `detail` (erro de rede, status 0) mostra "Não foi possível excluir a transação." e não recarrega                                                                                                                                                                                                                                                                                                                         | FR-022              |
| C11 | `transactions-page.spec.ts`                                                              | Uma restauração ("Desfazer") recusada mostra o `detail` da API ou, sem ele, "Não foi possível salvar a transação.", e não recarrega                                                                                                                                                                                                                                                                                                                 | FR-021              |
| C12 | `transactions-page.spec.ts`                                                              | `?categoryId=<uuid>&month=2026-09` juntos: a consulta leva os dois filtros, e os filtros abrem à vista, preenchidos                                                                                                                                                                                                                                                                                                                                 | FR-028              |
| C13 | `transactions-table.spec.ts`, `transactions-filters.spec.ts`, `transaction-form.spec.ts` | Uma descrição e um nome de categoria com marcação (`<b>teste</b>`) aparecem literalmente, como texto, nos templates do projeto: a célula "Descrição" e a etiqueta da categoria na tabela, e as opções e o valor escolhido do filtro "Categoria" e do campo "Categoria" do formulário. Os snack bars ficam fora, porque a garantia deles vem do Material                                                                                             | SC-008, FR-032      |

### Ajustes de dados de teste previstos (A)

As asserções de comportamento continuam iguais; só os dados passam a ser os que a API real produziria.

| Id  | Arquivos                                                                                                                                              | Ajuste                                                                                                                                                                                                                          |
| --- | ----------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A1  | `transactions.service.spec.ts`, `transactions-page.spec.ts`, `transactions-table.spec.ts`, `transactions-filters.spec.ts`, `transaction-form.spec.ts` | Ids de categoria e de transação em formato UUID, no lugar de `'mercado'`, `'salario'`, `'removida'`, `'transporte'` e `'t1'`…`'t10'`. O `buildCategory` de `categories/testing/` não muda; os specs passam UUIDs como overrides |
| A2  | `transactions-page.spec.ts` (`:232-245`)                                                                                                              | O teste do `categoryId` no endereço usa o UUID da categoria, de modo que a resposta `200` seja a que a API real daria                                                                                                           |
| A3  | `transactions-page.spec.ts` (`:357-361`)                                                                                                              | O teste da mensagem padrão do salvamento usa um erro de rede (status 0) no lugar do `500` sem corpo                                                                                                                             |
