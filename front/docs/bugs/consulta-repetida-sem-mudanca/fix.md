# Correção do bug: a consulta à API se repete quando nenhum critério mudou

- **Slug**: consulta-repetida-sem-mudanca
- **Corrigido em**: 2026-10-07
- **Avaliação**: ./assessment.md
- **Status**: applied

## Resumo

Opção (C) da P1, escolhida pelo usuário: um comparador por conteúdo em `shared/` (`shallowEqual`) e o `equal` dele nos
`signal` de estado das páginas: `filters` e `sort` em categorias e transações, `month` nos relatórios. Gravar de novo o
mesmo conteúdo (a ordenação em uso, o mesmo mês, o texto já aplicado, inclusive o `flushSync` ao sair do campo) não
notifica o `signal`, então o `httpResource` não refaz a consulta. Nos relatórios, o `month()` também mantém a
referência, e o gráfico de comparação acumulada e a lista de transações não recalculam. Pela P2 (A), saíram das specs
002 e 003 as notas "Desvio conhecido" pedidas.

## Alterações

| Arquivo | Alteração | Notas |
|------|--------|-------|
| `front/src/app/shared/shallow-equal/shallow-equal.ts` | adicionado | `shallowEqual(a, b)`: `Date` pelo `getTime()`; objetos comuns chave a chave, um nível, com `Date` pelo valor; o resto por `Object.is` |
| `front/src/app/shared/shallow-equal/shallow-equal.spec.ts` | teste adicionado | 16 casos (`it.each`) de igual e diferente |
| `front/src/app/categories/pages/categories-page/categories-page.ts` | modificado | `{ equal: shallowEqual }` em `filters` e `sort` |
| `front/src/app/categories/pages/categories-page/categories-page.spec.ts` | testes adicionados | 4 testes de regressão |
| `front/src/app/transactions/pages/transactions-page/transactions-page.ts` | modificado | `{ equal: shallowEqual }` em `filters` e `sort` |
| `front/src/app/transactions/pages/transactions-page/transactions-page.spec.ts` | testes adicionados | 4 testes de regressão |
| `front/src/app/reports/pages/reports-page/reports-page.ts` | modificado | `{ equal: shallowEqual }` em `month` |
| `front/src/app/reports/pages/reports-page/reports-page.spec.ts` | testes adicionados | 2 testes de regressão (consulta e referência do mês) |
| `front/specs/002-gestao-de-categorias/spec.md` | modificado | notas "Desvio conhecido" do FR-023, da SC-006 e dos Casos-limite removidas |
| `front/specs/003-gestao-de-transacoes/spec.md` | modificado | notas "Desvio conhecido" do FR-031, da SC-007 e dos Casos-limite removidas; frases do FR-031 e do esclarecimento sobre o lugar da correção ajustadas |

Nenhuma mudança no back, nos services, nos tipos, no contrato da API, no `sort-menu` nem no `month-field`.

## Destaques do diff

```ts
// categories-page.ts (transactions-page.ts é igual, com os seus valores iniciais)
protected readonly filters = signal<CategoryFilters>(
	{ name: '', withTransaction: '', month: null },
	{ equal: shallowEqual },
);
protected readonly sort = signal<Sort>({ active: 'name', direction: 'asc' }, { equal: shallowEqual });

// reports-page.ts
protected readonly month = signal<Date | null>(DateUtils.currentMonth(), { equal: shallowEqual });
```

O caso que a avaliação apontava como risco para (C), o signal form reescrevendo o objeto de filtros (depois da pausa e
no `flushSync` ao sair do campo), fica coberto: o `form()` grava no `model` do componente de filtros, que propaga pelo
`[(filters)]` para o `signal` da página, e é ali que a comparação barra a gravação de mesmo conteúdo. Os testes do
"Nome" e da "Descrição" confirmam.

## Testes adicionados

Todos escritos antes da correção e vistos falhando na asserção (sem estouro de tempo), seguindo o cuidado da
avaliação: nenhuma ação que dispara requisição passa por harness; o texto é digitado no DOM (`value` + evento `input`);
a saída do campo é um `blur` no DOM, seguido de `TestBed.tick()` e da verificação; sem `whenStable()` com requisição
pendente.

