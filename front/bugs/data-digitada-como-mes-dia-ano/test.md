# Verificação do bug: a data digitada no formulário de transação é lida como mês/dia/ano

- **Slug**: data-digitada-como-mes-dia-ano
- **Testado em**: 2026-10-07
- **Avaliação**: ./assessment.md
- **Correção**: ./fix.md
- **Resultado**: verified

## Resumo

O bug não se reproduz mais: o campo "Data" lê o texto digitado só como dia/mês/ano, com o ano em quatro algarismos,
salva "05/10/2026" como 5 de outubro e "5/9/2026" como 5 de setembro, mantém o texto ao sair do campo e recusa
"02/30/2026", "05/10/26", "05/10/0026", "5/10", "05-10-2026", "2026-09-24" e "Oct 5 2026". Os 17 testes de reprodução
falham sem a correção e passam com ela, e os 5 guardas passam nos dois casos. A suíte completa (357 testes, também com
`TZ=America/Sao_Paulo`), o lint e o build do front passam, sem regressão nem aviso de budget.

## Verificações realizadas

Todos os comandos rodados em `front/`, sobre o estado commitado da correção (`5c7e365`, árvore de trabalho limpa).

| Verificação | Comando / ação | Resultado | Observações |
| --- | --- | --- | --- |
| Prova de detecção (correção revertida) | `git checkout 5c7e365~1 -- .../transaction-form/transaction-form.ts`; corpo de `pt-br-date-adapter.ts` trocado por `export class PtBrDateAdapter extends NativeDateAdapter {}` (com os imports), como no `fix.md`; `npx ng test --watch=false` com `--include` de `pt-br-date-adapter.spec.ts` e `transaction-form.spec.ts` | fail (esperado) | `Tests 17 failed \| 23 passed (40)`, igual ao "antes" do `fix.md`. Falharam exatamente os 11 testes de reprodução do adapter (dia/mês/ano, dia acima de 12, espaços nas pontas, os 7 "should reject" de formatos e datas que a leitura antiga aceitava, texto vazio) e os 6 do `describe('typing the date')` do formulário. Os arquivos foram restaurados com `git checkout HEAD --` e o `git status` voltou a ficar limpo. |
| Guardas (correção revertida) | a mesma execução | pass | Passaram "should reject 31/02/2026", "13/13/2026", "00/10/2026", "05/10/2026x" e "should keep formatting as dd/mm/yyyy", além dos testes que já existiam no `transaction-form.spec.ts`. |
| Reprodução (depois da correção) | os mesmos 2 specs, com o código restaurado | pass | `Test Files 2 passed (2)`, `Tests 40 passed (40)`. Os testes de reprodução são o equivalente automatizado da reprodução da avaliação, feita por spec temporário com o mesmo fluxo (digitar, sair do campo, enviar). |
| Suíte de regressão | `npx ng test --watch=false` | pass | `Test Files 32 passed (32)`, `Tests 357 passed (357)`. |
| Suíte de regressão no fuso do Brasil | `TZ=America/Sao_Paulo npx ng test --watch=false` (Git Bash) | pass | `Test Files 32 passed (32)`, `Tests 357 passed (357)`. O fuso padrão da máquina já é America/Sao_Paulo (UTC-3); o Node respeita a variável `TZ` neste shell (com `TZ=UTC`, o `Intl` informa "UTC"). |
| Lint | `npm run lint` | pass | "All files pass linting." |
| Build e budgets | `npm run build` | pass | Sem aviso nem erro de budget; "Initial total" 633,08 kB (aviso em 700 kB), igual à referência. |
| Conferência manual no navegador (digitação, `blur` e Enter) | — | not-run | Pendência do `fix.md`. Os testes do formulário cobrem a digitação (evento `input`), o `blur` e o envio pelo submit, mas o fluxo não foi exercitado num navegador real. Os passos manuais da avaliação continuam disponíveis. |

## Trechos da saída

```text
# specs do adapter e do formulário, com a correção revertida
 × should parse a typed date as day/month/year
   AssertionError: expected 2026-05-10T03:00:00.000Z to deeply equal 2026-10-05T03:00:00.000Z
 × should parse a day above 12
   AssertionError: expected Invalid Date to deeply equal 2026-09-24T03:00:00.000Z
 × should ignore surrounding spaces
 × should reject 02/30/2026 | 05/10/26 | 05/10/0026 | 5/10 | 05-10-2026 | 2026-09-24 | Oct 5 2026
   AssertionError: expected true to be false // Object.is equality
 × should parse an empty text as no date
   AssertionError: expected Invalid Date to be null
 × should save a typed date as day/month/year: 05/10/2026 | 5/9/2026
   AssertionError: expected "vi.fn()" to be called with arguments: [ ObjectContaining{…} ]
 × should show the typed date unchanged after leaving the field
   AssertionError: expected '09/05/2026' to be '05/09/2026' // Object.is equality
 × should not save the date 02/30/2026 | 2026-09-24 | 05/10/26
   AssertionError: expected "vi.fn()" to not be called at all, but actually been called 1 times
 Test Files  2 failed (2)
      Tests  17 failed | 23 passed (40)

# os mesmos specs, com a correção
 Test Files  2 passed (2)
      Tests  40 passed (40)

# suíte completa (fuso padrão e TZ=America/Sao_Paulo)
 Test Files  32 passed (32)
      Tests  357 passed (357)

# lint
All files pass linting.

# build
                  | Initial total      | 633.08 kB |              148.69 kB
Application bundle generation complete.
```

## Riscos residuais

- A digitação, o `blur` e o Enter não foram conferidos num navegador real (`not-run`); a cobertura vem dos testes de
  componente em jsdom/Node.
- A suíte rodou só no fuso do Brasil (o da máquina e o do `TZ`); outros fusos não foram exercitados. A leitura usa
  `new Date(ano, mês, dia)` local, sem conversão UTC, o que reduz esse risco.
- `plan.md`, `research.md`, `tasks.md` e checklists da 003 podem ainda citar o desvio como pendente; ficam para o
  `/speckit-converge`.
- Os valores com 14 ou mais algarismos (`parseFloat`) e a recusa do zero seguem para avaliação própria (fora deste bug).

## Recomendação

Encerrar o bug: os 17 testes de reprodução falham sem a correção e passam com ela, os guardas passam nos dois casos, e
a suíte (357 testes, também com `TZ=America/Sao_Paulo`), o lint e o build do front estão verdes, sem aviso de budget.
A conferência no navegador é opcional e não impede o fechamento; em seguida, rodar o `/speckit-converge` da feature 003.
