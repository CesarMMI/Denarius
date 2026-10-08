# Verificação do bug: `expectNone` com string não pega a recarga da lista

- **Slug**: expect-none-nao-pega-recarga
- **Testado em**: 2026-10-08
- **Avaliação**: ./assessment.md
- **Correção**: ./fix.md
- **Resultado**: verified

## Resumo

Com a quebra temporária de produção da tabela de prova do diagnóstico, o spec anterior à correção (`90f06c2~1`) só
acusa a recarga no `afterEach` (nos relatórios, por timeout de 5 s), e o spec do HEAD (`90f06c2`) falha no próprio
corpo, na linha da asserção, uma vez só. Sem a quebra, os testes passam. A suíte (363 testes), o lint e o build passam,
sem regressão.

## Verificações realizadas

Todos os comandos rodados em `front/`, sobre o HEAD `90f06c2` (árvore limpa). Para cada asserção provada: a quebra foi
aplicada com `sed` no arquivo de produção; o spec rodou primeiro na versão `90f06c2~1` (`git checkout 90f06c2~1 --
<spec>`) e depois na do HEAD (`git checkout HEAD -- <spec>`); a quebra foi desfeita com `git checkout HEAD -- <arquivo>`
e o teste rodou de novo. Cada execução: `npx ng test --watch=false --include=<spec> --filter="<nome do teste>"`.

Quebras (as mesmas do `fix.md`):

- **save**: em `categories-page.ts`, o `error` do `save()` chama `this.reloadCategories()` antes do `showError`.
- **cancel**: em `categories-page.ts`, `if (!input) return;` vira `if (!input) return this.reloadCategories();`.
- **delete**: em `transactions-page.ts`, o `error` do `delete()` chama `this.reloadTransactions()` antes do `showError`.
- **retry**: em `reports-page.html:17`, `(retry)="expensesByCategory.reload()"` vira `(retry)="reload()"`.

| Verificação | Comando / ação | Resultado | Observações |
| --- | --- | --- | --- |
| Reprodução (antes da correção): categorias, salvar falha | quebra **save**, spec de `90f06c2~1`, filtro `not reload when saving fails` | fail (esperado) | Falha só no `afterEach` (`categories-page.spec.ts:64:30`): `Expected no open requests, found 1: GET …/api/categories?orderBy=name&asc=true`. |
| Reprodução (depois da correção): categorias, salvar falha | quebra **save**, spec do HEAD | fail na asserção (esperado) | Falha na linha 407 (via `expectNoListRequest`, `:124:15`): `Expected zero matching requests for criteria "Match by function: ", found 1.` Um erro só; o `afterEach` passa. |
| Reprodução (antes): categorias, formulário cancelado | quebra **cancel**, spec de `90f06c2~1`, filtro `should do nothing when the form is cancelled` | fail (esperado) | Falha só no `afterEach` (`:64:30`), mesma mensagem. |
| Reprodução (depois): categorias, formulário cancelado | quebra **cancel**, spec do HEAD | fail na asserção (esperado) | Falha na linha 390 (via `:124:15`), mensagem do predicado. Um erro só. |
| Reprodução (antes): transações, excluir falha | quebra **delete**, spec de `90f06c2~1`, filtro `the API error and keep the list when the deletion fails$` | fail (esperado) | Falha só no `afterEach` (`transactions-page.spec.ts:87:30`): `… found 1: GET …/api/transactions?orderBy=date&asc=false`. |
| Reprodução (depois): transações, excluir falha | quebra **delete**, spec do HEAD | fail na asserção (esperado) | Falha na linha 636 (via `expectNoListRequest`, `:96:15`), mensagem do predicado. Um erro só. |
| Reprodução (antes): relatórios, retry | quebra **retry**, spec de `90f06c2~1`, filtro `keep the other blocks when one fails` | fail (esperado) | `Test timed out in 5000ms` (5233 ms) e depois o `afterEach` (`reports-page.spec.ts:78:15`) com os 4 GETs `…?month=2026-10`. |
| Reprodução (depois): relatórios, retry | quebra **retry**, spec do HEAD | fail na asserção (esperado) | Falha na linha 163 em 354 ms: `… found 4.` Um erro só; o `afterEach` passa. |
| Sem a quebra (spec do HEAD) | os 4 filtros acima, depois de desfazer cada quebra | pass | `1 passed` em cada um. |
| Restauração | `git checkout HEAD -- <arquivos>`; `git status --short` | pass | Árvore limpa depois de cada prova e ao fim (salvo este `test.md`). |
| Suíte de regressão | `npx ng test --watch=false` | pass | `Test Files 32 passed (32)`, `Tests 363 passed (363)`, 13,61 s. |
| Lint | `npm run lint` | pass | "All files pass linting." |
| Build e budgets | `npm run build` | pass | Sem aviso nem erro de budget; "Initial total" 633,08 kB. |

