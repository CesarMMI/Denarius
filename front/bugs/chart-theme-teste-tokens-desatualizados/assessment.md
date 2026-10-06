# Avaliação do bug: teste do ChartThemeService espera tokens que o service não usa

- **Slug**: chart-theme-teste-tokens-desatualizados
- **Projeto**: front
- **Criada em**: 2026-10-05
- **Origem**: texto colado (relato do orquestrador)
- **Veredito**: válido
- **Severidade**: medium

## Relato (resumido)

> A suíte completa do front falha em 1 de 223 testes, com a árvore de trabalho limpa no HEAD `a93db31`:
> `front/src/app/reports/services/chart-theme.service.spec.ts` (por volta da linha 29) espera, por exemplo,
> `primaryVariant: 'var(--mat-sys-on-primary)'` e `--mat-sys-surface-container-high`, mas o service
> (`chart-theme.service.ts` ~linha 41) resolve `--mat-sys-secondary` e `-highest`. A suspeita é o commit `0b008af`
> ("style(front): align the report charts with the summary cards"), que pode ter mudado o service sem atualizar o
> teste — ou introduzido uma regressão no service.

## Sintoma

`npx ng test --watch=false` termina com 1 teste falhando: `ChartThemeService > should read each color from its
Material system token`. O teste espera `primaryVariant` = `on-primary` e `grid` = `surface-container-high`; o service
devolve `secondary` e `surface-container-highest`. Os outros seis tokens batem. O esperado é a suíte verde, com o
teste, o service e os artefatos da 001 descrevendo as mesmas cores.

## Reprodução

1. `git checkout a93db31` (árvore limpa; as pastas não rastreadas `front/specs/002-*` e `003-*` não afetam a suíte).
2. Em `front/`, rodar `npx ng test --watch=false`.
3. Saída (rodada em 2026-10-05):

```text
 FAIL  denarius  src/app/reports/services/chart-theme.service.spec.ts > ChartThemeService > should read each color from its Material system token
AssertionError: expected { …(8) } to deeply equal { …(8) }

-   "grid": "var(--mat-sys-surface-container-high)",
+   "grid": "var(--mat-sys-surface-container-highest)",
-   "primaryVariant": "var(--mat-sys-on-primary)",
+   "primaryVariant": "var(--mat-sys-secondary)",

 ❯ src/app/reports/services/chart-theme.service.spec.ts:29:18

 Test Files  1 failed | 26 passed (27)
      Tests  1 failed | 222 passed (223)
```

## Caminhos de código suspeitos

Código e testes:

- `front/src/app/reports/services/chart-theme.service.spec.ts:29-38`: a expectação com `on-primary` (linha 34) e
  `surface-container-high` (linha 36).
- `front/src/app/reports/services/chart-theme.service.ts:41,43`: `primaryVariant: this.resolve('secondary')` e
  `grid: this.resolve('surface-container-highest')`.
- `front/src/app/reports/components/cumulative-comparison-chart/cumulative-comparison-chart.ts:64-72`: o único
  consumidor de `primary` e `primaryVariant` (a linha do mês atual e a do mês anterior).
- `cumulative-comparison-chart.spec.ts:63-67`: o teste do componente, que afirma "o mês na cor primária e o anterior
  na variante", com os fakes `amber`/`brown`.
- `cumulative-comparison-chart.ts:86-89` e `income-vs-expense-chart.ts:67-70`: os consumidores de `grid`.

Artefatos da 001 (`front/specs/001-reports-dashboard/`) que descrevem as cores das linhas e da grade:

- `spec.md:135-136` (US5, AC1): "a linha do mês, em vermelho, para em hoje, e a linha do mês anterior, numa cor
  neutra, cobre o mês inteiro". A entrada da spec também pede "despesas em vermelho em todo lugar" (`spec.md:15`).
- `spec.md:202-203` (FR-010, obrigatório): "As despesas DEVEM aparecer no mesmo vermelho nos gráficos e na lista".
  A comparação acumulada é um gráfico de despesas.
- `spec.md:225-226` (SC-004): "100% das despesas nos gráficos e na lista usam o mesmo vermelho".
- `spec.md:28-29` (Esclarecimentos, sessão 2026-10-03): "os gráficos e a lista mantêm o vermelho".
- `spec.md:207` (FR-012): "A página DEVE seguir os temas claro e escuro da aplicação".
- `data-model.md:89`: "duas linhas […], atual em vermelho, anterior neutra".
- `quickstart.md:35`: "outubro, em vermelho, para no dia 3; setembro, numa cor neutra, cobre 30 dias".
- `research.md:154` (Formas e marcas): "o mês atual na cor de destaque (vermelho) e o mês anterior na neutra". A
  "neutra" é `outline` (`research.md:86`).
