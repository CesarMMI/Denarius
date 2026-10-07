# Verificação do bug: a consulta à API se repete quando nenhum critério mudou

- **Slug**: consulta-repetida-sem-mudanca
- **Testado em**: 2026-10-07
- **Avaliação**: ./assessment.md
- **Correção**: ./fix.md
- **Resultado**: verified

## Resumo

O bug não se reproduz mais: escolher de novo a ordenação em uso, o mesmo mês, redigitar o texto aplicado no "Nome" ou
na "Descrição" (e sair do campo depois) não gera requisição em categorias e transações, e escolher o mesmo mês nos
relatórios não refaz nenhum dos cinco blocos. Os 10 testes de regressão passam com a correção e falham sem ela. A suíte
completa (285 testes), o lint e o build do front passam, sem regressão nem aviso de budget.

## Verificações realizadas

Todos os comandos rodados em `front/`, sobre o estado commitado da correção (`4bbfd40`, árvore de trabalho limpa).

| Verificação | Comando / ação | Resultado | Observações |
| --- | --- | --- | --- |
| Reprodução (depois da correção) | `npx ng test --watch=false --include=<specs de categories-page, transactions-page, reports-page e shallow-equal>` | pass | `Test Files 4 passed (4)`, `Tests 89 passed (89)`. Os testes de regressão são o equivalente automatizado da reprodução da avaliação (o mesmo `HttpTestingController` e as mesmas ações: ordenação, mês, texto redigitado e saída do campo). |
| Prova de detecção (correção revertida) | `git checkout 4bbfd40~1 --` em `categories-page.ts`, `transactions-page.ts` e `reports-page.ts`; mesmos specs das páginas | fail (esperado) | `Tests 10 failed \| 63 passed (73)`: exatamente os 10 testes de regressão do `fix.md`, e nenhum outro. O `shallow-equal.ts` ficou no lugar para o spec dele compilar; os arquivos foram restaurados com `git checkout HEAD --` e o `git status` voltou a ficar limpo. |
| Depois de restaurar | os mesmos specs das páginas | pass | `Tests 73 passed (73)`; 5 usos de `{ equal: shallowEqual }` no código de produção (`filters` e `sort` em categorias e transações, `month` nos relatórios). |
| Testes novos do comparador | `shallow-equal.spec.ts` (incluído na primeira linha) | pass | 16 casos. |
| Suíte de regressão | `npx ng test --watch=false` | pass | `Test Files 29 passed (29)`, `Tests 285 passed (285)`; o estouro intermitente do `app.routes.spec.ts` não apareceu nesta execução. |
| Lint | `npm run lint` | pass | "All files pass linting." |
| Build e budgets | `npm run build` | pass | Sem aviso nem erro de budget; "Initial total" 633,08 kB (aviso em 700 kB), igual à referência. |
| Conferência manual no navegador | — | not-run | A reprodução da avaliação também foi feita por teste automatizado (spec temporário), e não no navegador; os testes de regressão a cobrem por inteiro. |

## Trechos da saída

```text
# testes de regressão, com a correção revertida
 × should not reload any block
   AssertionError: expected [ …(5) ] to deeply equal []
 × should keep the month given to the cumulative comparison and to the transactions
   AssertionError: expected 2026-10-01T03:00:00.000Z to be 2026-10-01T03:00:00.000Z // Object.is equality
 × should not reload when the name is typed again as the text applied
 × should not reload when leaving the name field after the text applied did not change
 × should not reload when the month in use is picked again            (categorias)
 × should not reload when the sort in use is chosen again             (categorias)
 × should not reload when the description is typed again as the text applied
 × should not reload when leaving the description field after the text applied did not change
 × should not reload when the month in use is picked again            (transações)
 × should not reload when the sort in use is chosen again             (transações)
 Test Files  3 failed (3)
      Tests  10 failed | 63 passed (73)

# os mesmos specs, com a correção
 Test Files  3 passed (3)
      Tests  73 passed (73)

# suíte completa
 Test Files  29 passed (29)
      Tests  285 passed (285)

# lint
All files pass linting.

# build
                  | Initial total      | 633.08 kB |               148.67 kB
Application bundle generation complete.
```

## Riscos residuais

- O `SortMenu` e o `MonthField` continuam gravando objetos novos de mesmo conteúdo; uma página nova que os use sem o
  `{ equal: shallowEqual }` volta a ter o problema (risco já registrado na avaliação e no `fix.md`).
- O `shallowEqual` compara o estado, e não a consulta: nos relatórios, um `Date` de outro dia do mesmo mês ainda
  geraria consulta repetida, caso que o `MonthField` não produz hoje.
- O `reload()` redundante depois do `month.set()` em "Mês anterior"/"Próximo mês" dos relatórios ficou fora deste bug.
- `plan.md`, `research.md` e checklists das features 002 e 003 ainda descrevem o desvio como conhecido ou a corrigir
  nos componentes compartilhados; ficam para o `/speckit-converge` (pendência do `fix.md`).
- O estouro intermitente de 5 s em `app.routes.spec.ts` › "should open the transactions page at the root" já existia
  antes da correção e não tem relação com ela; não se repetiu nesta verificação.

## Recomendação

Encerrar o bug: os 10 testes de regressão passam com a correção e falham sem ela, e a suíte (285 testes), o lint e o
build do front estão verdes, sem aviso de budget. As pendências listadas (artefatos das 002 e 003 e o estouro
intermitente do `app.routes.spec.ts`) seguem em separado e não impedem o fechamento.