As outras três asserções corrigidas (`categories-page.spec.ts:469` e `transactions-page.spec.ts:523, 540`) não foram
reprovadas de novo aqui; a prova delas está no `fix.md`, e elas usam o mesmo helper das que foram reprovadas.

## Trechos da saída

```text
# categorias, quebra save, spec de 90f06c2~1
Error: Expected no open requests, found 1: GET http://localhost:5276/api/categories?orderBy=name&asc=true
 ❯ src/app/categories/pages/categories-page/categories-page.spec.ts:64:30
     64|  afterEach(() => httpTesting.verify());

# categorias, quebra save, spec do HEAD
Error: Expected zero matching requests for criteria "Match by function: ", found 1.
 ❯ expectNoListRequest src/app/categories/pages/categories-page/categories-page.spec.ts:124:15
 ❯ src/app/categories/pages/categories-page/categories-page.spec.ts:407:4

# transações, quebra delete, spec de 90f06c2~1 / spec do HEAD
Error: Expected no open requests, found 1: GET http://localhost:5276/api/transactions?orderBy=date&asc=false
 ❯ src/app/transactions/pages/transactions-page/transactions-page.spec.ts:87:30
Error: Expected zero matching requests for criteria "Match by function: ", found 1.
 ❯ expectNoListRequest src/app/transactions/pages/transactions-page/transactions-page.spec.ts:96:15
 ❯ src/app/transactions/pages/transactions-page/transactions-page.spec.ts:636:4

# relatórios, quebra retry, spec de 90f06c2~1
× should keep the other blocks when one fails, and retry only that one 5233ms
Error: Test timed out in 5000ms.
Error: Expected no open requests, found 4: GET …/api/reports/summary?month=2026-10, … (4 GETs)
 ❯ src/app/reports/pages/reports-page/reports-page.spec.ts:78:15

# relatórios, quebra retry, spec do HEAD
× should keep the other blocks when one fails, and retry only that one 354ms
Error: Expected zero matching requests for criteria "Match by function: ", found 4.
 ❯ src/app/reports/pages/reports-page/reports-page.spec.ts:163:15

# suíte completa
 Test Files  32 passed (32)
      Tests  363 passed (363)

# lint
All files pass linting.

# build
                    | Initial total       | 633.08 kB |               148.69 kB
Application bundle generation complete.
```

## Riscos residuais

- A prova cobre 4 das 7 asserções corrigidas (pelo menos uma por spec); as outras 3 usam o mesmo helper e foram
  reprovadas no `/speckit-bug-fix`.
- A mensagem do predicado continua `Match by function: ` (função anônima). A stack aponta a linha da asserção, o que
  basta; dar nome ao predicado segue como pendência opcional do `fix.md`.
- Um `expectNone` escrito depois de um `await fixture.whenStable()` volta a falhar por timeout. A armadilha está agora
  no `SKILL.md` da `write-front-tests`, mas nenhum lint impede o padrão.

## Recomendação

Fechar o bug: verificado de ponta a ponta. Com a regressão simulada, as asserções corrigidas falham no próprio corpo,
na linha certa e sem cascata, enquanto as anteriores só acusavam no `afterEach` ou por timeout; a suíte, o lint e o
build passam.