| Teste | Antes da correção | Depois |
| --- | --- | --- |
| `categories-page.spec.ts` › filters › when nothing changes › should not reload when the name is typed again as the text applied | falha: `Expected zero matching requests …, found 1.` | passa |
| › should not reload when leaving the name field after the text applied did not change | falha: `… found 1.` | passa |
| › should not reload when the month in use is picked again | falha: `… found 1.` | passa |
| `categories-page.spec.ts` › should not reload when the sort in use is chosen again | falha: `… found 1.` | passa |
| `transactions-page.spec.ts` › filters, when nothing changes › should not reload when the description is typed again as the text applied | falha: `… found 1.` | passa |
| › should not reload when leaving the description field after the text applied did not change | falha: `… found 1.` | passa |
| › should not reload when the month in use is picked again | falha: `… found 1.` | passa |
| `transactions-page.spec.ts` › sorting › should not reload when the sort in use is chosen again | falha: `… found 1.` | passa |
| `reports-page.spec.ts` › when the month shown is picked again › should not reload any block | falha: as 5 URLs (`summary`, `expensesByCategory`, `incomeVsExpense`, `cumulativeExpenses`, `transactions`) | passa |
| › should keep the month given to the cumulative comparison and to the transactions | falha: `expected 2026-10-01T03:00:00.000Z to be 2026-10-01T03:00:00.000Z // Object.is equality` | passa |
| `shallow-equal.spec.ts` (15 casos; o 16º veio na revisão) | falha: `Cannot find module './shallow-equal'` | passa |

Os testes "saindo do campo" respondem, com `httpTesting.match`, ao que a redigitação tenha enviado antes do `blur`, para
que a falha isole a consulta da saída do campo; a consulta da redigitação é coberta pelo teste anterior. Os testes de
relatórios respondem às requisições antes de afirmar, para que a falha não deixe requisições abertas e derrube os
testes seguintes (o `afterEach` do arquivo restaura os timers depois do `verify()`).

Os testes existentes "should reload sorted by the option chosen in the sort menu", "should reload with the filters
chosen" e "should show every block for the month picked" continuam passando e garantem que a mudança real ainda gera a
consulta. Nenhum teste existente foi alterado.

## Verificação local

Em `front/`:

- `npx ng test --watch=false` → 29 arquivos, 284 testes, todos passando (eram 259; +25). Depois dos ajustes da
  revisão: 285 testes, todos passando; lint limpo; build com Initial total de 633,08 kB. Numa das três execuções, o
  teste existente `app.routes.spec.ts` › "should open the transactions page at the root" estourou os 5 s; ele passa
  sozinho (3 de 3) e a suíte inteira passou nas duas execuções seguintes. É lentidão ao carregar o chunk da rota sob a
  carga da suíte, sem relação com esta correção.
- `npm run lint` → "All files pass linting."
- `npm run build` → sem erro nem aviso de budget; Initial total 633,08 kB (igual à referência).
- `npx prettier --write` nos arquivos alterados; nenhum arquivo não tocado foi alterado.

## Specs ajustadas (P2 = A)

Só saíram ressalvas e a referência ao lugar da correção; nenhum requisito, critério ou cenário mudou.

- `front/specs/002-gestao-de-categorias/spec.md`:
  - Casos-limite: os itens "Escolher de novo a ordenação em uso, ou o mesmo mês…" e "No 'Nome', a lista não é
    consultada de novo…" ficaram só com a regra e a referência ao FR-023; saíram as notas "Desvio conhecido".
  - FR-023: saiu a nota "Desvio conhecido: hoje a consulta é refeita… e relatórios."
  - SC-006: saiu "(desvio conhecido: hoje gera; ver FR-023)".
- `front/specs/003-gestao-de-transacoes/spec.md`:
  - Esclarecimento (Sessão, P sobre a ordenação e o mês repetidos): "O atual é um desvio conhecido… a corrigir… no menu
    de ordenação e no campo 'Mês' compartilhados…" passou a "O desvio, que contrariava o princípio III, foi corrigido
    uma só vez pelo fluxo de bugs do front (`front/bugs/consulta-repetida-sem-mudanca/`), fora desta spec: a comparação
    por conteúdo fica no estado de cada página (filtros e ordenação; nos relatórios, o mês), e não no menu de ordenação
    nem no campo 'Mês' compartilhados, e vale também para as páginas de categorias e de relatórios."
  - Casos-limite: os itens da ordenação/mês repetidos e da busca por descrição ficaram só com a regra e a referência ao
    FR-031; saíram as notas "Desvio conhecido".
  - FR-031: a nota "Desvio conhecido: … no menu de ordenação, no campo 'Mês' compartilhados e na busca … fica fora
    desta spec" deu lugar a "Esse comportamento foi corrigido pelo fluxo de bugs do front
    (`front/bugs/consulta-repetida-sem-mudanca/`), fora desta spec, com uma comparação por conteúdo no estado da página
    (filtros e ordenação), e não no menu de ordenação nem no campo 'Mês' compartilhados; a mesma correção vale para as
    páginas de categorias (inclusive a busca por nome) e de relatórios."
  - SC-007: saiu "(desvio conhecido: hoje escolher de novo… ver FR-031)".
