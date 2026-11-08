# Checklist de contrato da API: Cor da categoria nas transações do mês

**Propósito**: validar a qualidade dos requisitos do contrato (mudança aditiva em `GET /api/reports/transactions`)
**Criado em**: 2026-11-07
**Feature**: [spec.md](../spec.md) · [contrato](../contracts/monthly-transactions-api.yaml)

**Nota**: `[x]` significa que o revisor aprovou o critério de qualidade do requisito, e não que a implementação está pronta. O `/speckit-implement` lê o estado, mas não altera as marcas.

## Completude

- [x] CHK001 O campo novo está definido com nome, tipo, formato e obrigatoriedade, em um só lugar? [Completeness, Spec §FR-001, FR-002, Contrato]
- [x] CHK002 Está dito que o campo nunca é nulo, e por quê (diferente de `color` em "Outras")? [Clarity, Spec §Premissas, Research]
- [x] CHK003 A resposta de erro (400, mês inválido) está coberta pelo contrato ou por referência explícita ao da 003? [Completeness, Contrato]

## Clareza e consistência

- [x] CHK004 O formato da cor é inequívoco (`#` + 6 dígitos hexadecimais em maiúsculas) e igual ao de `CategoryExpenseOutput.color`? [Consistency, Spec §FR-002]
- [x] CHK005 Os campos, tipos e nomes do schema novo coincidem com os de `MonthlyTransactionOutput` na 003, exceto o campo acrescentado? [Consistency, Contrato]
- [x] CHK006 "Cor atual" (FR-003) está definida em termos observáveis: o que a resposta mostra depois de a cor da categoria mudar? [Measurability, Spec §FR-003]

## Compatibilidade (princípio IV e B2)

- [x] CHK007 A mudança está classificada como aditiva, com a razão, e dizendo que nenhuma versão nova de rota é necessária? [Completeness, Spec §FR-004, Plan]
- [x] CHK008 Está definido o que não muda: rota, parâmetros, ordenação, ausência de paginação e status? [Coverage, Spec §FR-004, FR-006]
- [x] CHK009 O contrato da 003 permanece intocado, e a relação entre os dois contratos (versão 1.2.0) está explícita? [Traceability, Spec §FR-005]
- [x] CHK010 O limite do escopo (`/transactions` e demais relatórios sem o campo) está escrito de forma que um revisor possa conferi-lo? [Clarity, Spec §FR-006]

## Critérios de sucesso e dependências

- [x] CHK011 Cada SC (SC-001 a SC-003) pode ser verificado sem conhecer a implementação, e há um passo no quickstart para cada um? [Measurability, Spec §SC, Quickstart]
- [x] CHK012 Está documentado que o front só passa a depender do campo depois desta entrega, em uma feature própria? [Dependency, Spec §Premissas, Plan]

## Notas

- Itens avaliados pelo autor (agente) em 2026-11-07 a pedido do usuário, contra spec, plan, contrato e `Color.cs`; o usuário revisa a marcação. CHK005: nomes e tipos coincidem com a 003, e o contrato novo remete às descrições dela (data à meia-noite UTC, valor sempre positivo) sem repeti-las.
- 12 itens, só o tema de contrato; segurança e performance já são cobertos por FR/SC e pelo Constitution Check do plan.
