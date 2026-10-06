# Checklist de qualidade da especificação: Gestão de transações

**Objetivo**: Validar a completude e a qualidade da especificação antes de seguir para o planejamento
**Criado em**: 2026-10-04
**Feature**: [spec.md](../spec.md)

## Qualidade do conteúdo

- [x] Sem detalhes de implementação (linguagens, frameworks, APIs)
- [x] Focada no valor para o usuário e nas necessidades do negócio
- [x] Escrita para partes interessadas não técnicas
- [x] Todas as seções obrigatórias preenchidas

## Completude dos requisitos

- [x] Nenhum marcador [NEEDS CLARIFICATION] restante
- [x] Os requisitos são testáveis e sem ambiguidade
- [x] Os critérios de sucesso são mensuráveis
- [x] Os critérios de sucesso não dependem de tecnologia (sem detalhes de implementação)
- [x] Todos os cenários de aceitação estão definidos
- [x] Os casos-limite estão identificados
- [x] O escopo está claramente delimitado
- [x] Dependências e premissas identificadas

## Prontidão da feature

- [x] Todos os requisitos funcionais têm critérios de aceitação claros
- [x] Os cenários de usuário cobrem os fluxos principais
- [x] A feature atinge os resultados mensuráveis definidos nos critérios de sucesso
- [x] Nenhum detalhe de implementação vaza para a especificação

## Observações

- Esta é uma especificação **retroativa**: documenta a página de transações como já implementada em
  `front/src/app/transactions` (com as partes de `categories/` e de `shared/` que ela usa e o menu e as rotas de
  `app.ts`, `app.html` e `app.routes.ts`) e coberta pelos testes existentes (`transactions-page.spec.ts`,
  `transactions-table.spec.ts`, `transactions-filters.spec.ts`, `transaction-form.spec.ts`,
  `transactions.service.spec.ts` e os de `shared/` e `app.spec.ts`), e não descreve trabalho por fazer.
- As regras de validação, de filtro e de ordenação do backend aparecem como o que o usuário observa na página, citando
  [[002-transaction-management]], sem copiar aquela spec.
- A "API" aparece só como a origem dos dados e das mensagens de erro, e como o lugar onde a filtragem e a ordenação
  acontecem (FR-031, exigência de desempenho da constituição); a spec não cita rotas da API, formatos nem bibliotecas.
- `/transactions`, `?categoryId=` e `?month=YYYY-MM` ficam nos requisitos porque são endereços públicos da aplicação
  (F2 em `front/AGENTS.md`), que outras páginas e os favoritos do usuário usam.
- Os textos que o usuário vê (rótulos, mensagens e durações das mensagens) estão citados entre aspas porque são
  comportamento verificado no código e, na maioria, nos testes. Ficam só no código, sem teste, por exemplo, as
  mensagens "Informe uma data válida" e "Escolha uma categoria", a rolagem só da lista (FR-011) e a exibição como texto
  (SC-008).
- Nenhum marcador [NEEDS CLARIFICATION]: todo o comportamento foi inferido do código e dos testes. Onze casos-limite
  marcados _(a confirmar)_ descrevem o comportamento atual que talvez não seja intencional: transação salva fora dos
  filtros em uso; agrupamento por data em qualquer ordenação; valor sem limite no formulário (um único marcador cobre o
  zero, a alteração silenciosa de valores com 14 ou mais algarismos inteiros e a recusa a partir de cerca de 10^16);
  diálogo que fecha antes da resposta da API; data digitada fora do dia/mês/ano exibido; formulário sem categorias;
  calendários anunciados em inglês; dia escolhido na visão de dias do filtro "Mês" (suspeito, deduzido do código e não
  reproduzido); filtros ocultos que continuam valendo; `categoryId` inválido no endereço; e endereço que só define os
  filtros iniciais. Eles não tornam os requisitos ambíguos, porque o comportamento está descrito como é; a decisão sobre
  manter ou corrigir cada um fica com o usuário, no `/speckit-clarify`.
- A primeira validação pediu ajustes de redação: dois casos-limite repetiam FR-029 e FR-032, e o SC-006 podia ser lido
  como se proibisse mostrar a mensagem da API. Depois deles, todos os itens passaram na segunda validação.
- 2026-10-04, depois da revisão independente (nenhum achado CRITICAL ou HIGH): o SC-002 passou a valer só para a
  transação que atende aos filtros em uso, e o SC-003 só reconhece a categoria nas linhas cuja categoria está entre as
  carregadas. Os dois deixaram de contradizer os casos-limite. Também mudaram o carregamento (FR-008 e o cenário 5 da
  História 1, agora separados entre primeira carga e recarga), as premissas de validação e de desempenho, os rótulos
  da FR-001 e da FR-027 e o formato `YYYY-MM`, e entraram dois casos-limite _(a confirmar)_. Revalidados, os itens
  sobre critérios de sucesso (mensuráveis, sem tecnologia e atingidos pela feature) e os demais continuam passando.