- Por decisão do usuário (a lista inicial de seções era exemplo, não limite), saíram também as demais notas deste bug:
  - 002, cenários de aceitação 5 e 9: saiu "(desvio conhecido: hoje a consulta é refeita; ver FR-023)";
  - 002, requisito do menu de ordenação (linha ~469): "(desvio conhecido; ver FR-023)" passou a "(FR-023)";
  - 003, cenário 8: saiu "(desvio conhecido: hoje a consulta é refeita e as linhas dão lugar ao indicador; ver
    FR-031)";
  - 002, esclarecimento da ordenação/mês repetidos: "a corrigir… nos componentes compartilhados de ordenação e de mês"
    passou a dizer que foi corrigido por este bug, com a comparação no estado de cada página, e não nos componentes
    compartilhados;
  - 002 e 003, Sessão 2026-10-05 (P do "Nome" e P da "Descrição"): "O atual é um desvio conhecido, deduzido do código e
    ainda não reproduzido…" passou a dizer que o desvio foi reproduzido num teste e corrigido por este bug, pela mesma
    comparação nos filtros da página.
  - 003, Premissas: o parêntese "(FR-031, no menu de ordenação e no campo 'Mês' compartilhados com as mesmas páginas e
    na busca, ainda a reproduzir)" passou a "(FR-031, corrigida em `front/bugs/consulta-repetida-sem-mudanca/` com uma
    comparação por conteúdo no estado de cada página, e não no menu de ordenação nem no campo 'Mês' compartilhados; vale
    também para as páginas de categorias e de relatórios)" (achado MEDIUM da revisão).
  O texto dos cenários, requisitos e critérios não mudou.
- Notas de outros desvios (por exemplo, o dia de outro mês no campo "Mês", FR-016/FR-024) ficaram como estavam.

## Desvios da avaliação

- A avaliação preferia (A), o helper `distinctRequest`; o usuário escolheu (C) no portão. Os arquivos mudados seguem
  (C): `shared/shallow-equal/` no lugar de `shared/distinct-request/`, e o teste 5 da avaliação deu lugar aos testes do
  comparador.
- (C) compara o estado, e não a consulta, como a avaliação registra. Hoje dá no mesmo: todo campo de filtro e de
  ordenação vira parâmetro com o mesmo grão. Nos relatórios, um `Date` de outro dia do mesmo mês ainda seria uma
  consulta repetida, caso que o `MonthField` não produz hoje (grava o primeiro dia do mês).
- Fora isso, nenhum desvio: a correção cobriu todos os casos listados na avaliação, inclusive o signal form.

## Ajustes da revisão

- `shallow-equal.ts`: `sameValue` compara `Date` com `Object.is(a.getTime(), b.getTime())`, para que dois `Date`
  inválidos sejam iguais. O caso "should take two invalid dates as equal" entrou antes no `shallow-equal.spec.ts` e
  falhou (`expected false to be true`); depois da correção, passa (16 casos no arquivo).
- `transactions-page.spec.ts`: o helper `expectNoListRequest` subiu para o nível do arquivo, como em
  `categories-page.spec.ts`, e é usado nos testes novos; os `it` ficaram separados por linha em branco.

## Pendências

- O `SortMenu` e o `MonthField` continuam gravando valores novos de mesmo conteúdo; quem os usar sem o `equal` volta a
  ter o problema (risco já registrado na avaliação).
- O `reload()` redundante depois do `month.set()` em "Mês anterior"/"Próximo mês" dos relatórios continua fora deste
  bug.
- `plan.md`, `research.md` e checklists das 002 e 003 ainda descrevem o desvio como conhecido ou a corrigir nos
  componentes compartilhados (`002-gestao-de-categorias/plan.md:84`, `003-gestao-de-transacoes/plan.md:74,125,198`,
  `003-gestao-de-transacoes/research.md:55`, `checklists/requirements.md` e `checklists/performance.md`); a reavaliar
  no `/speckit-converge`.
- O estouro intermitente de 5 s em `app.routes.spec.ts` › "should open the transactions page at the root" já existia
  e não tem relação com esta correção (a revisão rodou a suíte 3 vezes sem repetir o problema).
- Próximo passo: `/speckit-bug-test project=front slug=consulta-repetida-sem-mudanca`.
