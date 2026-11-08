# Verificação do bug: teste da rota raiz estoura o timeout de forma intermitente

- **Slug**: teste-de-rota-raiz-intermitente
- **Testado em**: 2026-10-08
- **Avaliação**: ./assessment.md
- **Correção**: ./fix.md
- **Resultado**: verified

## Resumo

A correção (import estático da página de transações no `app.routes.spec.ts`, commit `eaf0d8e`) tira do tempo do C3 a
primeira carga do Angular Material e dos forms. A prova vermelho/verde com timeout local de 300 ms se repetiu: o spec
anterior falha sozinho e o spec do HEAD passa sozinho (3 de 3). Na suíte completa, o C3 levou de 20 a 27 ms (antes, de
1.529 a 2.703 ms), e a suíte (363 testes), o lint e o build passam, sem regressão.

## Verificações realizadas

Todos os comandos rodados em `front/`, sobre o HEAD `eaf0d8e` (árvore limpa). O timeout de 300 ms no C3
(`it(..., 300)`) foi aplicado só localmente, para a prova, e desfeito com `git checkout HEAD --`; não foi commitado.

| Verificação | Comando / ação | Resultado | Observações |
| --- | --- | --- | --- |
| Reprodução (antes da correção) | `git checkout eaf0d8e~1 -- src/app/app.routes.spec.ts`, `it(..., 300)` local; `npx ng test --watch=false --include=src/app/app.routes.spec.ts` | fail (esperado) | `Test timed out in 300ms`; C3 em 390 ms; "import 60ms, tests 391ms". |
| Reprodução (depois da correção) | spec do HEAD, `it(..., 300)` local; o mesmo comando, 3 vezes | pass | `Tests 1 passed (1)` nas três; "tests 35ms", "34ms", "34ms"; o custo foi para o import ("694ms", "708ms", "691ms"). |
| Restauração do spec | `git checkout HEAD -- src/app/app.routes.spec.ts`; `git status --short` | pass | Árvore limpa depois da prova. |
| Suíte de regressão (3 vezes) | `npx ng test --watch=false --reporters=verbose` | pass | `Test Files 32 passed (32)`, `Tests 363 passed (363)` nas três; C3 em 22, 27 e 20 ms. |
| Lint | `npm run lint` | pass | "All files pass linting." |
| Build e budgets | `npm run build` | pass | Sem aviso nem erro de budget; "Initial total" 633,08 kB (aviso em 700 kB); chunk `transactions-page` 41,61 kB. |

Duração do C3 na suíte completa:

| Execução | Resultado | Duração do C3 | Duração da suíte |
| --- | --- | --- | --- |
| 1 | 32 arquivos, 363/363 | 22 ms | 10,75 s |
| 2 | 32 arquivos, 363/363 | 27 ms | 10,83 s |
| 3 | 32 arquivos, 363/363 | 20 ms | 10,91 s |

Antes da correção: de 1.529 a 2.703 ms (avaliação, 6 execuções). No `fix.md`: 70, 44 e 38 ms.

## Trechos da saída

```text
# vermelho: spec anterior + it(..., 300), arquivo sozinho
× should open the transactions page at the root 390ms
Error: Test timed out in 300ms.
 Test Files  1 failed (1)
      Tests  1 failed (1)
   Duration  1.42s (transform 103ms, setup 270ms, import 60ms, tests 391ms, environment 543ms)

# verde: spec do HEAD + it(..., 300), arquivo sozinho (1 de 3)
 Test Files  1 passed (1)
      Tests  1 passed (1)
   Duration  1.68s (transform 110ms, setup 269ms, import 694ms, tests 35ms, environment 542ms)

# suíte completa (execução 1 de 3)
 ✓  denarius  src/app/app.routes.spec.ts > routes > should open the transactions page at the root 22ms
 Test Files  32 passed (32)
      Tests  363 passed (363)

# lint
All files pass linting.

# build
                    | Initial total       | 633.08 kB |               148.69 kB
Application bundle generation complete.
```

## Riscos residuais

- O estouro original dependia da carga da máquina (outros agentes rodando suítes e builds ao mesmo tempo). Esta
  verificação não reproduziu essa carga; a prova determinística é o limite de 300 ms, que o C3 cumpre com folga
  (cerca de 35 ms sozinho e de 20 a 27 ms na suíte).
- O teste de 300 ms não fica no código (exceção ao princípio I aceita pelo dono no portão do diagnóstico). A proteção
  que fica é o comentário junto ao import; se alguém removê-lo como "não usado", o problema volta sem que nenhum teste
  falhe de forma determinística.
- A mutação do `redirectTo` (o C3 ainda discrimina) foi feita no `/speckit-bug-fix` e não foi repetida aqui; a
  asserção do C3 não mudou desde então.

## Recomendação

Encerrar o bug: a prova vermelho/verde com 300 ms se repetiu no HEAD, o C3 caiu de 1,5-2,7 s para 20-27 ms na suíte
completa, e a suíte (363 testes), o lint e o build do front estão verdes, sem aviso de budget. Ficam as pendências do
`fix.md`: documentar o import por efeito na skill `write-front-tests` (mudança separada) e atualizar a pendência da
003 (`tasks.md:392`, `plan.md:107`) pelo `/speckit-converge`.
