# Checklist de contrato e dados: Filtros de transações no banco

**Objetivo**: Validar a qualidade dos requisitos de contrato da API (mudança 1.3.0, busca e ordem) e de dados (migration da collation, reversibilidade e desempenho das consultas) antes da aprovação do plan
**Criado em**: 2026-10-11
**Feature**: [spec.md](../spec.md) · [plan.md](../plan.md) (alteração de 2026-10-11)

**Nota**: Checklist gerado pelo `/speckit-checklist`, de profundidade padrão, para o revisor do plan (o usuário).
**Propriedade da revisão**: artefato de revisão da qualidade dos requisitos, de posse do revisor. Marque `[x]` só quando o revisor concluir que o critério foi atendido.
**Semântica do marcador**: `[x]` significa que o critério foi revisado e atendido quanto à qualidade dos requisitos. Não significa que a implementação está pronta.

## Contrato da API

- [x] CHK001 O FR-020 afirma que a mudança é compatível; a spec ou o plan justificam essa afirmação para a ordem alfabética, que muda para quem dependia da cultura do servidor? [Clarity, Spec §FR-020, Plan §B2]
- [x] CHK002 A regra de ordem do FR-018 está escrita no contrato 1.3.0 com o mesmo exemplo e a mesma sequência da spec, sem divergência entre os dois textos? [Consistency, Spec §FR-018, contracts/transactions-api.yaml]
- [x] CHK003 A descrição do parâmetro `description` no contrato cobre acentos, maiúsculas, espaços, `%`, `_`, `\` e transações sem descrição, como os cenários 7 a 10 da história 3? [Completeness, Spec §FR-013]
- [x] CHK004 O limite do mês (início inclusive, início do mês seguinte exclusive) aparece igual no contrato, nos FR-014 a FR-017, nos casos-limite e no cenário 6 da história 4? [Consistency, Spec §FR-014 a FR-017]
- [x] CHK005 Os FR-014 a FR-017 (as mesmas transações de antes) e a mudança do limite do mês podem ser lidos sem contradição? [Conflict, Spec §FR-014 a FR-017]
- [x] CHK006 Está definido o que o consumidor vê nas respostas de erro (`400`) da lista depois da mudança, ou a spec afirma que elas não mudam? [Completeness, Spec §FR-020]
- [x] CHK007 A posição das transações sem descrição, em cada direção, está escrita no contrato e não só na spec? [Gap, Spec §FR-018]
- [x] CHK008 A decisão de a spec 003 do front e o `back/AGENTS.md` serem atualizados depois da feature está registrada como pendência rastreável, com dono e momento? [Dependency, Spec §Premissas, Plan §Dependências e riscos da alteração]

## Dados e migration

- [x] CHK009 Os requisitos dizem que nenhuma coluna, índice ou dado existente muda, e o plan confirma isso para a migration da collation? [Completeness, Plan §data-model]
- [x] CHK010 O requisito de reversibilidade (B3) define o que "verificado" significa para a migration: aplicar, reverter e reaplicar, em qual ambiente e com qual resultado esperado? [Measurability, Plan §B3]
- [x] CHK011 Está especificado o que acontece com a migration em um PostgreSQL sem suporte a ICU (falha explícita) e quem trata essa falha? [Edge Case, Plan §Dependências e riscos]
- [x] CHK012 O plan trata a migration `AddUnaccentExtension` pendente na base de dev como pré-requisito, com a ordem de aplicação das duas migrations definida? [Dependency, Plan §Dependências e riscos]
- [x] CHK013 O requisito de ordem diz em que colunas e consultas a collation vale, e deixa claro que a comparação de igualdade e a busca não são afetadas? [Clarity, Spec §FR-018, data-model]
- [x] CHK014 O nome da collation e o provedor estão fixados em um só lugar do plan, sem divergir entre `research.md`, `data-model.md` e a estrutura do código? [Consistency]
- [x] CHK015 O plan define a regra de recuperação se a migration falhar no meio de uma implantação (por exemplo, collation criada e código antigo no ar)? [Gap, Exception Flow]

## Desempenho das consultas

- [x] CHK016 O SC-008 diz como a linha de base é obtida (antes da mudança), com quais dados e com qual estatística, de modo que dois executores obtenham a mesma conclusão? [Measurability, Spec §SC-008]
- [x] CHK017 Está decidido, ou explicitamente adiado ao usuário, se o SC-008 terá um teto absoluto de tempo além da razão de 5 vezes? [Gap, Spec §SC-008]
- [x] CHK018 O FR-019 deixa claro o comportamento esperado da busca e do filtro por tipo quando são os únicos filtros, sem prometer o que não cumpre? [Clarity, Spec §FR-019]
- [x] CHK019 O critério para criar um índice de busca no futuro (o que a medição precisa mostrar) está definido, ou fica explicitamente fora desta feature? [Gap, Plan §research Índices]
- [x] CHK020 O requisito de ordenação por nome da categoria define o custo esperado em relação à ordenação pelos outros campos, ou o aceita sem meta? [Gap, Spec §FR-018, §FR-019]
- [x] CHK021 O SC-008 cobre os dois filtros seletivos citados no FR-019 (mês e categoria) e diz o que acontece se um deles não atingir 5 vezes? [Coverage, Spec §SC-008, Plan §Dependências e riscos]

## Dependências e premissas

- [x] CHK022 A premissa de que o Docker precisa estar em execução para os testes está registrada onde o implement a verá (plan ou quickstart), com o que fazer quando não estiver? [Dependency, Plan §Dependências e riscos, quickstart]
- [x] CHK023 As premissas de volume (centenas a poucos milhares por ano, sem paginação) sustentam os alvos do SC-008 e a decisão de não indexar a busca? [Assumption, Spec §Premissas]

## Notas

- Marque os itens `[x]` só depois de a revisão confirmar o critério de qualidade do requisito.
- O `/speckit-implement` lê o estado deste arquivo como portão e não altera os marcadores.
- `checklists/requirements.md` tem ciclo próprio, mantido pelo `/speckit-specify` e pelo `/speckit-clarify`.
