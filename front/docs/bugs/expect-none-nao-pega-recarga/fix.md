# Correção do bug: `expectNone` com string não pega a recarga da lista

- **Slug**: expect-none-nao-pega-recarga
- **Corrigido em**: 2026-10-08
- **Avaliação**: ./assessment.md
- **Status**: applied

## Resumo

Aplicada a correção preferida do diagnóstico, com as decisões do dono no portão (P1 a P4 = A). As 4 asserções vazias
e as 2 parciais das páginas de categorias e transações passaram a usar o helper `expectNoListRequest()` de cada
arquivo, que confere com um predicado em `req.url`. No spec dos relatórios, a asserção foi movida para antes do
`await fixture.whenStable()` e virou um predicado só, para qualquer GET. A receita e o `SKILL.md` da skill
`write-front-tests` agora ensinam o padrão certo. Nenhum código de produção mudou.

## Alterações

| Arquivo                                                                        | Alteração  | Observação                                                                                                                                                                                 |
| ------------------------------------------------------------------------------ | ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `front/src/app/categories/pages/categories-page/categories-page.spec.ts`       | modificado | Linhas 390, 407 e 469 (eram 390-391, 407-408 e 470-471): `expectNoListRequest()`. No formulário cancelado, o `expectNone(baseUrl)` do POST continua logo depois (P2)                       |
| `front/src/app/transactions/pages/transactions-page/transactions-page.spec.ts` | modificado | Linhas 523, 540 e 636 (eram 523-524, 540-541 e 637-638): idem                                                                                                                              |
| `front/src/app/reports/pages/reports-page/reports-page.spec.ts`                | modificado | `TestBed.tick(); httpTesting.expectNone((req) => req.method === 'GET');` antes do `await fixture.whenStable()`, com um comentário em inglês; o laço de 4 `expectNone` com string saiu (P3) |
| `front/.claude/skills/write-front-tests/references/recipes.md`                 | modificado | Helper `expectNoListRequest()` ao lado do `expectList()` e as linhas 328-329 trocadas por `expectNoListRequest();`                                                                         |
| `front/.claude/skills/write-front-tests/SKILL.md`                              | modificado | Linha 41 cita o par `expectList`/`expectNoListRequest`; armadilha nova sobre string × query string e sobre o `expectNone` antes do `whenStable()`                                          |

`front/specs/`, o código de produção e o import por efeito do bug `teste-de-rota-raiz-intermitente` não foram tocados.

## Destaques do diff

```diff
 			expect(snackBar.open).toHaveBeenCalledWith('O nome da categoria não pode ser vazio.', 'Fechar', { duration: 5000 });
-			TestBed.tick();
-			httpTesting.expectNone(baseUrl);
+			expectNoListRequest();
```

```diff
 		expectReport('expensesByCategory').flush(reports.expensesByCategory);
+		// Before whenStable(): a reload of the other blocks would leave their requests pending and hang it.
+		TestBed.tick();
+		httpTesting.expectNone((req) => req.method === 'GET');
 		await fixture.whenStable();

-		for (const report of names.filter((report) => report !== 'expensesByCategory')) {
-			httpTesting.expectNone(`${baseUrl}/${report}`);
-		}
```

## Testes adicionados ou atualizados

Os próprios testes corrigidos são os testes de regressão. Nenhuma asserção foi enfraquecida: as do formulário cancelado
ganharam a checagem da recarga e mantiveram a do POST; a dos relatórios passou de 4 URLs que nunca casavam para
qualquer GET.

- `categories-page.spec.ts` > saving > `should do nothing when the form is cancelled`
- `categories-page.spec.ts` > saving > `should show the API error detail and not reload when saving fails`
- `categories-page.spec.ts` > deleting > `should show why the API refuses to delete a category with transactions`
- `transactions-page.spec.ts` > saving > `should do nothing when the form is cancelled`
- `transactions-page.spec.ts` > saving > `should show the API error detail and not reload when saving fails`
- `transactions-page.spec.ts` > deleting > `should show the API error and keep the list when the deletion fails`
- `reports-page.spec.ts` > `should keep the other blocks when one fails, and retry only that one`

## Verificação local

Todos os comandos rodados em `front/`. A prova vermelho/verde usou, por asserção, a quebra temporária de produção da
tabela do diagnóstico, aplicada antes da execução e desfeita com `git checkout -- <arquivo>` logo depois:

- **cancel**: `if (!input) return;` vira `if (!input) return this.reloadCategories();` (ou `reloadTransactions()`).
- **save**: o `error` do `save()` chama `reloadCategories()` (ou `reloadTransactions()`) antes do `showError`.
- **delete**: o `error` do `delete()` chama `reloadCategories()` (ou `reloadTransactions()`) antes do `showError`.
- **retry**: em `reports-page.html:17`, `(retry)="expensesByCategory.reload()"` vira `(retry)="reload()"`.