- `research.md:87` (Cores): "As linhas de grade usam `surface-container-high`".
- `tasks.md:203-206` (T025): alinhou os specs às "mudanças posteriores do dono", entre elas as "cores dos gráficos
  (receita `tertiary`, linhas `primary` e `on-primary`, texto `on-surface`)".

## Hipótese de causa raiz

**O commit `0b008af` não é a causa.** Ele só altera `reports-page.scss` (4 linhas: as colunas e as áreas da grade da
página). O service e o seu spec só aparecem num commit, `257afad` ("feat(front): add the reports dashboard"), e já
nasceram divergentes nele (`git log -p` dos dois arquivos). A falha existe desde a entrega da 001, e não é regressão
de commit posterior.

**A divergência tem três vias para as linhas da comparação acumulada:**

| Fonte | Mês atual | Mês anterior |
| --- | --- | --- |
| `spec.md` US5/AC1, FR-010, SC-004 e Esclarecimentos (2026-10-03); `data-model.md:89`, `quickstart.md:35`, `research.md:154` | vermelho (`error`) | neutra (`outline`) |
| `tasks.md` T025 ("mudanças posteriores do dono") e `chart-theme.service.spec.ts` | `primary` | `on-primary` |
| Código (`chart-theme.service.ts`, `cumulative-comparison-chart.ts`) | `primary` | `secondary` |

A grade tem duas vias: `research.md:87` e o teste dizem `surface-container-high`, e o código diz
`surface-container-highest`.

**Sequência provável (inferência, sem registro direto).** A T025 registra a troca de vermelho/neutra para
`primary`/`on-primary` como decisão do dono, e a T026 registra 223 testes passando. Hoje a suíte tem os mesmos 223
testes, com 1 falhando. Então, depois da última execução registrada da suíte e antes do commit `257afad`, o service
passou para `secondary` e `highest`. Nenhum commit, tarefa ou achado de validação registra essa troca nem o motivo
dela, e o teste e os artefatos não acompanharam. Também não há registro de validação visual depois dela: a
verificação visual registrada (T019 e "Achados da validação") é anterior à T025.

**O que a evidência sustenta sobre cada candidato para as linhas.** O fundo é a superfície do card (`mat-card`, cujo
contêiner é `surface-container-low`: N96 `#f4f3f3` no claro, N10 `#1a1c1c` no escuro). Valores calculados com a paleta
de `front/src/material-theme.scss`:

| Par (atual / anterior) | Contraste de cada linha com o card (claro; escuro) | Contraste entre as duas linhas (claro; escuro) |
| --- | --- | --- |
| `primary` / `on-primary` (T025 e teste) | 5,84 e **1,11**; 10,05 e **1,31** | — (a anterior quase some) |
| `primary` / `secondary` (código) | 5,84 e 5,85; 10,05 e 10,06 | **1,00; 1,00** (só o matiz as distingue: âmbar e marrom) |
| `error` / `outline` (spec) | 5,86 e 4,07; 10,07 e 5,40 | 1,44; 1,86 (e matizes distintos: vermelho e cinza-acastanhado) |

- `on-primary` sobre o card é praticamente invisível nos dois temas (P100 branco sobre N96; P20 sobre N10). Isso
  compromete o US5/AC1, que pede a linha do mês anterior cobrindo o mês inteiro (o que pressupõe uma linha visível),
  e o FR-012, porque nenhum dos dois temas a mostra. No Material 3,
  `on-primary` é a cor do conteúdo sobre um preenchimento `primary`, e não uma cor de marca sobre a superfície.
- `secondary` é visível, mas, junto com `primary`, também não cumpre o US5/AC1 (pede vermelho e neutra), o FR-010
  nem o SC-004 (despesas no mesmo vermelho nos gráficos). Além disso, tem a mesma luminância de `primary`.
- `error`/`outline` é o único par que cumpre a spec de registro.
- Indício fraco: os specs dos componentes usam os fakes `primary: 'amber'` / `primaryVariant: 'brown'`. Eles já
  existiam no `257afad`, junto com a T025, e `on-primary` no escuro (P20 `#462b00`) também é marrom. Não discriminam
  entre `secondary` e `on-primary`.

