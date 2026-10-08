# Avaliação do bug: `expectNone` com string não pega a recarga da lista

- **Slug**: expect-none-nao-pega-recarga
- **Projeto**: front
- **Criada em**: 2026-10-08
- **Origem**: texto colado (relato do orquestrador)
- **Veredito**: válido
- **Severidade**: medium

## Relato (resumido)

> Em vários specs do front, `httpTesting.expectNone(baseUrl)` recebe uma string. O `HttpClientTestingBackend` compara
> a string com `urlWithParams`, então a chamada nunca casa com o GET da lista, que sempre leva parâmetros
> (`?orderBy=…&asc=…`). Resultado: a asserção "não recarregou" passa mesmo quando a lista recarrega. Na prática, só o
> `httpTesting.verify()` do `afterEach` pega a recarga, e a falha aparece fora do teste e às vezes derruba os
> vizinhos. Os testes novos (C10, C11 da 003 e os bug-fix) já usam um predicado
> `(req) => req.method === 'GET' && req.url === baseUrl` ou o helper `expectNoListRequest`. A receita da skill
> `write-front-tests` ensina o padrão errado.

## Sintoma

Asserções do tipo "não recarregou" (`httpTesting.expectNone(baseUrl)`) nunca falham no corpo do teste, porque a
string não casa com nenhum GET da lista. Se o código passar a recarregar indevidamente, quem acusa é o `verify()` do
`afterEach`, com uma mensagem que não aponta a asserção, e a falha pode contaminar os testes seguintes do arquivo. O
esperado é que a própria asserção falhe, na linha dela.

## Reprodução

Ambiente: Windows 11, HEAD `06777bc`, `@angular/common` 21.2.20, Vitest 4. Experimentos temporários, todos desfeitos
(`git status` limpo ao fim).

**Quebra proposital** em `categories-page.ts`: o `error` do `save()` e o `error` do `delete()` passam a chamar
`this.reloadCategories()` (ou seja, a lista recarrega quando salvar ou excluir falha, o contrário do que os testes
dizem conferir).

1. **Arquivo inteiro** (`npx ng test --watch=false --include=src/app/categories/pages/categories-page/categories-page.spec.ts`):
   19 de 38 testes falham. O primeiro é "should show the API error detail and not reload when saving fails" (linha
   408, `expectNone(baseUrl)`), mas a falha vem do `afterEach` (linha 64):
   `Error: Expected no open requests, found 1: GET http://localhost:5276/api/categories?orderBy=name&asc=true`.
   Os outros 18 falham em cascata, inclusive testes que nada têm a ver com a quebra, com
   `Error: Cannot configure the test module when the test module has already been instantiated` (linha 48). O teste
   da linha 471 ("should show why the API refuses to delete a category with transactions"), que também é vazio, some
   no meio da cascata: a quebra no `delete()` não é atribuída a ele.
2. **Só os dois testes** (`--filter="not reload when saving fails|refuses to delete a category"`): os dois falham,
   o da linha 408 pelo `verify()` (stack em `categories-page.spec.ts:64:30`, não na linha 408) e o da linha 471
   pela cascata (`Cannot configure the test module`), não pela própria asserção.
3. **Com a asserção corrigida** (linhas 408 e 471 trocadas temporariamente por `expectNoListRequest()`), mesma
   quebra e mesmo filtro: os dois falham no próprio corpo, nas linhas 408 e 471, com
   `Expected zero matching requests for criteria "Match by function: ", found 1.` Sem cascata: o `match()` do
   `expectNone` retira a requisição casada da fila, e o `verify()` passa.
4. **Asserção corrigida, sem a quebra**: os dois passam (2 passed).

## Caminhos de código suspeitos

- `front/node_modules/@angular/common/fesm2022/http-testing.mjs:165-173` (`HttpClientTestingBackend._match`): com
  string, `this.open.filter(testReq => testReq.request.urlWithParams === match)`; com função, `match(testReq.request)`;
  com objeto `{ method, url }`, também compara `url` com `urlWithParams`. Só o predicado permite comparar `req.url`
  (sem a query string).
