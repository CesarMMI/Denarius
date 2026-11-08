# Avaliação do bug: teste da rota raiz estoura o timeout de forma intermitente

- **Slug**: teste-de-rota-raiz-intermitente
- **Projeto**: front
- **Criada em**: 2026-10-08
- **Origem**: texto colado (relato do orquestrador), com as menções em `front/bugs/consulta-repetida-sem-mudanca/fix.md`
  (linhas 89-91 e 165-166) e `test.md` (linhas 26, 75-76 e 82)
- **Veredito**: válido
- **Severidade**: medium

## Relato (resumido)

> O teste `app.routes.spec.ts` › "should open the transactions page at the root" (C3, T014 da feature 003) estoura o
> timeout de 5 s em cerca de 1 de cada 3 execuções da suíte completa (`npx ng test --watch=false`). Rodado sozinho,
> passa. Ele usa as rotas reais e navega até `''`, o que carrega a rota lazy de transações via import dinâmico. A
> suspeita é lentidão do import dinâmico sob a carga da suíte em paralelo.

O `fix.md` do bug `consulta-repetida-sem-mudanca` registra o estouro em 1 de 3 execuções; o `test.md` do mesmo bug, uma
execução sem o estouro. O `tasks.md` (linha 392) e o `plan.md` (linha 107) da 003 deixam a entrega pendente deste bug.

## Sintoma

Na suíte completa, o C3 às vezes falha com `Error: Test timed out in 5000ms.`, sem nenhuma mudança de código entre uma
execução e outra. O esperado é que ele passe sempre, porque o que ele confere (o endereço raiz leva a
`/transactions` com as rotas reais) é determinístico.

## Reprodução

Ambiente: Windows 11, 12 núcleos lógicos, HEAD `6d3c3c2`, Vitest 4.1.10, `@angular/build` 21.2.21, 32 arquivos e
363 testes. Todas as execuções em 2026-10-08.

**Suíte completa, sem alteração (6 execuções seguidas):**

| Execução | Resultado | Duração do C3 | Observação |
| --- | --- | --- | --- |
| 1 | 363/363 | 1.529 ms | primeira do dia (cache frio: "environment 512,27s") |
| 2 | 363/363 | 2.185 ms | |
| 3 | 363/363 | 2.342 ms | |
| 4 | 363/363 | 2.329 ms | |
| 5 | 363/363 | 2.483 ms | |
| 6 | 363/363 | 2.703 ms | |

Nenhuma das 6 falhou (0/6), mas o C3, que só faz uma navegação, gastou de 1,5 s a 2,7 s, ou seja, de 30% a 54% do
limite de 5 s. Nas execuções 2 a 6, o arquivo começou no instante 0 da suíte, junto com os outros 11 workers, que
também sobem o jsdom e importam o Angular nesse momento. O estouro relatado (1 em 3) aconteceu enquanto outros agentes
rodavam suítes e builds na mesma máquina. Com o dobro da carga, o tempo passa de 5 s.

**Rodado sozinho** (`npx ng test --watch=false --include=src/app/app.routes.spec.ts`, 3 vezes): passa, com cerca de
700 ms de teste ("tests 682-721ms").

**Reprodução determinística (experimento temporário, já desfeito):** com o timeout do C3 reduzido a 300 ms
(`it(..., 300)`), o teste falha sempre, mesmo sozinho: `Error: Test timed out in 300ms.` Isso mostra que o corpo do
teste gasta mais que um tempo de navegação razoável mesmo sem carga.

## Caminhos de código suspeitos

- `front/src/app/app.routes.spec.ts:10-16`: o C3. O corpo do `it` chama `router.navigateByUrl('')`, que segue o
  `redirectTo` e executa os dois carregadores lazy dentro do tempo medido do teste.