Cada execução: `npx ng test --watch=false --include=<spec> --filter="<nome do teste>"`.

| Asserção                                         | Quebra | Antes (spec do HEAD, com a quebra)                                                                                | Depois (spec corrigido, com a quebra)                                                                                                     | Sem a quebra |
| ------------------------------------------------ | ------ | ----------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- | ------------ |
| `categories-page.spec.ts:390` (cancelado)        | cancel | falha no `afterEach` (`:64:30`): `Expected no open requests, found 1: GET …/api/categories?orderBy=name&asc=true` | falha na linha 390 (via `expectNoListRequest`, `:124:15`): `Expected zero matching requests for criteria "Match by function: ", found 1.` | 1 passed     |
| `categories-page.spec.ts:407` (salvar falha)     | save   | falha no `afterEach` (`:64:30`), mesma mensagem                                                                   | falha na linha 407, mesma mensagem do predicado                                                                                           | 1 passed     |
| `categories-page.spec.ts:469` (excluir recusado) | delete | falha no `afterEach` (`:64:30`), mesma mensagem                                                                   | falha na linha 469, mesma mensagem do predicado                                                                                           | 1 passed     |
| `transactions-page.spec.ts:523` (cancelado)      | cancel | falha no `afterEach` (`:87:30`): `… found 1: GET …/api/transactions?orderBy=date&asc=false`                       | falha na linha 523 (via `expectNoListRequest`, `:96:15`), mensagem do predicado                                                           | 1 passed     |
| `transactions-page.spec.ts:540` (salvar falha)   | save   | falha no `afterEach` (`:87:30`)                                                                                   | falha na linha 540                                                                                                                        | 1 passed     |
| `transactions-page.spec.ts:636` (excluir falha)  | delete | falha no `afterEach` (`:87:30`)                                                                                   | falha na linha 636                                                                                                                        | 1 passed     |
| `reports-page.spec.ts:163` (retry)               | retry  | `Test timed out in 5000ms` (5234 ms) e depois o `afterEach` (`:78:15`) com os 4 GETs `…?month=2026-10`            | falha na linha 163 em 325 ms: `… found 4.`; o `afterEach` não falha                                                                       | 1 passed     |

Em todas as execuções "depois", a falha apareceu uma vez só: o `expectNone` com predicado retira da fila a requisição
casada, e o `verify()` do `afterEach` passa.

Na execução "antes" da quebra **delete** de transações, o filtro (`keep the list when the deletion fails`) também
pegou o vizinho `should show a fallback message and keep the list when the deletion fails without a detail`, que já
usava o predicado: ele falhou no próprio corpo (`:601:16`), enquanto o teste com string falhou no `afterEach`. Nas
execuções seguintes, o filtro foi ancorado (`the API error and keep the list when the deletion fails$`).

Depois de desfazer cada quebra, `git diff --stat -- front/src` mostra só os três `*.spec.ts`.

Suíte, lint, build e formatação:

- `npx ng test --watch=false` → 32 arquivos, **363 passed (363)**, 13,79 s.
- `npm run lint` → "All files pass linting."
- `npm run build` → concluído sem aviso nem erro de budget; "Initial total" de 633,08 kB.
- `npx prettier --check` nos três specs → sem divergência.

- **Verificação manual**: nenhuma (só teste).

## Desvios da avaliação

Nenhum na correção. Registros:

- A ordem das armadilhas no `SKILL.md`: a nova entrou **antes** da última ("In page specs, never drive an action
  through a harness…"), porque a introdução da seção diz que "the last one hangs the test without an error".
- O helper `expectNoListRequest()` acrescentado à receita (perto das linhas 290-293) segue o passo 4 do diagnóstico;
  sem ele, as linhas 328-329 corrigidas chamariam um helper que a receita não define.
- Os números de linha mudaram com a correção: as asserções das páginas estão agora nas linhas 390, 407, 469
  (categorias) e 523, 540, 636 (transações), e a dos relatórios na 163.

## Pendências

- Registrar na skill `write-front-tests` o import por efeito do bug `teste-de-rota-raiz-intermitente` (mudança
  separada, fora deste bug).
- Opcional (riscos do diagnóstico): dar nome ao predicado ou passar o `description` do `expectNone`, para a mensagem
  não sair como `Match by function: `.
- Próximo passo: `/speckit-bug-test project=front slug=expect-none-nao-pega-recarga`.
