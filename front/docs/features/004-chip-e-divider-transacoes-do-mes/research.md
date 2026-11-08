# Pesquisa: Chip colorido e divisores por dia em "Transações do mês"

## Chip

- **Decisão**: `<mat-chip [appMatChipColor]="transaction.color">` com `description ?? categoryName`, importando `MatChipsModule` e `MatChipColor`, como em `transactions-table.html`.
- **Motivo**: a diretiva já resolve os tons claro e escuro com contraste AAA; é o mesmo visual da tabela de Transações (FR-001), sem código novo de cor (F4, V).
- **Texto**: `description ?? categoryName` basta porque o back normaliza descrição vazia ou em branco para `null` (`back/src/Denarius.Domain/Entities/Transaction.cs`, `ValidateDescription`); não surge chip vazio.
- **Alternativas**: pintar o fundo com a cor crua (contraste incerto, diverge da tabela); novo componente de chip (abstração sem necessidade).

## Cabimento na coluna (FR-006)

- **Decisão**: o texto vai num `<span class="label">` dentro do chip, e o CSS do componente aplica `max-width: 100%` ao chip e `min-width: 0`, `overflow: hidden`, `text-overflow: ellipsis`, `white-space: nowrap` ao `.label`, substituindo a regra `.mat-column-description div`. Não se usa seletor de classe interna do Material (F4). A `td` tem `max-width: 0`, o chip é `inline-flex`: o conjunto só se comprova na verificação visual.
- **Motivo**: hoje o corte está num `div`; o chip é `inline-flex` e não encolhe sozinho. O jsdom não mede layout nem garante que o SCSS do componente seja aplicado, então o teste confere só a estrutura (texto em `span.label` dentro do chip), e o corte em si é verificação visual (SC-003, quickstart).
- **Risco**: o orçamento de estilos por componente (aviso 4 kB, erro 8 kB) não é afetado por poucas linhas de CSS.

## Divisor (FR-005)

- **Decisão**: um `computed` `sameDate` sobre `latest()`: o conjunto das linhas cuja seguinte tem o mesmo `date.slice(0, 10)`, e `[class.same-date]` no `tr[mat-row]` com `--mat-table-row-item-outline-color: transparent`, igual a `transactions-table`.
- **Motivo**: a data é exibida em UTC (`'dd/MM' : 'UTC'`), então o dia exibido é o `slice(0, 10)` do ISO; a lista é de um mês, então dia e mês bastam. Calcular sobre `latest()` garante que o divisor segue as 10 linhas exibidas (SC-002).
- **Alternativa rejeitada**: extrair a regra para `shared/` e usá-la nos dois componentes. Obrigaria mexer em `transactions-table` (FR-008) por uma linha de lógica (V); duplicar a linha é o menor custo. Se um terceiro uso aparecer, extrai-se.

## Tipo e fixture

- **Decisão**: `MonthlyTransaction.color: string` (não nulo, como o contrato); `buildMonthlyTransaction` ganha `color: '#43A047'` por padrão.
- **Motivo**: F2 (tipos espelham o contrato, sem campos supostos); o `expenses-by-category` usa `string | null` porque o contrato dele permite "Outras", o que não vale aqui.
