# Verificação do bug: teste do ChartThemeService espera tokens que o service não usa

- **Slug**: chart-theme-teste-tokens-desatualizados
- **Testado em**: 2026-10-06
- **Avaliação**: ./assessment.md
- **Correção**: ./fix.md
- **Resultado**: verified

## Resumo

O cenário do bug (o teste `ChartThemeService > should read each color from its Material system token`) não falha
mais: o teste e o service concordam nos oito tokens (`primaryVariant` = `secondary`, `grid` =
`surface-container-high`). A suíte completa, o lint e o build do front passam, sem regressão. A conferência visual da
grade nos temas claro e escuro fica pendente para o dono (sem navegador nesta verificação); ela não muda o veredito
do bug, mas pode levar a uma nova decisão sobre o token da grade.

## Verificações realizadas

Todos os comandos rodados em `front/`, sobre a árvore de trabalho com a correção não commitada (diff em
`chart-theme.service.ts`, `chart-theme.service.spec.ts` e nos artefatos da 001, como descrito no `fix.md`).

| Verificação | Comando / ação | Resultado | Observações |
| --- | --- | --- | --- |
| Reprodução (depois da correção) | `npx ng test --watch=false --include='src/app/reports/services/chart-theme.service.spec.ts'` | pass | `Test Files 1 passed (1)`, `Tests 2 passed (2)`. Antes da correção, o mesmo teste falhava (saída no `assessment.md` e no passo 1 do `fix.md`). |
| Teste atualizado | o mesmo comando acima | pass | Continua comparando os oito tokens por igualdade exata (`toEqual`), sem campos removidos. |
| Diff do código | `git diff front/src/` | pass | Só duas linhas: `primaryVariant` → `secondary` no spec; `grid` → `surface-container-high` no service. |
| Tokens antigos remanescentes | `grep -rn "surface-container-highest\|on-primary" src/app/reports` | pass | Nenhuma ocorrência. |
| Suíte de regressão | `npx ng test --watch=false` | pass | `Test Files 27 passed (27)`, `Tests 223 passed (223)`; antes da correção, 1 falhava. |
| Lint | `npm run lint` | pass | "All files pass linting." |
| Build e budgets | `npm run build` | pass | Sem aviso nem erro de budget; "Initial total" 633,08 kB (aviso em 700 kB); chunk `reports-page` 24,43 kB. |
| Formatação | `npx prettier --check` nos dois arquivos de código | pass | "All matched files use Prettier code style!" |
| Conferência visual da grade | manual, no navegador (passos abaixo) | not-run | Sem navegador nesta verificação; pendente para o dono. |

## Trechos da saída

```text
# teste do bug
 Test Files  1 passed (1)
      Tests  2 passed (2)

# suíte completa
 Test Files  27 passed (27)
      Tests  223 passed (223)

# lint
All files pass linting.

# build
                    | Initial total       | 633.08 kB |               148.67 kB
chunk-IBZ77VEV.js   | reports-page        |  24.43 kB |                 6.29 kB
Application bundle generation complete.
```

## Verificação manual pendente (dono)

A correção deixa a grade dos gráficos um pouco mais apagada (`surface-container-high`: 1,10:1 contra 1,17:1 no
claro; 1,19:1 contra 1,39:1 no escuro, segundo o diagnóstico). Passos:

1. Em `back/`, subir a API; em `front/`, rodar `npm start` e abrir a página de relatórios.
2. No tema claro, conferir no gráfico de comparação acumulada e no de receitas x despesas se as linhas de grade
   continuam visíveis e legíveis.
3. Trocar para o tema escuro e repetir o passo 2.
4. Na comparação acumulada, conferir também que a linha do mês atual (`primary`) e a do mês anterior (`secondary`)
   aparecem e que a legenda e o tooltip as distinguem.

Se a grade ficar apagada demais, a alternativa registrada é `surface-container-highest`, o que exige nova decisão do
dono e um novo ajuste no service, no teste e no `research.md` da 001.

## Riscos residuais

- A legibilidade da grade em `surface-container-high` não foi conferida na tela (verificação manual acima).
- As duas linhas da comparação acumulada têm a mesma luminância (1,00:1 entre si); em escala de cinza ou com
  deficiência de visão de cores, só a legenda e o tooltip as distinguem. Risco aceito pelo dono na P1 = A.
- A verificação rodou sobre a árvore de trabalho não commitada; o estado commitado deve ser conferido de novo depois
  do commit (a 001 entrou no `257afad` com a suíte vermelha, como registrado no diagnóstico).

## Recomendação

Encerrar o bug: o teste que falhava passa, e a suíte (223 testes), o lint e o build do front estão verdes, sem aviso
de budget. Antes de fechar, o dono confere a grade nos temas claro e escuro (passos acima); se a achar apagada demais,
abre-se nova decisão para voltar a `surface-container-highest`. Depois do commit, rodar `npx ng test --watch=false`
de novo sobre o estado commitado.
