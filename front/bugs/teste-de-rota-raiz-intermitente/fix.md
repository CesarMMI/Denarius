# Correção do bug: teste da rota raiz estoura o timeout de forma intermitente

- **Slug**: teste-de-rota-raiz-intermitente
- **Corrigido em**: 2026-10-08
- **Avaliação**: ./assessment.md
- **Status**: applied

## Resumo

Aplicada a correção preferida (A) do diagnóstico, conforme a decisão do dono no portão (2026-10-08): o
`app.routes.spec.ts` passou a importar estaticamente a página de transações, com um comentário que explica o motivo.
O custo de carregar o Angular Material e os forms no worker saiu do tempo do teste (C3) e foi para a fase de import do
arquivo, como nos outros specs. O `it`, as duas asserções e o timeout padrão (5 s) não mudaram.

## Alterações

| Arquivo                            | Alteração  | Observação                                                                                |
| ---------------------------------- | ---------- | ----------------------------------------------------------------------------------------- |
| `front/src/app/app.routes.spec.ts` | modificado | Import estático (por efeito) da `transactions-page`, com comentário em inglês. Nada mais. |

Nenhum código de produção mudou. `app.routes.ts`, `front/specs/` e a skill `write-front-tests` não foram tocados.

## Destaques do diff

```diff
 import { routes } from './app.routes';
+// Loads the page and its Material dependencies at import time, as the other specs do,
+// so the test only times the navigation.
+import './transactions/pages/transactions-page/transactions-page';
```

## Testes adicionados ou atualizados

- `app.routes.spec.ts` > `routes` > `should open the transactions page at the root` (C3): é o próprio teste de
  regressão. Continua navegando para `''` com as rotas reais (o router executa o `redirectTo`, o `loadChildren` e o
  `loadComponent`) e conferindo `true` e `/transactions`. Só os imports do arquivo mudaram.

## Verificação local

Todos os comandos rodados em `front/`. Como o defeito depende de tempo e carga, a prova vermelho/verde usou um timeout
local de 300 ms no C3 (`it(..., 300)`), que **não foi commitado**.

1. **Vermelho antes** (spec do HEAD + `it(..., 300)`):
   `npx ng test --watch=false --include=src/app/app.routes.spec.ts`

   ```text
   × should open the transactions page at the root 393ms
   Error: Test timed out in 300ms.
   Test Files  1 failed (1)
        Tests  1 failed (1)
   Duration 1.44s (transform 102ms, setup 284ms, import 60ms, tests 394ms, environment 545ms)
   ```

2. **Verde depois** (com o import estático + `it(..., 300)`), mesmo comando, 3 vezes: `Tests 1 passed (1)` nas três,
   com "tests 34ms", "tests 35ms" e "tests 34ms". O custo foi para a fase de import ("import 687ms", "702ms",
   "695ms"; antes, 60 ms), que não conta para o timeout do teste.

3. **O C3 ainda discrimina** (mutação local, desfeita): troquei em `app.routes.ts` o `redirectTo: 'transactions'` por
   `'categories'` e rodei o mesmo comando:

   ```text
   AssertionError: expected '/categories' to be '/transactions' // Object.is equality
   Tests  1 failed (1)
   ```

   Depois desfiz a troca; o `git diff --stat` voltou a mostrar só o `app.routes.spec.ts`.

4. **Retirada do timeout**: o `}, 300);` voltou a `});`. O `it` está como no HEAD, com o timeout padrão; o diff final
   do spec são só as duas linhas acima.

5. **Suíte completa, 3 vezes** (`npx ng test --watch=false --reporters=verbose`; o `--reporters=verbose` só serve
   para mostrar a duração de cada teste):

   | Execução | Resultado            | Duração do C3 |
   | -------- | -------------------- | ------------- |
   | 1        | 32 arquivos, 363/363 | 70 ms         |
   | 2        | 32 arquivos, 363/363 | 44 ms         |
   | 3        | 32 arquivos, 363/363 | 38 ms         |

   Antes da correção, o C3 levava de 1.529 a 2.703 ms na suíte (avaliação, 6 execuções).

6. **Lint, build e formatação**:
   - `npm run lint` → "All files pass linting."
   - `npm run build` → concluído sem aviso nem erro de budget; "Initial total" de 633,08 kB.
   - `npx prettier --check src/app/app.routes.spec.ts` → sem divergência.

- **Verificação manual**: nenhuma (só teste).

## Desvios da avaliação

Nenhum na correção. Registros:

- **Exceção aceita ao princípio I (TDD)**: o teste que reproduz o bug (C3 com timeout de 300 ms) não fica no código,
  porque o estouro depende de tempo e carga da máquina e um limite tão apertado seria frágil na suíte. A prova
  vermelho/verde foi local e está registrada acima; a proteção que fica no código é o comentário junto ao import.
  Decisão do dono no portão do diagnóstico (2026-10-08).
- O passo 2 da avaliação pedia o verde com 300 ms também na suíte completa. Aqui, com 300 ms, o verde foi provado com
  o arquivo sozinho (3 vezes); a suíte completa rodou 3 vezes já com o timeout padrão, e o C3 ficou entre 38 e 70 ms,
  bem abaixo dos 300 ms.

## Pendências

- Documentar na skill `write-front-tests` o import por efeito num spec que navega pelas rotas reais com carga lazy
  (por exemplo, um teste futuro de `/reports`, que arrasta o Chart.js). Mudança separada, fora deste bug.
- A pendência registrada na 003 (`tasks.md:392`, `plan.md:107`) fica para o `/speckit-converge` da 003.
- Próximo passo: `/speckit-bug-test project=front slug=teste-de-rota-raiz-intermitente`.