- Depois do `/speckit-clarify` (2026-10-04), com as decisões registradas em `## Esclarecimentos` da spec:
  - Os comportamentos esperados dos defeitos foram decididos pelo usuário, e não lidos no código; o comportamento
    atual de cada um ficou registrado na spec como desvio conhecido, a corrigir fora dela. As notas anteriores, que
    falam da página "como já implementada" e de textos "verificados no código", valem só para o comportamento atual:
    "Cadastre uma categoria antes de registrar uma transação.", "Ver categorias", "Transação criada, mas ela não
    aparece com os filtros em uso." e "Abrir calendário" são textos esperados, que ainda não existem no código. Os
    casos-limite _(a confirmar)_ que essas notas citam, os onze da primeira validação e os dois da primeira revisão,
    foram todos decididos nos Esclarecimentos, e a spec não tem mais nenhum.
  - Defeitos do front, pelo fluxo de bugs: a data digitada lida como dia/mês/ano, a recusa do zero e o limite de 13
    algarismos na parte inteira do valor (FR-014); o diálogo aberto até a resposta da API (FR-016), com o
    comportamento durante a espera em aberto; o aviso da transação salva fora dos filtros em uso (FR-017); "Nova
    transação" sem categorias, com a mensagem, a ação "Ver categorias" e o botão desabilitado até a primeira carga
    (FR-019); o mês do dia escolhido no calendário do "Mês" (FR-024, no campo compartilhado e ainda não reproduzido);
    os calendários em português (FR-030); e, decidida depois da revisão pós-clarify, a consulta que não se repete ao
    escolher de novo a ordenação em uso ou o mesmo mês (FR-031, no menu de ordenação e no campo "Mês"
    compartilhados), à qual se juntou, na sessão de 2026-10-05, a busca por descrição que não refaz a consulta com o
    mesmo texto (FR-031, deduzida do código e ainda não reproduzida). As correções nos componentes compartilhados
    valem também para [[002-gestao-de-categorias]] e para os relatórios.
  - Requisitos sem desvio: ocultar os filtros não os limpa (FR-023), e a busca por descrição não diferencia maiúsculas
    de minúsculas, mas diferencia acentos (FR-024). Trabalho futuro: o indicador de filtro em uso (FR-023). O link da
    página de categorias, que deve levar também o mês, é um desvio registrado em [[002-gestao-de-categorias]]; esta
    página já aceita a categoria e o mês juntos (FR-028).
  - O usuário viu e não pediu mudança no `categoryId` inválido no endereço, no endereço que só define os filtros
    iniciais e na falta de divisória entre transações vizinhas da mesma data, em qualquer ordenação; esses pontos
    continuam documentados como comportamento atual. Entraram também, como comportamento atual, dois casos-limite
    achados na varredura do clarify: o clique duplo em "Excluir", cuja mensagem de erro toma o lugar da que oferece
    "Desfazer", e a edição da mesma transação em duas janelas, em que vale a última gravação.
  - O item "A feature atinge os resultados mensuráveis definidos nos critérios de sucesso" continua marcado porque os
    requisitos, com o comportamento esperado, atingem os critérios; na prática, o SC-002 só vale depois da correção da
    FR-017, e o SC-007, depois da correção da FR-031.
  - A revisão independente depois do clarify (0 CRITICAL, 3 HIGH, 2 MEDIUM e 5 LOW, dos quais 1 HIGH e 1 LOW
    eram da 002) pediu esta nota, o caso-limite da consulta repetida e ajustes de redação (FR-021, FR-028, SC-002,
    cenários 1 e 3 da História 5, casos-limite e premissas), já aplicados. Na mesma rodada, a FR-016, com os cenários
    4 e 6 da História 2 e o caso-limite do diálogo, e a FR-024 mudaram para ficar coerentes com a
    [[002-gestao-de-categorias]], e não por achado da revisão. Nenhum item do checklist mudou de estado.
  - A segunda rodada da revisão (0 CRITICAL, 0 HIGH, 1 MEDIUM e 5 LOW) pediu a ressalva do dia do mês já aplicado
    (FR-024, cenário 7 da História 4 e caso-limite do "Mês"), o cenário 8 da História 4, sobre a consulta repetida, a
    volta ao formato `- P: … → R: …` nos Esclarecimentos e ajustes nesta nota. Também registrou, deduzido do código e
    não reproduzido, o caso-limite da busca por descrição que refaz a consulta com o mesmo texto, contrário ao
    princípio III, com a classificação pendente com o usuário. Nenhum item do checklist mudou de estado.
  - Sessão de 2026-10-05 dos Esclarecimentos: o usuário classificou a busca que refaz a consulta com o mesmo texto
    como defeito, o mesmo da consulta repetida (FR-031, SC-007, caso-limite e premissas; vale também para o "Nome" da
    002), e confirmou que o comportamento do diálogo durante a espera fica para o `/speckit-bug-assess` da correção da
    FR-016, que vai propor o comportamento no portão do bug. Não resta classificação pendente; o SC-007 só vale na
    prática depois da correção da FR-031. Nenhum item do checklist mudou de estado.
- Itens marcados como incompletos exigem atualizar a spec antes do `/speckit-clarify` ou do `/speckit-plan`.
