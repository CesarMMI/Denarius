# Correção do bug: teste do ChartThemeService espera tokens que o service não usa

- **Slug**: chart-theme-teste-tokens-desatualizados
- **Corrigido em**: 2026-10-06
- **Avaliação**: ./assessment.md
- **Status**: applied

## Resumo

Aplicada a combinação "P1 = A + grade `high`" do diagnóstico, conforme as decisões do dono no portão (2026-10-06):
P1 = A (linhas da comparação acumulada em `primary`/`secondary`, como no código), P2 = B (grade volta a
`surface-container-high`, o service muda) e P3 = A (os artefatos da 001 são realinhados aqui). O teste do
`ChartThemeService` passou a esperar `secondary` em `primaryVariant`, o service passou a ler `surface-container-high`
na grade, e a spec, o research, o data-model e o quickstart da 001 passaram a descrever o mesmo par de cores.

## Alterações

| Arquivo                                                      | Alteração        | Observação                                                                                                                                                                        |
| ------------------------------------------------------------ | ---------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `front/src/app/reports/services/chart-theme.service.spec.ts` | teste atualizado | `primaryVariant`: `on-primary` → `secondary` (decisão P1 = A); `grid` já esperava `surface-container-high` e ficou igual. Continua comparando os oito tokens por igualdade exata. |
| `front/src/app/reports/services/chart-theme.service.ts`      | modificado       | `grid`: `surface-container-highest` → `surface-container-high` (decisão P2 = B).                                                                                                  |
| `front/specs/001-reports-dashboard/spec.md`                  | modificado       | Nova sessão de Esclarecimentos (2026-10-06); FR-010, SC-004 e US5/AC1 reescritos.                                                                                                 |
| `front/specs/001-reports-dashboard/research.md`              | modificado       | Cores (decisão, justificativa e alternativas) e Formas e marcas.                                                                                                                  |
| `front/specs/001-reports-dashboard/data-model.md`            | modificado       | Linha "Comparação acumulada" da tabela de estado derivado (antes na linha 89).                                                                                                    |
| `front/specs/001-reports-dashboard/quickstart.md`            | modificado       | Passo 3 (linha 35).                                                                                                                                                               |
| `front/specs/001-reports-dashboard/plan.md`                  | modificado       | Resumo de cores (linha 17) cita as linhas da comparação acumulada. Ampliação pedida na revisão (R-01).                                                                            |

Nada mudou em `cumulative-comparison-chart.*`, nos outros gráficos, nos fakes de `ChartColors`, em `categories/`,
`transactions/`, `shared/`, nas specs 002/003 nem no back.

## Destaques do diff

```diff
// chart-theme.service.spec.ts
-			primaryVariant: 'var(--mat-sys-on-primary)',
+			primaryVariant: 'var(--mat-sys-secondary)',

// chart-theme.service.ts
-			grid: this.resolve('surface-container-highest'),
+			grid: this.resolve('surface-container-high'),
```

Novo texto na spec da 001:

- **Esclarecimentos, sessão 2026-10-06**: "P: Que cores usam as linhas da comparação acumulada? → R: O par da cor
  primária do tema: o mês atual em `primary` e o mês anterior em `secondary`, como foi entregue. Isso ressalva, só
  para a comparação acumulada, a resposta de 2026-10-03 de que os gráficos mantêm o vermelho; os demais gráficos e a
  lista mantêm o vermelho das despesas. A legenda e o tooltip distinguem as duas linhas. (Decisão do dono no bug
  `chart-theme-teste-tokens-desatualizados`.)"
- **FR-010**: "As despesas DEVEM aparecer no mesmo vermelho nos gráficos e na lista, exceto nas linhas da comparação
  acumulada, que DEVEM usar a cor primária do tema (`primary`) no mês atual e a secundária (`secondary`) no mês
  anterior; os cards de resumo as mostram como valores negativos, sem cor."
- **SC-004**: "100% dos valores monetários da página estão em BRL com formatação pt-BR, e 100% das despesas nos
  gráficos e na lista usam o mesmo vermelho, exceto as linhas da comparação acumulada, que usam `primary` (mês atual)
  e `secondary` (mês anterior)."
- **US5/AC1**: "**Dado** o mês atual, **Quando** o gráfico de linha é exibido, **Então** a linha do mês, na cor
  primária do tema (`primary`), para em hoje, e a linha do mês anterior, na cor secundária (`secondary`), cobre o mês
  inteiro."

Nos outros artefatos da 001:

- `research.md`, Cores: a decisão acrescenta "As linhas da comparação acumulada usam `primary` (mês atual) e
  `secondary` (mês anterior)" e mantém a grade em `surface-container-high`; a justificativa registra a escolha do dono
  (Esclarecimentos, 2026-10-06), a luminância igual das duas linhas e a coerência da grade com as divisórias da
  tabela; as alternativas registram `error`/`outline`, `primary`/`on-primary` e a grade em `surface-container-highest`.
- `research.md`, Formas e marcas: "o mês atual na cor primária (`primary`) e o mês anterior na secundária
  (`secondary`)".
