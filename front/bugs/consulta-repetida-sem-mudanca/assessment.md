# Avaliação do bug: a consulta à API se repete quando nenhum critério mudou

- **Slug**: consulta-repetida-sem-mudanca
- **Projeto**: front
- **Criada em**: 2026-10-07
- **Origem**: texto colado (relato do orquestrador, a partir do `/speckit-converge` das features 002 e 003)
- **Veredito**: válido
- **Severidade**: medium

## Relato (resumido)

> Nas telas de Categorias, Transações e Relatórios, escolher de novo a mesma ordenação, o mesmo mês ou o mesmo texto
> de filtro dispara uma nova consulta à API, embora nada tenha mudado. As specs exigem que a consulta só se repita
> quando um critério muda. Evidência preliminar do converge: o `shared/sort-menu` sempre grava um objeto novo; o
> `month-field` também; o filtro "Nome" reescreve o `model`; por isso o `httpResource` refaz a consulta mesmo sem
> mudança. Por decisão do usuário, a avaliação cobre também os relatórios (`front/src/app/reports/`).

## Sintoma

Uma ação que não muda nenhum critério da consulta (a ordenação em uso escolhida de novo, o mesmo mês escolhido de
novo, o texto da busca redigitado igual ao aplicado, ou a saída do campo de busca depois da pausa nesse caso) faz uma
nova requisição GET idêntica à anterior, e a lista troca as linhas pelo indicador de carregamento. Nos relatórios,
escolher o mesmo mês refaz as cinco consultas. O esperado é nenhuma consulta e a tela como está.

## Requisitos violados

- `front/specs/002-gestao-de-categorias/spec.md`: FR-023 (488-493), SC-006 (519-521) e Casos-limite (352-359). O
  FR-023 pede uma correção só, pelo fluxo de bugs, "começando por um teste que reproduza o caso do 'Nome'", e que valha
  também para transações e relatórios.