**Grade.** A favor de `surface-container-high`: é a decisão escrita (`research.md:87`) e o mesmo token das divisórias
da tabela (`row-item-outline-color` em `front/src/material.scss`). A favor de `surface-container-highest`: é o que está
no código, e as duas opções são linhas finíssimas, com `highest` um pouco mais visível (1,17:1 contra 1,10:1 no claro;
1,39:1 contra 1,19:1 no escuro). Nenhum registro diz por que o código usa `highest`.

**Conclusão.** O teste falha porque o service e o teste divergem desde o `257afad`. Nenhum dos dois coincide com a
spec de registro da 001. Confiança alta em um ponto: `on-primary` (o que o teste espera) é inviável como cor de linha.
Qual par é o certo (`primary`/`secondary` ou `error`/`outline`) e qual token de grade usar são decisões do dono, sem
evidência que decida sozinha: perguntas P1 e P2.

**É bug, e não comportamento novo.** As cores fazem parte da 001, já entregue. O que falta é coerência entre a spec,
o service e o teste. Se a resposta à P1 for (A), a spec de registro passa a descrever o que foi entregue. É um
realinhamento de artefato, sem funcionalidade nova, mas muda requisito (FR-010), critério de sucesso (SC-004) e
cenário (US5/AC1), além de um Esclarecimento já registrado (o que a P3 trata).

**Princípio I.** Qualquer que seja a resposta, o teste continua comparando os oito tokens (ou os que restarem, na
opção B) por igualdade exata, sem `objectContaining`, `skip` nem campos removidos para passar. A expectativa muda
porque a decisão do dono no portão define o comportamento pretendido, e essa decisão fica registrada (P3). Isso não
enfraquece o teste. Atualizar a expectativa sem essa decisão seria só fazer o teste concordar com o código.

## Correção proposta

Resumo por combinação de respostas:

| P1 (linhas) | P2 (grade) | Service / componente | Testes |
| --- | --- | --- | --- |
| A: `primary`/`secondary` | `highest` | sem mudança | `chart-theme.service.spec.ts`: `primaryVariant` → `secondary`, `grid` → `highest` |
| A: `primary`/`secondary` | `high` | service: `grid` → `high` | `chart-theme.service.spec.ts`: `primaryVariant` → `secondary` |
| B: `error`/`outline` | qualquer | `cumulative-comparison-chart.ts` usa `colors.expense` / `colors.neutral`; `primary` e `primaryVariant` saem de `ChartColors` e do service (ficariam sem uso); `grid` conforme a P2 | `cumulative-comparison-chart.spec.ts` passa a esperar as cores de despesa e neutra; `chart-theme.service.spec.ts` e os fakes de `ChartColors` (4 specs) perdem os dois campos; `grid` conforme a P2 |
| C: `primary`/`on-primary` | qualquer | service: `primaryVariant` → `on-primary`; `grid` conforme a P2 | nenhum, além do `grid` se P2 = `highest` |

Artefatos da 001, conforme a P3:

- Se P1 = A: no `spec.md`, o FR-010 e o SC-004 passam a excetuar as linhas da comparação acumulada do "mesmo
  vermelho"; o US5/AC1 passa a dizer `primary`/`secondary`; e o Esclarecimento de 2026-10-03 ("os gráficos e a lista
  mantêm o vermelho") é ressalvado por uma nova entrada em Esclarecimentos. O `data-model.md:89`, o
  `quickstart.md:35` e o `research.md` (Cores e Formas e marcas) também passam a dizer `primary`/`secondary`.
- Se P1 = B: a spec já está certa; só a T025 fica como registro histórico, citada no `fix.md`.
- Se P2 = `highest`: o `research.md:87` muda.

Arquivos que podem mudar: `chart-theme.service.ts` e `.spec.ts`, `cumulative-comparison-chart.ts` e `.spec.ts`, os
fakes de `ChartColors` em `expenses-by-category-chart.spec.ts`, `income-vs-expense-chart.spec.ts` e
`reports-page.spec.ts`, e os artefatos da 001 listados acima.

**Teste de regressão previsto:**

- O vermelho atual (`chart-theme.service.spec.ts:29`) é a reprodução.
- Combinações em que o código muda (A com `high`, B, C): o teste atualizado ou novo falha contra o código atual antes
  da correção, e é o ciclo vermelho → verde clássico. Em B, o vermelho é `cumulative-comparison-chart.spec.ts`
  esperando a cor de despesa e a neutra.
