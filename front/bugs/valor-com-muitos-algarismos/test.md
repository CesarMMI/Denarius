# Verificação do bug: valores com 14 ou mais algarismos inteiros são salvos diferentes do digitado

- **Slug**: valor-com-muitos-algarismos
- **Testado em**: 2026-10-08
- **Avaliação**: ./assessment.md
- **Correção**: ./fix.md
- **Resultado**: verified

## Resumo

O bug não se reproduz mais: o formulário de transação recusa, com "Informe um valor válido" e o diálogo aberto, os
valores com mais de 13 algarismos na parte inteira ("99999999999999,99", "12345678901234,56", "10000000000000",
"999999999999999,99" e "00000000000001") e salva "9999999999999,99" exatamente como `-9999999999999.99`. Os 5 testes de
reprodução falham sem a correção e passam com ela, e o teste-limite passa nos dois casos. A suíte completa (363
testes), o lint e o build do front passam, sem regressão nem aviso de budget.

## Verificações realizadas

Todos os comandos rodados em `front/`, sobre o estado commitado da correção (`b51d890`, árvore de trabalho limpa).

| Verificação | Comando / ação | Resultado | Observações |
| --- | --- | --- | --- |
| Prova de detecção (correção revertida) | `git checkout b51d890~1 -- src/app/transactions/components/transaction-form/transaction-form.ts`; `npx ng test --watch=false --include='src/app/transactions/components/transaction-form/*.spec.ts'` | fail (esperado) | `Tests 5 failed \| 41 passed (46)`, igual ao "antes" do `fix.md`. Falharam exatamente os 5 casos de "should not save a value with more than 13 integer digits: %s", com `close` chamado. O arquivo foi restaurado com `git checkout HEAD --` e o `git status` voltou a ficar limpo. |
| Teste-limite (correção revertida) | a mesma execução | pass | "should save the largest value with 13 integer digits exactly" passou, assim como os testes que já existiam no spec. |
| Reprodução (depois da correção) | o mesmo comando, com o código restaurado | pass | `Test Files 2 passed (2)`, `Tests 46 passed (46)`; os 5 casos de reprodução e o teste-limite passam. São o equivalente automatizado da reprodução da avaliação (digitar o valor, enviar e conferir o `close`). |
| Suíte de regressão | `npx ng test --watch=false` | pass | `Test Files 32 passed (32)`, `Tests 363 passed (363)`. |
| Lint | `npm run lint` | pass | "All files pass linting." |
| Build e budgets | `npm run build` | pass | Sem aviso nem erro de budget; "Initial total" 633,08 kB (aviso em 700 kB), igual à referência. |
| Conferência manual no navegador ("99999999999999,99" e "9999999999999,99") | — | not-run | Pendência do `fix.md`. Os testes do formulário cobrem a digitação (evento `input`) e o envio, mas o fluxo não foi exercitado num navegador real. Os passos manuais da avaliação continuam disponíveis. |

## Trechos da saída

```text
# spec do formulário, com a correção revertida
 × should not save a value with more than 13 integer digits: 99999999999999,99
 × should not save a value with more than 13 integer digits: 12345678901234,56
 × should not save a value with more than 13 integer digits: 10000000000000
 × should not save a value with more than 13 integer digits: 999999999999999,99
 × should not save a value with more than 13 integer digits: 00000000000001
   AssertionError: expected "vi.fn()" to not be called at all, but actually been called 1 times
 Test Files  1 failed | 1 passed (2)
      Tests  5 failed | 41 passed (46)

# o mesmo spec, com a correção
 ✓ ... should save the largest value with 13 integer digits exactly
 Test Files  2 passed (2)
      Tests  46 passed (46)

# suíte completa
 Test Files  32 passed (32)
      Tests  363 passed (363)

# lint
All files pass linting.

# build
                  | Initial total      | 633.08 kB |              148.69 kB
Application bundle generation complete.
```

## Riscos residuais

- A digitação e o envio não foram conferidos num navegador real (`not-run`); a cobertura vem dos testes de componente.
- Transações já gravadas com 14 ou mais algarismos inteiros (antes da correção ou por outro cliente da API) podem ser
  exibidas arredondadas, e a edição exige corrigir o valor para salvar (comportamento esperado, ver a avaliação).
- Os totais dos relatórios podem passar de 15 algarismos significativos e ser exibidos com arredondamento (fora deste
  bug).
- A recusa do zero no formulário e a validação de faixa e escala na API seguem como desvios conhecidos, nos fluxos de
  bugs do front e do back.
- `plan.md`, `research.md`, `tasks.md` e checklists da 003 podem ainda citar o desvio como pendente; ficam para o
  `/speckit-converge`.

## Recomendação

Encerrar o bug: os 5 testes de reprodução falham sem a correção e passam com ela, o teste-limite passa nos dois casos,
e a suíte (363 testes), o lint e o build do front estão verdes, sem aviso de budget. A conferência no navegador é
opcional e não impede o fechamento; em seguida, rodar o `/speckit-converge` da feature 003.