- `http-testing.mjs:200-206` (`expectNone`): usa o mesmo `match()` e só lança se algo casar. Uma string que nunca casa
  faz a asserção passar sempre.
- `http-testing.mjs:207-216` (`verify`): pega qualquer requisição aberta, por isso é o único que acusa a recarga.
- `front/src/app/categories/services/categories.service.ts:17-24` e
  `front/src/app/transactions/services/transactions.service.ts:17-26`: o `list()` põe `orderBy`/`asc` sempre que o
  sort tem direção, e as páginas sempre têm um sort padrão (o GET real é `…/categories?orderBy=name&asc=true`).
- `front/src/app/reports/services/reports.service.ts:35-38`: cada relatório leva `?month=…` quando há mês, e a página
  começa com o mês corrente (`reports-page.ts:49`); o spec confere `month=2026-10` (linha 85 do spec).
- `front/.claude/skills/write-front-tests/references/recipes.md:329`: a receita da página termina com
  `httpTesting.expectNone(baseUrl);` para "not reload", o padrão vazio.

### Ocorrências em `front/src/**/*.spec.ts`

Foram conferidas todas as chamadas de `expectNone`, `expectOne` e `match` com string (não há chamadas com objeto).

**Vazias (nunca podem falhar pela recarga): 4 linhas**

| Arquivo:linha | Teste | Por quê |
| --- | --- | --- |
| `categories/pages/categories-page/categories-page.spec.ts:408` | saving › should show the API error detail and not reload when saving fails | O POST já foi consumido; a string só casaria um GET sem parâmetros, que não existe |
| `categories/pages/categories-page/categories-page.spec.ts:471` | deleting › should show why the API refuses to delete a category with transactions | Idem, depois de um DELETE |
| `transactions/pages/transactions-page/transactions-page.spec.ts:541` | saving › should show the API error detail and not reload when saving fails | Idem ao 408 |
| `transactions/pages/transactions-page/transactions-page.spec.ts:638` | deleting › should show the API error and keep the list when the deletion fails | Idem ao 471 |

**Mal posicionada (a regressão faz o teste falhar, mas por timeout): 1 linha**

| Arquivo:linha | Teste | Por quê |
| --- | --- | --- |
| `reports/pages/reports-page/reports-page.spec.ts:164` | should keep the other blocks when one fails, and retry only that one | Laço sobre os 4 outros relatórios; cada GET leva `?month=2026-10`, então a string nunca casa. Além disso, o laço vem depois de `await fixture.whenStable()` (linha 161): com uma recarga indevida, os 4 GETs ficam pendentes, o `whenStable()` trava e o teste estoura os 5 s antes de chegar ao laço (`Test timed out in 5000ms`, seguido do `verify()` com os 4 GETs). A regressão é pega, mas a mensagem não aponta a causa. Trocar só a string por predicado, no mesmo lugar, não resolve |

**Parciais (pegam um POST indevido, mas não a recarga): 2 linhas**

| Arquivo:linha | Teste | Por quê |
| --- | --- | --- |
| `categories/pages/categories-page/categories-page.spec.ts:391` | saving › should do nothing when the form is cancelled | O POST de criação vai a `baseUrl` sem parâmetros e seria pego; uma recarga, não |
| `transactions/pages/transactions-page/transactions-page.spec.ts:524` | saving › should do nothing when the form is cancelled | Idem |

**Corretas (a requisição real não tem parâmetros):**

- `expectOne(baseUrl)` para POST: `categories-page.spec.ts:187, 362, 400, 445, 491`;
  `transactions-page.spec.ts:493, 533, 581, 624`; `categories.service.spec.ts:57`; `transactions.service.spec.ts:69`.
- `expectOne(\`${baseUrl}/${id}\`)` para PUT e DELETE: `categories-page.spec.ts:377, 415, 426, 459`;
  `transactions-page.spec.ts:510, 548, 562`; `categories.service.spec.ts:69, 81`; `transactions.service.spec.ts:81, 93`.
- `expectOne('data/default-colors.json')` / `expectOne(URL)`: `category-form.spec.ts:29`, `colors.service.spec.ts:24, 31`
  (GET de arquivo estático, sem parâmetros).