- A com `highest`: a correção está só no teste. Para provar que a asserção discrimina, o `/speckit-bug-fix` troca
  temporariamente, só no local, o service para `on-primary` e depois para `high`, confere que o teste falha em cada
  caso, desfaz e registra isso no `fix.md`.
- Em todos os casos, no fim: `npx ng test --watch=false`, `npm run lint` e `npm run build`.

## Riscos e considerações

- **Visual**: A com `highest` não muda a tela. A com `high` deixa a grade mais apagada. B troca as cores das linhas
  (âmbar/marrom → vermelho/neutra). C deixa a linha do mês anterior invisível.
- **Distinção entre as linhas**: `primary` e `secondary` têm luminância idêntica (1,00:1 entre si) e matizes
  próximos. Em escala de cinza ou com deficiência de visão de cores, só a legenda e o tooltip as distinguem. O par
  `error`/`outline` tem diferença de luminância de 1,44:1 (claro) e 1,86:1 (escuro), além do matiz.
- **Processo**: a T026 registra a suíte verde, mas o commit `257afad` já entrou com ela vermelha, e a troca do service
  não deixou registro. A revisão da 001 não rodou a suíte sobre o estado commitado.
- **Escopo**: só o front. Nenhum contrato da API, dado ou segurança é afetado.

## Perguntas em aberto

- **P1 — Cores das linhas da comparação acumulada**: [NEEDS CLARIFICATION: qual par as linhas usam?]
  - (A) Manter `primary`/`secondary`, como no código. Só o teste e os artefatos da 001 mudam, e a tela fica igual.
    Mas essa opção reescreve um requisito obrigatório (FR-010), um critério de sucesso (SC-004), o cenário US5/AC1 e
    um Esclarecimento já registrado, e as duas linhas continuam com luminância idêntica.
  - (B) Voltar à spec: `error` no mês atual e `outline` no anterior. Mudam o componente, o service (os dois campos
    saem), os fakes e a tela; os artefatos da 001 já descrevem isso. Desfaz a mudança que a T025 registra como do
    dono.
  - (C) `primary`/`on-primary`, como na T025 e no teste. Só o service muda, mas a linha do mês anterior fica
    invisível (1,11:1 no claro), o que viola o US5.
  - **Recomendação**: (B). É o único par que cumpre a spec aprovada (FR-010, SC-004 e US5/AC1) sem reescrever
    requisito, critério de sucesso ou cenário. Usa o vermelho que a spec exige para despesas nos gráficos e distingue
    melhor as duas linhas. Se o dono
    confirmar que preferiu o par da cor primária (T025) e aceitar a luminância igual, (A) é a menor mudança.
- **P2 — Token da grade**: [NEEDS CLARIFICATION: `surface-container-high` ou `surface-container-highest`?]
  - (A) `highest`, como no código: o teste e o `research.md:87` mudam, e a tela fica igual. É um pouco mais visível
    (1,17:1 contra 1,10:1 no claro).
  - (B) `high`, como no `research.md` e no teste: o service muda e a grade fica mais apagada. É coerente com as
    divisórias da tabela.
  - **Recomendação**: (B). É a decisão escrita e coerente com a tabela, e nenhum registro justifica a troca. Prefira
    (A) se o dono achar a grade apagada demais na tela.
- **P3 — Onde realinhar os artefatos da 001** (`spec.md` FR-010, SC-004, US5/AC1 e Esclarecimentos;
  `data-model.md:89`, `quickstart.md:35`; `research.md` em Cores e em Formas e marcas): [NEEDS CLARIFICATION: no
  bug-fix ou num ciclo à parte?]
  - (A) No próprio `/speckit-bug-fix`, registrado no `fix.md`. Se mudar requisito, critério de sucesso ou cenário
    (P1 = A), a decisão tomada neste portão entra também numa nova entrada de Esclarecimentos da spec da 001, que
    ressalva a de 2026-10-03.
  - (B) Num ciclo à parte (`/speckit-clarify` da 001 ou fluxo de features), com o bug-fix tocando só o teste e o
    service/componente.
  - **Recomendação**: (A). Com P1 = B e P2 = B, sobra pouco a realinhar (só o registro da T025 no `fix.md`). Com
    P1 = A, a decisão já foi tomada pelo dono neste portão, e um ciclo à parte só repetiria a pergunta e deixaria os
    artefatos divergentes até lá. Mas, por mexer em FR-010 e SC-004, a P1 = A é o caso em que (B) se justifica, se o
    dono preferir que a mudança de requisito passe pelo `/speckit-clarify` e pelo `/speckit-analyze` da 001.
