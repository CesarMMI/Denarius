# Checklist de qualidade da especificação: Gestão de categorias

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

- Esta é uma especificação **retroativa**: documenta a página de categorias como já implementada em
  `front/src/app/categories` (com as partes de `front/src/app/shared` que ela usa) e coberta pelos seus testes, e não
  descreve trabalho por fazer.
- A Entrada cita o pedido, que nomeia pastas do código; os requisitos ficam livres de frameworks e de estrutura de
  código. "API" aparece só como a fonte dos dados e das mensagens, o ponto de integração com o back (princípio IV), e
  não como escolha de tecnologia.
- A rota `/categories`, o endereço `/transactions?categoryId=<id>` gerado pelo link da quantidade, a formatação
  BRL/pt-BR, os textos entre aspas (inclusive os nomes acessíveis, em português ou não), os tempos das mensagens, a
  pausa de 300 ms da busca e a paleta de 11 cores ficam na spec porque são comportamento que o usuário vê; a rota e o
  endereço, além disso, são públicos (princípio F2).
- As regras do back (validação do nome e da cor, recusa da exclusão de categoria em uso, cálculo da quantidade e do
  saldo) entram como o que o usuário observa na página, citando [[001-category-management]], sem copiar a spec do back.
  O lado da página de transações que recebe o filtro por categoria fica na [[003-gestao-de-transacoes]].
- O SC-009 reformula o carregamento sob demanda da rota e da paleta como um resultado para o usuário (nada baixado
  antes de ser usado).
- Nenhum marcador [NEEDS CLARIFICATION]: o comportamento foi lido no código e nos testes, não suposto. Os pontos que
  parecem não intencionais ou que não têm teste estão documentados como estão, em Casos-limite e Premissas, e devem ser
  levados ao usuário no `/speckit-clarify`:
  - nome só com espaços aceito pelo diálogo e recusado só pela API;
  - diálogo que fecha antes da resposta da API;
  - filtros ocultos que continuam valendo;
  - link da quantidade que não leva o mês;
  - busca por nome que diferencia maiúsculas, minúsculas e acentos (regra da API);
  - nomes acessíveis em inglês no calendário do campo "Mês" e cores da paleta anunciadas pelo código hexadecimal;
  - recarga sem indicador de carregamento;
  - consulta repetida ao escolher de novo a mesma ordenação ou o mesmo mês (princípio III);
  - falha silenciosa da paleta, que só é buscada de novo ao recarregar a aplicação;
  - "Cor personalizada" sobrescrita pela paleta que chega atrasada, na primeira abertura do diálogo;
  - mensagem de "Desfazer" substituída por outra;
  - limite de 100 caracteres sem teste.
- Todos os itens passaram na primeira validação. A revisão independente (0 CRITICAL, 0 HIGH, 3 MEDIUM, 7 LOW) pediu só
  ajustes na documentação do comportamento atual, já aplicados; nenhum item do checklist mudou de estado.
- Depois do `/speckit-clarify` (2026-10-04), com as decisões registradas em `## Esclarecimentos` da spec:
  - Os comportamentos esperados dos defeitos foram decididos pelo usuário, e não lidos no código; o comportamento
    atual de cada um ficou registrado na spec como desvio conhecido, a corrigir fora dela. As notas anteriores, que
    falam da página "como já implementada", do comportamento "lido no código e nos testes" e do endereço
    `/transactions?categoryId=<id>`, valem só para o comportamento atual: com um mês escolhido, o endereço esperado é
    `/transactions?categoryId=<id>&month=YYYY-MM` (FR-004), e "Abrir calendário" é um texto esperado, que ainda não
    existe no código. Os pontos que essas notas mandavam levar ao `/speckit-clarify` foram todos decididos nos
    Esclarecimentos, menos o limite de 100 caracteres sem teste (abaixo).
  - Defeitos do front, pelo fluxo de bugs: o link da quantidade leva também o mês escolhido, no endereço
    `/transactions?categoryId=<id>&month=YYYY-MM` (FR-004); o diálogo fica aberto até a resposta da API (FR-010), com
    o comportamento durante a espera em aberto; escolher um dia no calendário do "Mês" aplica o mês daquele dia
    (FR-016, no campo compartilhado, decidido no esclarecimento da [[003-gestao-de-transacoes]] e ainda não
    reproduzido); os nomes acessíveis do calendário passam a ser em português (FR-020); e escolher de novo a ordenação
    em uso, ou o mesmo mês, não refaz a consulta (FR-023, nos componentes compartilhados, decidido depois da revisão
    pós-esclarecimento e válido também para a [[003-gestao-de-transacoes]]).
  - Defeito do back, num ciclo próprio: a busca por nome deixa de diferenciar maiúsculas de minúsculas e continua
    diferenciando acentos (FR-017).
  - Requisito: ocultar os filtros não os limpa (FR-015). Trabalho futuro: o indicador de filtro em uso (FR-015) e um
    nome para cada cor da paleta (FR-020).
  - O usuário viu e não pediu mudança no nome só com espaços, na recarga sem indicador, na paleta e na exclusão sem
    confirmação, com o "Desfazer" substituído; esses pontos continuam documentados como comportamento atual.
  - Na sessão de 2026-10-05: a consulta repetida no "Nome" com o texto já aplicado é defeito do front, o mesmo da
    FR-023 (deduzido do código e ainda não reproduzido; o diagnóstico começa por um teste que o reproduza), e o
    comportamento do diálogo durante a espera fica para o `/speckit-bug-assess` da FR-010, que o propõe ao usuário.
  - O item "A feature atinge os resultados mensuráveis definidos nos critérios de sucesso" continua marcado porque os
    requisitos, com o comportamento esperado, atingem os critérios; na prática, o SC-006 só vale depois da correção da
    FR-023.
  - O limite de 100 caracteres sem teste fica registrado para o plan levar ao usuário.
- Itens marcados como incompletos exigem atualizar a spec antes do `/speckit-clarify` ou do `/speckit-plan`.