- `front/src/app/app.routes.ts:10-12`: `loadChildren` de `transactions` (import dinâmico de `transactions.routes`).
- `front/src/app/transactions/transactions.routes.ts:6`: `loadComponent` da `TransactionsPage` (segundo import
  dinâmico). O router o resolve na navegação, mesmo sem `RouterOutlet`.
- `front/src/app/transactions/pages/transactions-page/transactions-page.ts:1-25`: a página e os componentes que ela
  importa (filtros, tabela, formulário, `shared/`) trazem 18 entradas do `@angular/material` (`button`, `icon`,
  `sort`, `tooltip`, `input`, `datepicker`, `core`, `select`, `form-field`, `dialog`, `table`, `snack-bar`,
  `progress-spinner`, `progress-bar`, `menu`, `chips`, `card`, `button-toggle`), além de `@angular/forms` e
  `@angular/forms/signals`. Nada de Chart.js: o `ng2-charts` só entra no chunk de relatórios.
- `node_modules/@angular/build/src/builders/unit-test/runners/vitest/build-options.js:126` (`externalPackages: true`)
  e `plugins.js:128` (`isolate: false`): configuração do builder, não do projeto. O front não tem `vitest.config` nem
  opções de teste no `angular.json` (`"test": { "builder": "@angular/build:unit-test" }`), e o `testTimeout` é o
  padrão do Vitest (5 s).

## Hipótese de causa raiz

**O C3 paga, dentro do tempo do teste, a primeira carga do Angular Material no worker.** Confiança alta.

Como o builder roda o Vitest:

- Os specs e o código do app são empacotados pelo esbuild em memória; os pacotes de `node_modules` ficam externos
  (`externalPackages: true`). A página de transações vira um chunk de 59,94 kB (`chunk-…` "transactions-page"), mas o
  Material e o `@angular/forms` são carregados pelo Node a partir de `node_modules` na primeira vez que um módulo os
  importa naquele worker.
- Nos outros specs, esses imports são estáticos: o custo cai na fase de import do arquivo ("import 23-26s" no
  resumo), que não conta para o `testTimeout`.
- No C3, o único caminho até a página é o import dinâmico do router, que roda dentro do `it`. O custo cai inteiro no
  tempo medido do teste, que tem 5 s.

**Evidência (instrumentação temporária no C3, já desfeita).** Tempos do corpo do teste, em ms:

| Cenário | Material e forms | `transactions.routes` | página | navegação |
| --- | --- | --- | --- | --- |
| Sozinho, import da página sem pré-carga | — | 4 | 662 | 11 |
| Suíte, import da página sem pré-carga (3 execuções) | — | 6-13 | 1.664-2.057 | 20-46 |
| Sozinho, Material e forms importados antes, em separado | 580 | 4 | 82 | 12 |
| Suíte, Material e forms importados antes (2 execuções) | 1.901-2.433 | 7-9 | 80-86 | 20-26 |

- O import de `transactions.routes` (293 bytes) e a navegação em si custam de 4 a 46 ms. Não são o problema.
- Cerca de 88% do import da página (580 de 662 ms sozinho) é a carga do Material e dos forms. Com eles já carregados,
  a página leva cerca de 80 ms, com ou sem a suíte em volta.
- Sob a carga da suíte, a mesma carga do Material fica de 3 a 4 vezes mais lenta (580 → 1.900-2.430 ms): os 11
  workers começam juntos, sobem o jsdom e importam o Angular ao mesmo tempo, e o C3 está entre os primeiros arquivos
  agendados.

**Sobre as hipóteses do relato:**

- _O import do chunk da página_: o chunk em si é barato (cerca de 80 ms). O que pesa é o que ele arrasta.
- _O que o chunk arrasta_: o Angular Material (18 entradas) e os forms. Não há Chart.js.
- _`isolate: false` e o pool_: com `isolate: false`, um worker que já tivesse rodado outro spec da página teria o
  Material em cache, e o C3 levaria cerca de 100 ms. Como o C3 costuma ser um dos primeiros arquivos do seu worker, ele
  encontra o cache vazio. A ordem dos arquivos depende do sequenciador do Vitest e do histórico de durações; por isso
  o tempo varia entre execuções, mas o C3 nunca está protegido por ela.
