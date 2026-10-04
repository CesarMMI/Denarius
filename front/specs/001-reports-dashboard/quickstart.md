# Guia rápido: validação do painel de relatórios

## Pré-requisitos

- Node 24 e as dependências instaladas (`npm ci` em `front/`)
- Para a verificação manual: a API de relatórios do backend rodando em `http://localhost:5276` com dados — o banco
  descartável e o seed do quickstart do backend
  ([back/specs/003-financial-reports/quickstart.md](../../../back/specs/003-financial-reports/quickstart.md),
  passos 3–4) dão um histórico de meses conhecido.

## 1. Validação automatizada

A partir de `front/`:

```
npx ng test --watch=false --include='src/app/reports/**/*.spec.ts'
npx ng test --watch=false
npm run lint
npm run build
```

Esperado: todos os specs passam (os de relatórios e todos os existentes, incluindo o `app.spec.ts` com o novo link);
o lint passa limpo; o build não emite aviso de budget, e o "Initial total" fica onde estava antes da feature
(~630 kB) — o Chart.js só aparece no chunk lazy da rota de relatórios.

## 2. Verificação manual

Suba o front com `npm start` e abra `http://localhost:4200/reports` (ou "Relatórios" no menu). Com o seed do backend
e hoje em 2026-10-03:

1. A página abre em `10/2026`. Cinco cards: Saldo R$ 5.790,00 (↑ 106,8% vs. setembro, verde); Receitas R$ 8.000,00
   (0% vs. setembro); Despesas -R$ 2.210,00, sem vermelho (↓ 57,5% vs. setembro, verde: o gasto caiu); Taxa de
   poupança 72,4%; Projeção -R$ 2.170,00, saldo projetado R$ 5.830,00.
2. Barras: doze meses, de nov. 2025 a out. 2026, com meses vazios no meio; tooltips em BRL com o mês por extenso.
3. Linha: outubro, em vermelho, para no dia 3; setembro, numa cor neutra, cobre 30 dias.
4. Clique em "Mês anterior" (ou escolha `09/2026`): todos os blocos recarregam uma vez; a rosca mostra quatro
   categorias e "Outras"; a lista mostra as dez mais recentes das doze transações de setembro ("Transações do mês
   (12)"), com as despesas em vermelho. "Ver todas" abre a página de transações com os filtros visíveis em `09/2026`,
   listando as doze.
5. Escolha `03/2026`: o resumo e a lista dizem "Sem movimentações neste mês.", a rosca, "Sem despesas neste mês.", a
   linha, "Sem despesas neste mês nem no anterior.", e as barras continuam mostrando o ano.
6. Pare a API e clique em "Recarregar": cada bloco mostra o seu erro e "Tentar novamente"; suba a API de novo e tente
   de novo um bloco — só esse bloco recarrega.
7. Em 1366×768 ou mais, nada rola além da lista; estreitar a janela abaixo de 1200 px empilha os blocos, e a página
   rola. Mudar o sistema para o modo escuro redesenha os gráficos com o tema escuro.
