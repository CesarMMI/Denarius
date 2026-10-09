# Checklist do contrato da API: Relatórios financeiros (alteração de 2026-10-09)

**Objetivo**: Avaliar se os requisitos e o contrato descrevem com clareza a mudança de significado da série acumulada
e da projeção do mês atual, sem mudança de formato (versão 1.1.0)
**Criado em**: 2026-10-09
**Feature**: [spec.md](../spec.md), [contracts/reports-api.yaml](../contracts/reports-api.yaml)

**Responsabilidade**: Este checklist é do revisor e avalia a qualidade dos requisitos. Um item só é marcado `[x]`
quando o revisor conclui que o critério foi atendido; `[x]` não quer dizer que a implementação está pronta.

## Completude

- [x] CHK001 O contrato descreve até onde vai a série acumulada nos três casos (mês passado, mês atual e mês futuro)? [Completude, Spec §FR-013]
- [x] CHK002 A descrição de `projectedExpense` no contrato inclui a parcela das despesas com data depois de hoje e diz como ela é arredondada? [Completude, Spec §FR-006, §FR-015]
- [x] CHK003 Está documentado que `projectedBalance` passa a refletir a nova despesa projetada, sem outra mudança de regra? [Completude, Spec §FR-006]
- [x] CHK004 A mudança de versão do contrato (1.0.0 → 1.1.0) traz o motivo junto da versão? [Completude, Gap]

## Clareza

- [x] CHK005 "Último dia com despesa do mês" está definido sem ambiguidade, deixando claro que receitas não contam? [Clareza, Spec §FR-013]
- [x] CHK006 Fica claro que a regra do mês atual vale também para `previousMonth` quando o mês anterior é o mês atual? [Clareza, Spec §FR-013, Premissas]
- [x] CHK007 Está claro que uma despesa de hoje entra no ritmo, e não na parcela "depois de hoje", para não ser contada duas vezes? [Clareza, Spec §Premissas]

## Consistência

- [x] CHK008 As descrições do contrato concordam com o FR-006, o FR-012 e o FR-013 da spec e com o data-model? [Consistência]
- [x] CHK009 Os exemplos do quickstart (projeção de 4.170 e série até o dia 10) concordam com as regras novas? [Consistência, Quickstart §4]

## Compatibilidade e consumidores

- [x] CHK010 A spec ou o plan justificam que a mudança é compatível (formato, campos obrigatórios e status codes iguais), conforme o B2? [Consistência, Plan §Constitution Check da alteração]
- [x] CHK011 Está identificado o impacto no consumidor, com o front desenhando a série devolvida, e o que fica desatualizado do lado dele? [Dependência, Plan §Alteração]

## Casos-limite

- [x] CHK012 Estão cobertos o dia 1º sem despesa até hoje, o último dia do mês e uma despesa no último dia do mês? [Cobertura, Spec §Premissas, §História 4]
- [x] CHK013 Está dito que despesas de meses seguintes não entram na série nem na projeção do mês atual? [Cobertura, Spec §Premissas]

## Notas

- Marque `[x]` só depois de a revisão confirmar o critério.
- Deixe desmarcado o item que ainda pedir esclarecimento ou correção.