- _O primeiro teste a importar paga a compilação_: em parte. O que se paga é a avaliação dos módulos do Material pelo
  Node no worker, e não uma compilação do esbuild; o build termina antes de qualquer teste rodar ("Application bundle
  generation complete").

**Não é regressão de código do app nem defeito das rotas.** O C3 nasceu assim no commit `ef291ee`, e a navegação dele
sempre levou ao destino certo quando não estourou o tempo. O defeito está no teste: ele mede, sem querer, o custo de
inicialização de dependências, que nos outros specs fica fora do tempo do teste.

## Correção proposta

**Preferida (A): import estático da página no topo do spec, com a asserção igual.**

```ts
import { routes } from './app.routes';
// Loads the page and its Material dependencies at import time, as the other specs do, so the test only times the navigation.
import './transactions/pages/transactions-page/transactions-page';
```

- O custo do Material passa para a fase de import do arquivo, como em todos os outros specs, e sai do tempo do teste.
- O C3 continua usando as rotas reais: o router ainda executa o `redirectTo`, o `loadChildren` e o `loadComponent`, e
  a navegação só termina com `true` se os dois carregadores resolverem. Os imports dinâmicos apenas encontram os
  módulos já carregados.
- O `it` e as duas asserções não mudam; o timeout continua o padrão (5 s).
- Experimento (temporário, já desfeito): com essa linha e o timeout do C3 forçado a 300 ms, o C3 passou sozinho e em 3
  execuções seguidas da suíte completa (363/363), com 16, 62 e 20 ms. O `eslint` aceitou o arquivo.

**Alternativas:**

- (B) Timeout maior só no C3 (por exemplo, `it(..., 20_000)`), com a asserção igual. É a menor mudança e não toca nos
  imports. Mas a causa continua lá: o teste segue gastando de 2 a 3 s numa navegação, e a margem depende da carga da
  máquina, que é exatamente o que varia.
- (C) Pré-carregar a página num `beforeAll(() => import('./transactions/pages/transactions-page/transactions-page'))`.
  Tira o custo do `it`, mas o coloca num hook com timeout próprio (10 s por padrão). Melhora a margem, mas o custo
  continua cronometrado.
- (D) Trocar o `loadComponent`/`loadChildren` real por um stub no teste. **Não recomendada**: o C3 existe para conferir
  as rotas reais de `app.routes.ts` (T014 e `research.md:272` da 003), e um stub deixaria de executar o carregador
  real. Isso enfraquece o teste, o que o princípio I proíbe.
- (E) Subir o `testTimeout` global ou reduzir os workers (via `runnerConfig`/`vitest.config`). **Não recomendada**:
  afeta a suíte inteira por causa de um teste, cria configuração nova e esconde os travamentos de `whenStable()` que
  a skill `write-front-tests` descreve como falhas por timeout.

**Arquivos que mudam:**

- `front/src/app/app.routes.spec.ts` (só ele).

**Teste de regressão e prova da correção:**

O C3 é o próprio teste de regressão. Como o estouro depende da carga da máquina, o `/speckit-bug-fix` prova a
correção com um limite apertado e determinístico, em vez de confiar em N execuções:

1. **Vermelho antes**: no spec atual, reduzir só localmente o timeout do C3 para 300 ms e rodar o arquivo sozinho:
   falha com `Test timed out in 300ms` (reproduzido nesta avaliação).
2. **Verde depois**: aplicar a correção, manter os 300 ms temporários e rodar o arquivo sozinho e a suíte completa ao
   menos 3 vezes: passa, com o C3 abaixo de 100 ms (reproduzido nesta avaliação: 16, 62 e 20 ms).
3. Voltar ao timeout padrão (remover o `300`) e registrar os números no `fix.md`. A redução para 300 ms não é
   commitada.
4. Confirmar que o C3 ainda discrimina: trocar localmente o `redirectTo: 'transactions'` de `app.routes.ts` por
   `'categories'` e ver o C3 falhar (`/categories` ≠ `/transactions`), como a 003 registrou na T014; desfazer.
5. No fim: `npx ng test --watch=false` (ao menos 3 vezes, anotando a duração do C3), `npm run lint` e
   `npm run build`.

Se o dono escolher a alternativa B, a prova muda: o passo 2 mede que o C3 continua em 2-3 s e o argumento passa a ser
a margem (20 s contra o máximo observado de 2,7 s), sem um vermelho/verde determinístico.

## Riscos e considerações

- **Cobertura do C3**: com o import estático, o C3 não cobre mais a primeira carga do chunk da página. Mas um caminho
  errado no import dinâmico já falha no build (TypeScript e esbuild), e a navegação continua executando os
  carregadores reais. O que o C3 confere (FR-002: o endereço raiz leva a `/transactions`) não muda.
- **Acoplamento**: o spec passa a citar o caminho da página. Se a página mudar de lugar, o import quebra no build do
  teste, com erro claro. O comentário explica por que o import existe, para que ninguém o remova como "não usado".
- **Outras rotas**: hoje só o C3 navega pelas rotas reais com import dinâmico. Um teste futuro que faça o mesmo (por
  exemplo, para `/reports`, que arrasta o Chart.js) terá o mesmo problema; a correção serve de modelo. Registrar isso
  na skill `write-front-tests` seria mudança fora deste bug; fica como sugestão.
- **Pendência**: documentar na skill `write-front-tests` o import por efeito (import estático da página num spec que
  navega pelas rotas reais com carga lazy), numa mudança separada, fora deste bug.
- **Medição**: a ausência de falhas nas 6 execuções desta avaliação não contradiz o relato; o estouro depende da carga
  total da máquina, que aqui foi menor. Os tempos medidos (até 54% do limite) mostram que a margem é pequena.
- **Escopo**: só o spec do front. Nenhum código de produção, contrato da API, dado ou segurança é afetado. As pastas
  `front/specs/002-*` e `003-*` não são tocadas por esta avaliação; a pendência que a 003 registra (tasks.md:392,
  plan.md:107) é atualizada pelo `/speckit-converge` da 003, e não por este bug.

## Decisões do usuário (portão do diagnóstico, 2026-10-08)

- **P1 = (A)**: import estático da página no topo do `app.routes.spec.ts`, com comentário.
- **Prova vermelho/verde local**: feita com o timeout do C3 forçado a 300 ms, que não é commitado, porque o defeito
  depende de tempo e carga. O `fix.md` registra isso como exceção aceita ao princípio I. A proteção que fica no código
  é o comentário junto ao import.
- **Verificação**: o `test.md` anota a duração do C3 em 3 ou mais execuções da suíte completa.

## Perguntas em aberto

- **P1: Qual correção aplicar?** Respondida no portão do diagnóstico (2026-10-08): (A).
  - (A) Import estático da página no topo do `app.routes.spec.ts`, com comentário. Tira a causa do tempo do teste, o
    C3 cai para menos de 100 ms e a prova é determinística (300 ms: vermelho antes, verde depois).
  - (B) Timeout de 20 s só no C3. Uma linha, sem mexer nos imports, mas o teste continua lento e dependente da carga
    da máquina.
  - (C) `beforeAll` com o import dinâmico. Tira o custo do `it`, mas o deixa cronometrado no hook (10 s).
  - **Recomendação**: (A). É a única que remove a causa (o custo de inicialização dentro do tempo do teste), mantém
    as rotas reais e a asserção igual, e permite provar a correção sem depender de sorte.