- `reload-when-idle.spec.ts:35, 40`: `expectOne(url)` e `expectNone(url)` com `url = '/items'` e um `httpResource`
  sem parâmetros. Corretas hoje; só ficariam vazias se o teste passasse a usar parâmetros.

Os demais `expectNone`/`expectOne`/`match` já usam predicado (por exemplo, os helpers `expectList`,
`expectNoListRequest`, `expectReport` e as linhas 601 e 630 de `transactions-page.spec.ts`).

## Hipótese de causa raiz

**Uso de string em `expectNone` para uma requisição que sempre leva query string.** Confiança alta.

O `HttpClientTestingBackend` compara a string com `request.urlWithParams` (confirmado no código, linhas 166-167).
Como o GET da lista sempre leva `orderBy`/`asc` (e o dos relatórios, `month`), a string `baseUrl` nunca casa e o
`expectNone` passa sempre. O padrão veio da receita da skill `write-front-tests` (commit `94b5e64`), que mostra o
`expectList()` com predicado, mas termina o exemplo com `expectNone(baseUrl)`. Os testes escritos depois disso para
"não recarregou" (C10, C11 da 003 e as correções de bugs) já usam o predicado ou o helper `expectNoListRequest`, o que
deixou os dois padrões convivendo no mesmo arquivo.

Não é defeito do código de produção: na HEAD atual, nenhuma das recargas indevidas acontece (a suíte passa e o
`verify()` não reclama). O defeito é de cobertura: as asserções não protegem o que dizem proteger, e uma regressão
apareceria fora do lugar e misturada com falhas em cascata.

## Correção proposta

**Preferida: predicado em todas as asserções vazias e parciais, pelos helpers de cada arquivo, e receita corrigida.**

1. `categories-page.spec.ts:408, 471` e `transactions-page.spec.ts:541, 638`: trocar `TestBed.tick();` +
   `httpTesting.expectNone(baseUrl);` por `expectNoListRequest();` (o helper já faz o `TestBed.tick()`).
2. `categories-page.spec.ts:391` e `transactions-page.spec.ts:524` (formulário cancelado): trocar
   `TestBed.tick();` + `httpTesting.expectNone(baseUrl);` por `expectNoListRequest();` **seguido** de
   `httpTesting.expectNone(baseUrl);`. O helper vem primeiro porque faz o `TestBed.tick()`; o `expectNone(baseUrl)`
   continua conferindo que nenhum POST saiu.
3. `reports-page.spec.ts:164`: mover a asserção para **antes** do `await fixture.whenStable()` (linha 161), logo
   depois do `expectReport('expensesByCategory').flush(...)`, e trocar o laço por uma asserção só:
   `TestBed.tick(); httpTesting.expectNone((req) => req.method === 'GET');` (opção A da P3). Ela retira da fila
   todos os GETs que casarem, então o `verify()` fica limpo. Validado nesta avaliação (experimento temporário,
   desfeito): com a quebra, o teste falha no corpo, na linha da asserção, em 323 ms
   (`Expected zero matching requests for criteria "Match by function: ", found 4.`); sem a quebra, passa.
4. `front/.claude/skills/write-front-tests/references/recipes.md`: na receita da página, acrescentar um
   `expectNoListRequest()` ao lado do `expectList()` (linhas 290-293) e trocar as linhas 328 e 329
   (`TestBed.tick();` + `httpTesting.expectNone(baseUrl);`) por `expectNoListRequest();`. No `SKILL.md`, citar o
   par `expectList`/`expectNoListRequest` na linha 41 e acrescentar uma linha nas armadilhas (perto da linha 79):
   "`expectOne`/`expectNone` com string comparam com a URL **com** a query string; para uma requisição com
   parâmetros, use um predicado com `req.url`, senão o `expectNone` nunca falha. Faça o `expectNone` antes de
   aguardar estabilidade: depois de um `await fixture.whenStable()`, uma recarga pendente trava o teste e a asserção
   nunca roda".

**Helper compartilhado ou um por arquivo?** Recomendo **manter um helper por arquivo**, como hoje:

- Não existe `src/testing/`; as pastas `testing/` das features só têm fixtures de dados. Um helper compartilhado
  seria a primeira abstração de teste transversal, com ganho de duas linhas por arquivo.
- O helper de cada página fica ao lado do `expectList()` dela, com o mesmo predicado e o mesmo `TestBed.tick()`, e
  a skill já ensina esse par por arquivo. Os dois juntos documentam, no próprio spec, qual requisição é "a lista".
- O princípio V pede seguir os padrões existentes e não criar abstrações para necessidades hipotéticas.
- Alternativa, se o dono quiser: uma função `getTo(url)` que devolve o predicado, num `src/app/shared/testing/`.
  Resolve o mesmo problema, mas mexe em todos os helpers existentes para ser coerente.

**Alternativas descartadas:**

- Objeto `{ method: 'GET', url: baseUrl }`: **não serve**, porque também compara com `urlWithParams` (linha 171).
- `expectNone(\`${baseUrl}?orderBy=name&asc=true\`)`: casa só com o sort padrão; uma recarga com outro sort ou com
  filtros passaria. Frágil.
- Confiar só no `verify()`: é o comportamento atual, com falha fora do lugar e em cascata.

**Arquivos que mudam:**

- `front/src/app/categories/pages/categories-page/categories-page.spec.ts`
- `front/src/app/transactions/pages/transactions-page/transactions-page.spec.ts`
- `front/src/app/reports/pages/reports-page/reports-page.spec.ts`
- `front/.claude/skills/write-front-tests/references/recipes.md`
- `front/.claude/skills/write-front-tests/SKILL.md` (linha 41 e uma linha nas armadilhas)

Nenhum código de produção muda.

## Testes de regressão e prova da correção

Os próprios testes corrigidos são os testes de regressão. Como a mudança é nos testes, o vermelho/verde se faz com
uma quebra temporária no código de produção, não commitada, por asserção:

| Asserção | Quebra temporária | Antes (string) | Depois (predicado) |
| --- | --- | --- | --- |
| `categories-page.spec.ts:408` | `error` do `save()` chama `reloadCategories()` | falha só no `verify()` (linha 64) | falha na linha 408 (reproduzido nesta avaliação) |
| `categories-page.spec.ts:471` | `error` do `delete()` chama `reloadCategories()` | falha em cascata ou no `verify()` | falha na linha 471 (reproduzido nesta avaliação) |
| `categories-page.spec.ts:391` | `openForm()` chama `reloadCategories()` quando `input` é vazio | falha só no `verify()` | falha na asserção nova |
| `transactions-page.spec.ts:541, 638, 524` | as mesmas três quebras em `transactions-page.ts` | falha só no `verify()` | falha na asserção |
| `reports-page.spec.ts:164` | o retry do bloco de despesas recarrega todos (`reports-page.html:17`: `(retry)="expensesByCategory.reload()"` vira `(retry)="reload()"`) | timeout de 5 s no `whenStable()` e depois o `verify()` com 4 GETs (reproduzido) | falha na asserção movida para antes do `whenStable()`, em 323 ms (reproduzido) |

Para cada linha: rodar o arquivo com `--filter` no nome do teste, conferir que a stack aponta a linha da asserção (e
não `afterEach`), desfazer a quebra e conferir que volta a passar. Registrar os resultados no `fix.md`. No fim,
`npx ng test --watch=false`, `npm run lint` e `npm run build`.

## Riscos e considerações

- **Testes que passam a falhar de verdade**: se alguma das asserções vazias estiver escondendo uma recarga real, ela
  passaria a falhar. Na HEAD atual isso não acontece: o `verify()` já pegaria a recarga, e a suíte passa. Risco baixo.
- **Mensagem do predicado**: o erro sai como `Match by function: ` (função anônima). A stack aponta a linha, o que
  basta; dar nome à função (ou passar o `description`, segundo argumento do `expectNone`) melhoraria a mensagem, mas
  é opcional e fora do mínimo.