- `data-model.md`: "duas linhas sobre os dias 1…max(dias), atual em `primary`, anterior em `secondary`".
- `quickstart.md`: "Linha: outubro, na cor primária do tema, para no dia 3; setembro, na cor secundária, cobre 30
  dias."

A entrada original da spec ("despesas em vermelho em todo lugar", linha 15) é a citação do pedido do usuário e ficou
como estava; a nova entrada de Esclarecimentos a ressalva.

## Testes adicionados ou atualizados

- `chart-theme.service.spec.ts` > `ChartThemeService` > `should read each color from its Material system token`:
  fixa os oito tokens que o service lê, agora com `primaryVariant` = `secondary` e `grid` = `surface-container-high`.

## Verificação local

Todos os comandos rodados em `front/`.

1. **Vermelho** (teste ajustado, service ainda sem correção):
   `npx ng test --watch=false --include='src/app/reports/services/chart-theme.service.spec.ts'`

   ```text
   - Expected
   + Received
   -   "grid": "var(--mat-sys-surface-container-high)",
   +   "grid": "var(--mat-sys-surface-container-highest)",
       "primaryVariant": "var(--mat-sys-secondary)",
    ❯ src/app/reports/services/chart-theme.service.spec.ts:29:18
    Test Files  1 failed (1)
         Tests  1 failed | 1 passed (2)
   ```

   Falha só pelo `grid`; o `primaryVariant` já coincide com o código.

2. **Verde** (service com `grid` → `surface-container-high`), mesmo comando: `Test Files 1 passed (1)`,
   `Tests 2 passed (2)`.

3. **Prova de que a asserção de `primaryVariant` discrimina** (mutação local, desfeita, sem commit): troquei no service
   `primaryVariant: this.resolve('secondary')` por `this.resolve('on-primary')` e rodei o mesmo comando:

   ```text
   -   "primaryVariant": "var(--mat-sys-secondary)",
   +   "primaryVariant": "var(--mat-sys-on-primary)",
    Test Files  1 failed (1)
         Tests  1 failed | 1 passed (2)
   ```

   Depois desfiz a troca (`primaryVariant: this.resolve('secondary')` de volta, conferido com `grep` e `git diff`: o
   service só difere do HEAD na linha do `grid`).

4. **Suíte, lint e build**:
   - `npx ng test --watch=false` → `Test Files 27 passed (27)`, `Tests 223 passed (223)`.
   - `npm run lint` → "All files pass linting."
   - `npm run build` → concluído sem aviso nem erro de budget; "Initial total" de 633,08 kB.
   - `npx prettier --check` nos arquivos alterados → sem divergência.

- **Verificação manual**: nenhuma. A troca da grade para `surface-container-high` deixa a grade um pouco mais apagada
  (1,10:1 contra 1,17:1 no claro; 1,19:1 contra 1,39:1 no escuro, segundo o diagnóstico); a conferência visual fica
  para o `/speckit-bug-test` ou para o dono.

## Desvios da avaliação

Nenhum. A combinação aplicada é a linha "A: `primary`/`secondary` | `high`" da tabela da correção proposta. A prova de
mutação foi feita para `on-primary`, como pedido; a prova para o `grid` é o próprio ciclo vermelho → verde do passo 1.

Registro sobre a `tasks.md` da 001: a T025 (concluída) diz que as linhas usam "`primary` e `on-primary`". Essa parte
da T025 foi superada pela decisão de 2026-10-06 (linhas em `primary`/`secondary`). A tarefa não foi reescrita, por ser
registro histórico de trabalho concluído.

Ampliação pedida na revisão do bug-fix (aprovada com 0 CRITICAL/HIGH/MEDIUM e 3 LOW), dentro do realinhamento da P3 = A:

- **R-01**: `plan.md`, resumo de cores, passa a dizer "…"Outras" `outline`, linhas da comparação acumulada
  `primary`/`secondary` — veja pesquisa → Cores."
- **R-02**: `spec.md`, Premissas, passa a dizer "Nas barras, as receitas aparecem no verde do tema (`tertiary`) e as
  despesas em vermelho; …", para não contradizer a exceção do FR-010.
- **R-03**: o Esclarecimento de 2026-10-06 passa a dizer "Isso ressalva, só para a comparação acumulada, a resposta de
  2026-10-03 de que os gráficos mantêm o vermelho; …" (texto citado acima, já atualizado); e o parágrafo de Cores do
  `research.md` teve a quebra de linha uniformizada (só cosmético).

Depois disso, `npx prettier --write` nos arquivos tocados (sem divergência).

## Pendências

- As duas linhas da comparação acumulada têm a mesma luminância (1,00:1 entre si); em escala de cinza ou com
  deficiência de visão de cores, só a legenda e o tooltip as distinguem. Risco aceito pelo dono na P1 = A.
- Conferir na tela, nos temas claro e escuro, se a grade em `surface-container-high` continua legível; se ficar
  apagada demais, a alternativa registrada é `surface-container-highest` (nova decisão do dono).
- Processo: a 001 entrou no `257afad` com a suíte vermelha, embora a T026 registre a suíte verde. Vale rodar a suíte
  sobre o estado commitado na revisão do implement.
- Próximo passo: `/speckit-bug-test project=front slug=chart-theme-teste-tokens-desatualizados`.