- `front/specs/003-gestao-de-transacoes/spec.md`: FR-031 (551-558), SC-007 (592-596) e Casos-limite (338-346).
- `front/specs/001-reports-dashboard/spec.md`: não tem requisito equivalente explícito. O FR-002 (192-193) e a SC-002
  (231) só pedem que escolher um mês atualize os cinco blocos. Vale o princípio III do `AGENTS.md` da raiz ("Nada de
  requisições, consultas ou cálculos repetidos sem necessidade"), e o próprio FR-023 da 002 estende a correção aos
  relatórios.

## Reprodução

Reproduzido em 2026-10-07, no `main` (`ef291ee`), com um spec temporário (`src/app/zz-repro-temp.spec.ts`, rodado com
`npx ng test --watch=false --include=...` e apagado em seguida; a árvore de trabalho ficou limpa). O spec criava as
páginas com `HttpTestingController`, respondia a cada GET e registrava as requisições geradas por cada ação:

| Ação | Requisições geradas |
| --- | --- |
| Categorias: escolher de novo "Nome (A–Z)" (em uso) | 1: `orderBy=name&asc=true` |
| Categorias: escolher 09/2026 no "Mês" | 1: `dateRef=2026-09-01&…` (esperado) |
| Categorias: escolher 09/2026 de novo | 1: `dateRef=2026-09-01&…` (idêntica) |
| Categorias: digitar "mer" no "Nome" e esperar 300 ms | 1: `name=mer&…` (esperado) |
| Categorias: redigitar "mer" e esperar 300 ms | 1: `name=mer&…` (idêntica) |
| Categorias: sair do campo logo depois | 1: `name=mer&…` (idêntica, de novo) |
| Categorias: digitar "merc", esperar 300 ms e sair do campo | 1 na pausa (esperado); 0 na saída |
| Transações: escolher de novo "Mais recentes primeiro" (em uso) | 1: `orderBy=date&asc=false` |
| Relatórios: escolher no "Mês" o mês já mostrado | 5: `summary`, `expensesByCategory`, `incomeVsExpense`, `cumulativeExpenses`, `transactions` |
| Relatórios: "Mês anterior" | 5 (esperado; nenhuma em dobro) |

Isso confirma também os desvios que as specs 002 e 003 marcavam como "deduzidos do código e ainda não reproduzidos"
(o "Nome" e a "Descrição", inclusive a consulta a mais ao sair do campo, que só acontece quando o texto aplicado não
mudou). A "Descrição" de transações usa o mesmo código do "Nome" (`debounce(path.description!, 300)`) e não foi
rodada à parte.

Passos manuais equivalentes, com a aba Rede do navegador aberta:

1. Em `/categories` (ou `/transactions`), abrir o menu de ordenação e clicar na opção já marcada: sai um GET igual ao
   anterior e a tabela mostra o indicador de carregamento.
2. Exibir os filtros, escolher um mês e depois o mesmo mês: o segundo GET é igual ao primeiro.
3. Digitar um texto no "Nome", esperar a lista, apagar e redigitar o mesmo texto (ou digitar e apagar uma letra em
   menos de 300 ms): sai um GET igual; ao sair do campo, sai mais um.
4. Em `/reports`, escolher no "Mês" o mês já mostrado: saem cinco GETs e os cinco blocos voltam ao carregamento.

## Caminhos de código suspeitos

Origem dos valores novos com o mesmo conteúdo:

- `front/src/app/shared/sort-menu/sort-menu.ts:30-32`: `select()` grava `{ active, direction }`, um objeto novo, mesmo
  quando a opção é a que está em uso (`sort-menu.html:11`). O `model` propaga para o `signal` da página pelo
  `[(sort)]` (`categories-page.html:17`, `transactions-page.html:17`).
- `front/src/app/shared/month-field/month-field.html:17`: `(monthSelected)="value.set($event)"` grava um `Date` novo;
  dois `Date` do mesmo mês são objetos diferentes. Nos filtros, o campo é um controle do signal form
  (`categories-filters.html:31`, `transactions-filters.html:39`), que reescreve o objeto de filtros; nos relatórios, o
  `[(value)]` grava direto no `month` da página (`reports-page.html:11`).
- `front/src/app/categories/components/categories-filters/categories-filters.ts:19` e
  `front/src/app/transactions/components/transactions-filters/transactions-filters.ts:21`: o `form(this.filters, …)`
  com `debounce(…, 300)`. No Angular Signal Forms (21.2.20):
  - `node_modules/@angular/forms/fesm2022/_validation_errors-chunk.mjs:872-893` (`deepSignal`/`valueForWrite`):
    gravar um campo sempre cria um objeto raiz novo (`{ ...sourceValue, [prop]: newPropValue }`), mesmo com o valor
    igual;
  - mesmo arquivo, 1333-1380 (`controlValueSignal`, `debounceSync`, `sync`): cada tecla agenda um `sync()` que, depois
    da pausa, grava o valor do controle no modelo, sem comparar com o aplicado;
  - mesmo arquivo, 1214-1220 (`pendingSync`) e 1305-1309/1356-1362 (`markAsTouched`/`flushSync`): o `pendingSync` só
    é zerado quando o valor do campo muda. Se o texto aplicado ficou igual, ao sair do campo o `flushSync()` grava de
    novo e gera mais um objeto raiz. É a consulta a mais "ao sair do campo".

Por que um objeto novo vira consulta nova:

- `front/src/app/categories/pages/categories-page/categories-page.ts:40,43,53-55`,
  `front/src/app/transactions/pages/transactions-page/transactions-page.ts:53-61,73-75` e
  `front/src/app/reports/pages/reports-page/reports-page.ts:45-59`: os `signal` de filtros, ordenação e mês usam a
  igualdade padrão (`Object.is`), então qualquer objeto ou `Date` novo notifica, e o `httpResource` recalcula a
  requisição.
- `front/src/app/categories/services/categories.service.ts:17-24`, `transactions.service.ts:17-25` e
  `reports.service.ts:35-38`: cada chamada monta um `HttpParams` novo.
- `node_modules/@angular/common/fesm2022/http.mjs:390-424`: o `httpResource` envolve a função de requisição em
  `normalizeRequest`, que cria um `HttpRequest` novo a cada execução.
- `node_modules/@angular/core/fesm2022/_resource-chunk.mjs:187`: o `extRequest` do `ResourceImpl` é um `linkedSignal`
  sobre essa requisição, sem igualdade de conteúdo. Requisição nova (por referência) é recarga nova.

Cálculo repetido nos relatórios, além da consulta: o `month()` da página também chega por `[month]` ao
`cumulative-comparison-chart.ts:30,41` (o `computed` `monthNames`, que devolve um objeto novo e recalcula os dados do
gráfico, linhas 62, 69 e 115) e ao `transactions-list.ts:20,34` (o `monthFilter`). Um `Date` novo do mesmo mês
recalcula esses `computed` e redesenha o gráfico, o que também é cálculo repetido (princípio III). Não foi medido.

Fora de causa, com uma ressalva: os botões "Limpar" só aparecem com o filtro preenchido. **Não reproduzido**: pela
leitura do Material, o `mat-select` dos filtros não reemite ao escolher a opção já marcada; isso não foi testado nesta
avaliação. Com a correção (A), o caso fica coberto mesmo que reemita; com (C), também, porque a comparação é feita
no `signal` da página.

## Hipótese de causa raiz

Confiança: **alta** (reproduzida e confirmada no código do Angular).

O `httpResource` refaz a consulta sempre que a função de requisição volta a rodar, porque cada execução cria um
`HttpRequest` novo e o `ResourceImpl` não compara conteúdo. A função volta a rodar sempre que um `signal` lido nela
notifica, e os `signal` de filtros, ordenação e mês notificam a cada gravação de um objeto ou `Date` novo, mesmo com
conteúdo igual. Três caminhos gravam valores novos sem mudança: o menu de ordenação, o campo de mês e o signal form
dos filtros (que reescreve o objeto raiz depois da pausa e de novo ao sair do campo). Não falta comparação num ponto
específico da interface: falta uma comparação, em qualquer ponto, entre a consulta nova e a anterior.

**É bug, e não comportamento novo.** As specs 002 e 003 já exigem o comportamento (FR-023, FR-031, SC-006, SC-007) e
registram o desvio como conhecido, encaminhado ao fluxo de bugs. Nos relatórios, o princípio III da constituição já
proíbe a consulta repetida.

## Correção proposta

**Preferida: deduplicar a requisição num helper de `shared/`, comparando o que vai para a API.**

Criar em `front/src/app/shared/` um helper pequeno, por exemplo `distinct-request/distinct-request.ts`, que recebe a
função de requisição e devolve um `computed` com igualdade por conteúdo da consulta (`url` e `params.toString()`):

```ts
/** A GET request as the services build it: only the URL and the query parameters, compared by content. */
export interface QueryRequest {
	url: string;
	params: HttpParams;
}

export function distinctRequest(request: () => QueryRequest): Signal<QueryRequest> {
	return computed(request, {
		equal: (a, b) => a.url === b.url && a.params.toString() === b.params.toString(),
	});
}
```

O parâmetro tem o formato exato `{ url: string; params: HttpParams }`, sem genérico: uma requisição com cabeçalhos,
corpo ou método não cabe no tipo e não compila, em vez de ser comparada pela metade. O comentário registra o limite,
e o `distinct-request.spec.ts` cobre a comparação por URL e por parâmetros.

As páginas passam a envolver a função que já entregam ao `httpResource`, sem outra mudança:

- `categories-page.ts:53-55`: `httpResource<Category[]>(distinctRequest(() => this.categoriesService.list(…)))`;
- `transactions-page.ts:73-75`: o mesmo na lista de transações (a de categorias, linha 77, não depende de nenhum
  `signal` e fica como está);
- `reports-page.ts:47-59`: o mesmo nos cinco relatórios.

Como funciona: o `linkedSignal` do `httpResource` passa a depender do `computed`; quando a consulta nova é igual à
anterior, o `computed` não muda de versão e o `httpResource` não recalcula nem recarrega. A premissa foi testada no
mesmo spec temporário, com um componente mínimo: com o `computed`, gravar filtros de mesmo conteúdo não gerou
requisição (sem ele, gerou); mudar o filtro gerou uma; o `reload()` continuou recarregando.

O que pesa a favor de (A):

- a regra comparada é a do requisito, a própria consulta: a consulta só se repete quando um parâmetro dela muda,
  qualquer que seja o caminho que gravou o valor (ordenação, mês, signal form com o `flushSync` ao sair do campo, ou um
  caminho futuro);
- segue os padrões do projeto: os services continuam montando URL e parâmetros (F1), as páginas continuam donas do
  `httpResource`, e o helper vive em `shared/` porque serve a três features;
- não muda o comportamento visível de nenhum caso em que a consulta muda.

O que (A) não faz: o `month()` dos relatórios continua mudando de referência, então o gráfico de comparação
acumulada e a lista de transações recalculam os seus `computed` (ver Riscos). E (A) contraria a redação do FR-031 da
003 sobre o lugar da correção (ver P1).

**Alternativas:**

- (B) Corrigir cada origem: o `SortMenu.select()` ignora a opção em uso; o `MonthField` ignora um mês igual ao atual
  (comparando ano e mês); e, como o signal form sempre grava um objeto novo, as páginas de categorias e de transações
  ainda precisam de `signal<…Filters>(…, { equal })` com uma comparação rasa que trate `Date`. São quatro ou cinco
  pontos. Vantagens: os componentes compartilhados param de emitir valores sem mudança, o `month()` dos relatórios
  deixa de mudar de referência (o gráfico e a lista não recalculam) e a correção fica onde o FR-031 da 003 diz.
  Desvantagem: cada caminho novo de gravação precisa da sua própria guarda.
- (C) Igualdade nos `signal` das páginas: um comparador em `shared/` (igualdade rasa de objetos que compara `Date`
  pelo valor) e três `equal` (`filters` e `sort` em categorias e transações, `month` nos relatórios). O tamanho é
  parecido com o de (A): um arquivo em `shared/` e mudanças de uma linha nas páginas. Cobre todos os casos de hoje,
  inclusive o `flushSync` e o `mat-select`, porque a comparação fica no `signal` que recebe a gravação. Nos relatórios,
  o `month()` deixa de mudar de referência, então também evita o cálculo repetido no gráfico e na lista. Diferenças
  para (A): a igualdade é pelo estado, e não pela consulta, o que não muda nada hoje, porque cada campo de filtro e de
  ordenação vira parâmetro, com o mesmo grão (em categorias e transações o `dateRef` usa o dia inteiro, `toDateKey`,
  então outro dia do mês gera outra consulta tanto em (A) quanto em (C); nos relatórios, o parâmetro é o mês,
  `toMonthKey`, e em (C) um outro dia do mês ainda seria um `Date` diferente, com consulta igual repetida, caso que o
  `MonthField` não produz hoje porque grava o primeiro dia do mês). (C) também não segue a redação do FR-031 à risca,
  porque a correção fica nas páginas, e não nos componentes compartilhados.

**Arquivos que devem mudar (preferida):**

- novo: `front/src/app/shared/distinct-request/distinct-request.ts` e `distinct-request.spec.ts`;
- `front/src/app/categories/pages/categories-page/categories-page.ts` e `.spec.ts`;
- `front/src/app/transactions/pages/transactions-page/transactions-page.ts` e `.spec.ts`;
- `front/src/app/reports/pages/reports-page/reports-page.ts` e `.spec.ts`.

Nenhuma mudança no back, nos services, nos tipos nem no contrato da API.

**Testes de regressão previstos** (escritos antes da correção; todos falham hoje, como mostra a reprodução):

Começando pelo "Nome", como pede o FR-023:

1. `categories-page.spec.ts`, descrito em `filters`:
   - "should not reload when the name is typed again as the text applied": aplicar "mer" (setValue no `MatInputHarness`
     e pausa de 300 ms), responder, redigitar "mer", esperar a pausa e verificar `expectNoListRequest()`;
   - "should not reload when leaving the name field after the text applied did not change": na sequência acima, sair
     do campo pelo DOM (ver o cuidado abaixo) e verificar `expectNoListRequest()`;
   - "should not reload when the month in use is picked again": emitir o mesmo mês no `monthSelected` do
     `MatDatepicker` duas vezes, responder a primeira e verificar `expectNoListRequest()` na segunda.
2. `categories-page.spec.ts`: "should not reload when the sort in use is chosen again" (`sortBy('Nome (A–Z)')` e
   `expectNoListRequest()`).
3. `transactions-page.spec.ts`: os mesmos quatro casos, com a "Descrição", o mês e "Mais recentes primeiro".
4. `reports-page.spec.ts`: "should not reload any block when the month shown is picked again" (`expectNone` para os
   cinco relatórios).
5. `distinct-request.spec.ts`: o helper isolado, num componente de teste: não recarrega com uma consulta igual;
   recarrega uma vez quando um parâmetro muda; o `reload()` continua recarregando.

Os testes existentes "should reload sorted by the option chosen in the sort menu" e "should reload with the filters
chosen" ficam como estão e garantem que a mudança real continua gerando uma consulta.

Se a escolha for (B) ou (C), os testes 1 a 4 continuam iguais, porque verificam o comportamento nas páginas. O 5 dá
lugar aos testes do que mudar: guardas no `sort-menu.spec.ts` e no `month-field.spec.ts` (B), ou o comparador em
`shared/` (C). Em (B) ou (C), um teste no `reports-page.spec.ts` pode verificar também que o `month` passado ao gráfico
mantém a referência quando o mesmo mês é escolhido.

Cuidado ao escrever os testes (ver a skill `write-front-tests` e a memória do projeto): `fixture.whenStable()` e os
harnesses esperam o `httpResource` pendente, e antes da correção a ação sob teste deixa uma requisição pendente. Para
que o vermelho seja a asserção, e não um estouro de tempo:

- **nenhuma ação que possa disparar requisição passa por um harness**. O `MatInputHarness.blur()` chama
  `forceStabilize()` e trava; o mesmo vale para o `setValue()` quando há requisição pendente;
- **sair do campo é feito no DOM**: `input.dispatchEvent(new Event('blur'))` (o que a reprodução usou; o `FormField`
  escuta `blur`) ou o `.blur()` nativo do elemento, seguido de `TestBed.tick()` e da verificação. Vale para o "Nome"
  (categorias) e para a "Descrição" (transações);
- o `setValue()` do harness só é usado sem requisição pendente (antes da pausa de 300 ms, com a resposta anterior já
  entregue), ou é trocado por gravar `value` no elemento e disparar `input` no DOM;
- a verificação (`TestBed.tick()` + `expectNone`/`expectNoListRequest`) vem logo depois da ação, sem `whenStable()`
  entre elas. O mesmo vale para o menu de ordenação e para o `monthSelected` do datepicker.

Verificação final: `npx ng test --watch=false`, `npm run lint` e `npm run build` em `front/`.

## Riscos e considerações

- **Comparação pela consulta (A)**: o helper compara só `url` e `params`, que é tudo o que os services montam hoje (GET
  sem corpo nem cabeçalhos). O parâmetro tem o tipo exato `{ url: string; params: HttpParams }`, com o limite
  registrado em comentário e no teste; uma consulta com cabeçalhos ou corpo não compila e obriga o helper a crescer
  junto. O `HttpParams.toString()` é determinístico porque os services gravam os parâmetros
  sempre na mesma ordem.
- **Recarregar continua funcionando**: "Recarregar", "Tentar de novo" e as recargas depois de salvar ou excluir usam
  `reload()`, que não passa pela comparação (confirmado na reprodução).
- **Mês anterior/próximo nos relatórios**: o `reload()` chamado logo depois do `month.set()` (`reports-page.ts:69-77`)
  hoje não faz nada, porque o recurso já está carregando; a reprodução mostrou cinco requisições, nenhuma em dobro. A
  correção não muda isso. A chamada redundante fica fora deste bug.
- **Campo de mês com dia de outro mês**: o desvio do "Mês" que aplica no campo o mês de um dia escolhido sem mudar o
  filtro (FR-023/FR-024 da 002 e 003) é outro bug, com correção própria no `month-field`, fora deste. Nenhuma das
  opções o resolve. Em categorias e transações o `dateRef` usa o dia inteiro (`toDateKey`), então um `Date` em outro
  dia do mês gera outra consulta em (A), (B) ou (C). Só nos relatórios, cujo parâmetro é o mês (`toMonthKey`), (A)
  trataria esse `Date` como a mesma consulta.
- **Cálculo repetido nos relatórios**: com (A), escolher o mesmo mês não refaz as consultas, mas o `month()` da página
  ainda muda de referência e chega a `cumulative-comparison-chart.ts:30,41` (o `monthNames` recalcula e o gráfico
  redesenha) e a `transactions-list.ts:20,34` (o `monthFilter`). Pelo princípio III, é cálculo repetido; o custo não
  foi medido. Com (A), isso fica explicitamente **fora deste bug**, que trata da consulta à API, e pode virar um bug
  próprio. (B) e (C) resolvem também esse ponto, porque o `month` deixa de mudar.
- **Componentes compartilhados continuam emitindo**: com (A) e com (C), o `SortMenu` e o `MonthField` ainda gravam
  valores novos de mesmo conteúdo. Atrás do helper ou do `equal` é inofensivo, mas quem usar esses componentes sem
  essa proteção volta a ter o problema. Só (B) resolve isso, ao custo de mais pontos de mudança.
- **Escopo**: só o front. Nenhuma mudança no back, no contrato da API, em dados ou em segurança. O efeito é menos
  requisições, coerente com o princípio III.
- **Specs**: depois da correção, as notas de "Desvio conhecido" do FR-023, SC-006 e Casos-limite da 002, e do FR-031,
  SC-007 e Casos-limite da 003, deixam de valer. Elas mesmas dizem que saem da spec quando o desvio for corrigido.

## Perguntas em aberto

- **P1 — Onde corrigir**: [NEEDS CLARIFICATION: comparar a consulta, o estado das páginas ou cada origem?]
  Atenção: o FR-031 da 003 (`front/specs/003-gestao-de-transacoes/spec.md:557-558`, e também a sessão de esclarecimentos, linhas 70-72) diz que a correção fica "no menu de
  ordenação, no campo 'Mês' compartilhados e na busca", o que descreve (B). O FR-023 da 002 só pede uma correção
  única, pelo fluxo de bugs, sem dizer onde.
  - (A) Helper `distinctRequest` em `shared/`, envolvendo os 7 `httpResource` das três páginas. Compara a própria
    consulta e por isso cobre qualquer caminho de gravação, de hoje ou futuro. Consequências: o `month()` dos
    relatórios continua mudando de referência, e o recálculo do gráfico e da lista fica fora deste bug; os componentes
    compartilhados continuam emitindo; o bug-fix ajusta as frases do FR-031 e do esclarecimento (linhas 70-72) sobre o lugar da correção, junto com as notas
    da P2, e registra a decisão no `fix.md`.
  - (B) Corrigir cada origem: guardas no `SortMenu` e no `MonthField` e `equal` nos `signal` de filtros das páginas,
    necessário por causa do signal form. Segue o FR-031 à risca, os componentes compartilhados param de emitir, e o
    gráfico e a lista dos relatórios não recalculam. Consequências: quatro ou cinco pontos de mudança, e cada caminho
    novo de gravação precisa da sua guarda.
  - (C) Um comparador em `shared/` (igualdade rasa, com `Date` comparado pelo valor) e três `equal` nas páginas
    (`filters` e `sort` em categorias e transações, `month` nos relatórios). O tamanho é parecido com o de (A). Cobre
    todos os casos de hoje e também evita o recálculo nos relatórios. Consequências: compara o estado, e não a consulta
    (hoje dá no mesmo, porque todo campo vira parâmetro com o mesmo grão); os componentes compartilhados continuam
    emitindo; a frase do FR-031 também precisa de ajuste.
  - **Recomendação**: (A), por pouco sobre (C). A regra que (A) implementa é literalmente a do requisito ("a consulta
    só se repete quando um critério muda"), e não depende de cada `signal` ganhar o `equal` certo nem de o estado
    espelhar os parâmetros. O custo é deixar o recálculo do gráfico e da lista dos relatórios fora deste bug e ajustar
    a frase do FR-031. Se o usuário quiser resolver também o recálculo nos relatórios sem mexer nos componentes
    compartilhados, a escolha é (C). Se quiser seguir o FR-031 como está escrito, é (B).
- **P2 — Specs 002 e 003**: [NEEDS CLARIFICATION: ajustar as specs no próprio bug-fix ou deixar para uma revisão à
  parte?]
  - (A) No `/speckit-bug-fix`, com registro no `fix.md`. Saem as notas "Desvio conhecido" ligadas ao FR-023 e ao
    FR-031, às SC-006 e SC-007 e aos Casos-limite, como as próprias specs mandam fazer quando o desvio for corrigido.
    Se a P1 for (A) ou (C), as frases do FR-031 e do esclarecimento (linhas 70-72) sobre o lugar da correção também passam a dizer onde ela ficou. Os
    requisitos e critérios em si não mudam.
  - (B) Deixar as specs como estão e só citá-las no `fix.md`. As notas e a frase do FR-031 continuam descrevendo um
    comportamento e um lugar de correção que não existem mais, até uma revisão à parte.
  - **Recomendação**: (A). A mudança só tira ressalvas e corrige a referência ao lugar da correção, sem alterar
    requisito, critério ou cenário, e evita que os artefatos fiquem divergentes do código.