- **Ordem em relação ao `whenStable()`**: faça o `expectNone` antes de aguardar estabilidade. Depois de um
  `await fixture.whenStable()`, uma recarga indevida deixa requisições pendentes, o `whenStable()` trava e a asserção
  nunca roda (é o caso da linha 164 dos relatórios). Nas páginas, as asserções corrigidas não têm `await` antes delas.
- **Cascata no arquivo**: quando o `verify()` falha no `afterEach`, o TestBed não é resetado e os testes seguintes
  falham com `Cannot configure the test module`. A correção não muda isso, mas tira essas asserções do caminho que leva
  a ela.
- **Skill**: a correção da receita muda o que os agentes geram nos próximos specs. Os outros exemplos com string da
  receita (linhas 45, 68 e 326) são POST e JSON estático, corretos.
- **Fora deste bug**: o registro na skill do import por efeito do bug `teste-de-rota-raiz-intermitente` continua
  numa mudança separada.
- **Escopo**: só specs e a skill do front. Nenhum código de produção, contrato da API, dado ou segurança é afetado.
  Specs de features (`front/specs/*`) não são tocados.

## Decisões do usuário (portão do diagnóstico, 2026-10-08)

- **P1 = (A)**: um helper por arquivo; o spec dos relatórios usa o predicado inline.
- **P2 = (A)**: nos testes de formulário cancelado, `expectNoListRequest()` primeiro e `expectNone(baseUrl)` depois.
- **P3 = (A)**: no `reports-page.spec.ts`, `TestBed.tick(); httpTesting.expectNone((req) => req.method === 'GET');`
  antes do `await fixture.whenStable()`.
- **P4 = (A)**: corrigir a receita e acrescentar a armadilha no `SKILL.md` (com o par `expectList`/`expectNoListRequest`
  na linha 41).

## Perguntas em aberto

- **P1: Helper compartilhado ou um por arquivo?** Respondida no portão do diagnóstico (2026-10-08): (A).
  - (A) Um helper por arquivo, como hoje: `expectNoListRequest` já existe nas duas páginas; o spec de relatórios
    fica com o predicado inline (P3). Mudança mínima e coerente com a skill.
  - (B) Um utilitário compartilhado (`getTo(url)`) em `src/app/shared/testing/`, usado por todos os helpers. Uma
    fonte única do predicado, mas cria uma pasta e uma abstração novas e mexe em helpers que já funcionam.
  - **Recomendação**: (A).
- **P2: Nos testes de formulário cancelado (391, 524), manter a checagem do POST?** Respondida no portão do diagnóstico (2026-10-08): (A).
  - (A) `expectNoListRequest()` primeiro (ele faz o tick) e `expectNone(baseUrl)` (POST) depois. Cobre as duas coisas
    que "do nothing" promete.
  - (B) Trocar só por `expectNoListRequest()`. Mais curto, mas perde a checagem do POST (que o `verify()` ainda pegaria,
    fora do lugar).
  - **Recomendação**: (A).
- **P3: No `reports-page.spec.ts:164`, como escrever a asserção, já movida para antes do `await fixture.whenStable()`?** Respondida no portão do diagnóstico (2026-10-08): (A).
  - (A) `TestBed.tick(); httpTesting.expectNone((req) => req.method === 'GET');`, numa asserção só. Retira os 4 GETs
    da fila, então o `verify()` fica limpo e a falha aparece uma vez só, na linha certa. Validada nesta avaliação.
  - (B) `TestBed.tick()` e um helper `expectNoReport(report)`, ao lado do `expectReport`, num laço. Simétrico ao
    `expectList`/`expectNoListRequest`, mas o laço para no primeiro GET e deixa os outros 3 para o `verify()`, que
    falha de novo no `afterEach`.
  - **Recomendação**: (A). Uma asserção mais simples, sem helper novo, e sem falha duplicada no `afterEach`.
- **P4: Acrescentar a armadilha no `SKILL.md` além de corrigir a receita?** Respondida no portão do diagnóstico (2026-10-08): (A).
  - (A) Sim, uma linha nas armadilhas. Explica o porquê e evita a volta do padrão.
  - (B) Só a receita.
  - **Recomendação**: (A).
