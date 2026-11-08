# Verificação do bug: os calendários mostram e anunciam textos em inglês

- **Slug**: calendarios-em-ingles
- **Testado em**: 2026-10-07
- **Avaliação**: ./assessment.md
- **Correção**: ./fix.md
- **Resultado**: verified

## Resumo

O bug não se reproduz mais: o botão que abre o calendário e os controles das três visões (meses, dias e anos), nos
calendários do "Mês" (`MonthField`) e da "Data" (`TransactionForm`), se anunciam em português. Os 3 testes de regressão
dos componentes passam com a correção e falham sem ela. A suíte completa (304 testes), o lint e o build do front
passam, sem regressão nem aviso de budget.

## Verificações realizadas

Todos os comandos rodados em `front/`, sobre o estado commitado da correção (`0c2e57d`, árvore de trabalho limpa).

| Verificação | Comando / ação | Resultado | Observações |
| --- | --- | --- | --- |
| Prova de detecção (correção revertida) | `git checkout 0c2e57d~1 --` em `month-field.ts` e `transaction-form.ts`; `npx ng test --watch=false --include=<month-field.spec.ts> --include=<transaction-form.spec.ts>` | fail (esperado) | `Tests 3 failed \| 22 passed (25)`: exatamente os 3 testes de regressão do `fix.md`, e nenhum outro. A classe `PtBrDatepickerIntl` ficou no lugar para o spec dela compilar; os arquivos foram restaurados com `git checkout HEAD --` e o `git status` voltou a ficar limpo. |
| Reprodução (depois da correção) | os mesmos specs, mais `pt-br-datepicker-intl.spec.ts` | pass | `Test Files 3 passed (3)`, `Tests 41 passed (41)`. Os testes de regressão são o equivalente automatizado da reprodução da avaliação (o mesmo `open()` do `MatDatepicker`, a mesma navegação pelas visões e as mesmas consultas ao overlay). |
| Testes novos da classe | `pt-br-datepicker-intl.spec.ts` (incluído na linha acima) | pass | 16 casos: os 14 rótulos, o `formatYearRangeLabel` ("de 2016 a 2039") e o `formatYearRange`. |
| Suíte de regressão | `npx ng test --watch=false` | pass | `Test Files 30 passed (30)`, `Tests 304 passed (304)`. |
| Lint | `npm run lint` | pass | "All files pass linting." |
| Build e budgets | `npm run build` | pass | Sem aviso nem erro de budget; "Initial total" 633,08 kB (aviso em 700 kB), igual à referência. |
| Conferência visual no navegador (leitor de tela ou inspetor de acessibilidade) | — | not-run | A reprodução da avaliação também foi feita por teste automatizado (spec temporário), e não no navegador; os testes de regressão cobrem os mesmos rótulos. Os passos manuais da avaliação continuam disponíveis para uma conferência opcional. |

## Trechos da saída

```text
# testes de regressão, com a correção revertida
 × should name the calendar toggle in Portuguese                    (month-field)
   AssertionError: expected 'Open calendar' to be 'Abrir calendário'
 × should name the calendar controls in Portuguese in every view    (month-field)
   AssertionError: expected 'Choose date' to be 'Escolher data'
 × should name the date calendar controls in Portuguese             (transaction-form)
   AssertionError: expected 'Open calendar' to be 'Abrir calendário'
 Test Files  2 failed (2)
      Tests  3 failed | 22 passed (25)

# os mesmos specs e o da classe, com a correção
 Test Files  3 passed (3)
      Tests  41 passed (41)

# suíte completa
 Test Files  30 passed (30)
      Tests  304 passed (304)

# lint
All files pass linting.

# build
                  | Initial total      | 633.08 kB |               148.65 kB
Application bundle generation complete.
```

## Riscos residuais

- Um componente novo com calendário precisa do mesmo provider nos `providers`; sem ele, os rótulos voltam ao inglês
  (risco registrado na avaliação e no `fix.md`).
- Numa atualização do Angular Material, um rótulo novo do `MatDatepickerIntl` sairia em inglês; o teste da classe
  lista os atuais.
- A conferência no navegador não foi feita; o anúncio pelo leitor de tela real não foi observado, só os atributos e
  textos no DOM.
- `plan.md`, `research.md`, `tasks.md` e checklists das features 002 e 003 podem ainda citar o desvio como pendente;
  ficam para o `/speckit-converge` (pendência do `fix.md`).

## Recomendação

Encerrar o bug: os 3 testes de regressão passam com a correção e falham sem ela, e a suíte (304 testes), o lint e o
build do front estão verdes, sem aviso de budget. A conferência visual no navegador é opcional e não impede o
fechamento.
