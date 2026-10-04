# Quickstart: Validating the Reports Dashboard

## Prerequisites

- Node 24 and the dependencies installed (`npm ci` in `front/`)
- For the manual check: the backend reports API running on `http://localhost:5276` with data — the
  backend quickstart's scratch database and seed
  ([back/specs/003-financial-reports/quickstart.md](../../../back/specs/003-financial-reports/quickstart.md),
  steps 3–4) give a known month history.

## 1. Automated validation

From `front/`:

```
npx ng test --watch=false --include='src/app/reports/**/*.spec.ts'
npx ng test --watch=false
npm run lint
npm run build
```

Expected: every spec passes (the reports specs and every existing one, including `app.spec.ts` with the
new link); lint is clean; the build reports no budget warning, and the "Initial total" stays where it was
before the feature (~630 kB) — Chart.js appears only in the lazy chunk of the reports route.

## 2. Manual check

Start the front with `npm start` and open `http://localhost:4200/reports` (or "Relatórios" in the menu).
With the backend seed and today on 2026-10-03:

1. The page opens on `10/2026`. Five cards: Saldo R$ 5.790,00 (↑ 106,8% vs. setembro, green); Receitas
   R$ 8.000,00 (0% vs. setembro); Despesas -R$ 2.210,00, without red (↓ 57,5% vs. setembro, green:
   spending went down); Taxa de poupança 72,4%; Projeção -R$ 2.170,00, saldo projetado R$ 5.830,00.
2. Bars: twelve months from nov. 2025 to out. 2026, with empty months in the middle; tooltips in BRL with
   the month spelled out.
3. Line: October in red stops at day 3; September in a neutral color covers 30 days.
4. Click "Mês anterior" (or pick `09/2026`): every block reloads once; the doughnut shows four categories
   and "Outras"; the list shows the ten newest of September's twelve transactions ("Transações do mês
   (12)"), expenses in red. "Ver todas" opens the transactions page with the filters shown on `09/2026`,
   listing all twelve.
5. Pick `03/2026`: the summary and the list say "Sem movimentações neste mês.", the doughnut "Sem despesas
   neste mês.", the line "Sem despesas neste mês nem no anterior.", and the bars still show the year.
6. Stop the API and click "Recarregar": each block shows its error and "Tentar novamente"; start the API
   again and retry one block — only that block reloads.
7. At 1366×768 or more nothing scrolls but the list; narrowing the window under 1200 px stacks the blocks
   and the page scrolls. Switching the system to dark mode repaints the charts with the dark theme.
