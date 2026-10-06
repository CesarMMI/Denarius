# Checklist de performance: Gestão de categorias

**Objetivo**: Testar a qualidade dos requisitos de performance da página de categorias (spec retroativa): se as metas
estão escritas como critérios mensuráveis, se o trabalho na fonte dos dados, as consultas repetidas, o debounce, o cache
da paleta, o carregamento sob demanda e o budget do bundle estão especificados de forma completa, clara e consistente,
conforme o princípio III do `AGENTS.md` da raiz e as restrições de performance do `front/AGENTS.md`
**Criado em**: 2026-10-06
**Feature**: [spec.md](../spec.md) · [plan.md](../plan.md) · [research.md](../research.md) ·
[data-model.md](../data-model.md) · [quickstart.md](../quickstart.md)

**Observação**: Checklist gerado pelo `/speckit-checklist` a partir da spec, do plan e dos artefatos da Fase 1. Os
itens testam os requisitos escritos, e não o código. Como a feature é retroativa, os desvios conhecidos e as exceções já
decididos pelo usuário não são reabertos: os itens só conferem se estão bem documentados (destino, bloqueio e critério
de correção).
**Responsável pela revisão**: Este checklist é um artefato de revisão da qualidade dos requisitos, de posse do revisor.
Marque `[x]` só quando o revisor concluir que o critério está atendido.
**Semântica das marcações**: `[x]` significa que o critério foi revisado e está atendido quanto à qualidade dos
requisitos. Não significa que a implementação esteja concluída.

## Completude dos requisitos

- [x] CHK001 As metas de desempenho que importam ao usuário estão todas na spec como critérios de sucesso, e a ausência
      de metas de tempo (para carregar a lista, salvar ou excluir) está justificada, como pede o princípio III? [Gap,
      Spec §SC-006, Spec §SC-009] — ok: segunda rodada (2026-10-06): a Spec §Premissas registra que não há meta de tempo, com o motivo (uso pessoal, dezenas de categorias; SC-006 e SC-009 ocupam o lugar), por decisão do usuário (Esclarecimentos 2026-10-06, Research §D14).
- [x] CHK002 O volume esperado de categorias está quantificado na spec (o plan fala em "dezenas"), com o limite a partir
      do qual a decisão de não paginar a lista deve ser revista? [Gap, Spec §Premissas, Plan §Contexto técnico] — ok: segunda rodada (2026-10-06): a Spec §Premissas quantifica o volume (dezenas de categorias) e define quando a falta de paginação é revista: se uma medição mostrar lentidão ("primeiro meça", princípio III), por decisão do usuário (Research §D14).
- [x] CHK003 Está especificado quantas requisições cada operação gera (uma escrita mais uma recarga da lista ao criar,
      editar, excluir ou desfazer), e por que a lista é buscada inteira de novo, em vez de atualizada localmente? [Gap,
      Spec §FR-011, Spec §FR-013] — ok: FR-011 e FR-013 (escrita seguida de recarga) e Contracts §API consumida ("corpo
      ignorado; a página recarrega a lista"); o motivo vem da FR-022 e das Premissas: quantidade, saldo, filtros e
      ordem são calculados pela API, e atualizar a lista localmente duplicaria essas regras.
- [x] CHK004 O plan diz como as metas SC-006 e SC-009 são cumpridas e conferidas, e não só que existem (princípio III:
      "o plan diz como serão cumpridas")? [Completude, Plan §Contexto técnico, Quickstart §1] — ok: Plan §Constitution
      Check III (rota lazy, filtros na API, debounce de 300 ms, paleta buscada uma vez, com `arquivo:linha`), Plan
      §Contexto técnico (medição do build) e Quickstart §1 (build e specs). A conferência da segunda parte da SC-009
      fica no CHK015.

## Clareza dos requisitos

- [x] CHK005 "Texto aplicado não mudou" (FR-023) está definido com precisão: um texto que só difere por espaços nas
      pontas, ou por maiúsculas, conta como o mesmo texto? [Ambiguity, Spec §FR-023, Spec §Casos-limite] — ok: a FR-017
      e as Premissas dizem que a página envia o texto exatamente como foi digitado, e que espaços, maiúsculas e acentos
      são regra da API; logo, o "texto aplicado" é o texto exato, e um texto diferente é uma consulta legítima. O
      `/speckit-bug-assess` da FR-023 parte dessa leitura.
- [x] CHK006 A combinação da pausa de 300 ms com a saída do campo está especificada sem margem para uma consulta
      duplicada (uma pausa seguida de saída do campo gera uma consulta ou duas)? [Clareza, Spec §FR-017, Spec §FR-023]
      — ok: a FR-023 e os Casos-limite dizem que o "Nome" não refaz a consulta quando o texto aplicado não mudou, e
      registram a consulta extra ao sair do campo depois da pausa como desvio conhecido.
- [x] CHK007 "Uma única nova consulta" (SC-006) está definida em termos observáveis (requisições HTTP à lista por ação
      do usuário), incluindo se a recarga depois de uma escrita entra na contagem? [Clareza, Spec §SC-006] — ok: a
      "consulta da lista" é o `GET {apiUrl}/categories` (Contracts §API consumida); a SC-006 trata só de mudanças de
      filtro e de ordenação, e a recarga depois de uma escrita é regida pela FR-011 e pela FR-013 (uma por escrita
      aceita).
- [x] CHK008 "O mesmo mês" (FR-023) cobre explicitamente o dia de um mês já aplicado, escolhido na visão de dias, como a
      FR-016 diz ("um dia do mês já aplicado não muda nada")? [Clareza, Spec §FR-016, Spec §FR-023] — ok: a FR-016 liga
      o caso à FR-023 de forma explícita ("um dia do mês já aplicado não muda nada (FR-023)").

## Consistência dos requisitos

- [x] CHK009 Filtros, ordenação, quantidade e saldo estão atribuídos à API em todos os requisitos (FR-017, FR-018,
      FR-019, FR-022 e Premissas), sem nenhum ponto que admita filtrar ou ordenar no front depois de carregar tudo?
      [Consistência, Spec §FR-017, Spec §FR-018, Spec §FR-019, Spec §FR-022] — ok: FR-017 ("filtrada pela API"), FR-019
      ("ordenada pela API"), FR-022 e Premissas ("são feitos pela API… a página mostra o resultado como vem").
- [x] CHK010 A regra "filtros vazios não são aplicados" (FR-016) é coerente com a busca só com espaços, que é enviada à
      API e ignorada por ela (Casos-limite)? [Consistência, Spec §FR-016, Spec §Casos-limite] — ok: o Data-model §Filtros
      da página define "vazio" como `''`; um texto só com espaços não é vazio, é enviado, e a API o ignora (Casos-limite
      e Data-model, coluna "Parâmetro enviado").
- [x] CHK011 O comportamento de carregamento é coerente entre os casos: a mudança de filtro ou de ordenação troca as
      linhas pelo indicador, e a recarga ("Recarregar", depois de salvar, excluir ou desfazer) mantém as linhas; a
      diferença está justificada? [Consistência, Spec §FR-006, Spec §Casos-limite, Data-model §Estado da página] — ok:
      Casos-limite, Data-model §Estado da página e Research §Lista, estados e link (o spinner a cada recarga esconderia
      as linhas atuais sem necessidade); a mesma regra está na FR-008 da 003.
- [x] CHK012 A política de cache da paleta (buscada uma vez, sem nova tentativa até a aplicação ser recarregada) é
      coerente entre a spec (Casos-limite), a SC-009, o research e a remoção futura do `ColorsService.reload()` (D6)?
      [Consistência, Spec §Casos-limite, Spec §SC-009, Research §Paleta padrão, Research §D6] — ok: os quatro dizem o
      mesmo; o Research §Paleta padrão (Observação) registra que o `reload()` não tem uso em produção, coerente com o
      caso-limite.
- [x] CHK013 A FR-023 e a SC-006 estão escritas da mesma forma que os requisitos equivalentes da
      [003-gestao-de-transacoes](../../003-gestao-de-transacoes/spec.md) ("Descrição") e dos relatórios, já que a
      correção é uma só nos componentes compartilhados? [Consistência, Spec §FR-023, Spec §Esclarecimentos] — ok: a
      FR-031 e a SC-007 da 003 têm o mesmo conteúdo (ordenação em uso, mesmo mês, texto aplicado inalterado; correção
      única, começando por teste). Observação fora desta feature: a spec da 001 (relatórios) não registra esse desvio,
      embora a 002 e a 003 digam que a correção vale para os relatórios.

## Qualidade dos critérios de aceitação

- [x] CHK014 A SC-006 pode ser medida objetivamente nas três partes (uma consulta por mudança, no máximo uma por pausa
      de 300 ms, nenhuma ao repetir a escolha), e o plan diz qual delas hoje não é atendida? [Mensurabilidade, Spec
      §SC-006, Plan §Dependências e riscos] — ok: as três partes são contáveis em requisições; o Plan §Contexto técnico
      ("esta última é desvio conhecido"), o Constitution Check III e o §Dependências e riscos dizem qual não é atendida.
- [x] CHK015 A SC-009 tem um critério verificável para as duas partes: o chunk `categories-page` fora do "Initial
      total" no build e a requisição da paleta só na primeira abertura do diálogo? [Mensurabilidade, Spec §SC-009,
      Quickstart §1] — ok: segunda rodada (2026-10-06): o Quickstart §1 tem a linha SC-009, com o build (chunk lazy) e, para a paleta, `categories-page.spec.ts:61` (`verify()`), `category-form.spec.ts:29` e `colors.service.spec.ts`.
- [x] CHK016 As medições registradas no plan (chunk `categories-page` com 13,73 kB; bundle inicial com 633,08 kB, aviso
      em 700 kB) têm data e comando, e está definido o que fazer se uma mudança futura reduzir essa margem?
      [Mensurabilidade, Plan §Contexto técnico, Plan §Constitution Check] — ok: data (2026-10-05) e comando
      (`npm run build`) no Plan §Contexto técnico e em Restrições técnicas; o que fazer perto do limite é regra do
      `front/AGENTS.md` (budgets do `angular.json`), e não desta feature. O risco dos bug-fix fica no CHK022.

## Cobertura de cenários e casos-limite

- [x] CHK017 Está especificado o que acontece com respostas fora de ordem quando o usuário muda os filtros rapidamente
      (só a resposta da última combinação é exibida, e as anteriores são descartadas)? [Edge Case, Gap, Spec §FR-017] — ok: segunda rodada (2026-10-06): o Data-model §Estado da página, linha `categories`, registra que uma mudança de filtros ou de ordenação com requisição em andamento cancela a anterior e só a última combinação é exibida.
- [x] CHK018 Está especificado o comportamento de vários "Recarregar" seguidos enquanto a lista ainda carrega (uma
      consulta só, ou uma por clique)? [Edge Case, Gap, Spec §FR-006] — ok: segunda rodada (2026-10-06): o Research §Lista, estados e link registra o comportamento atual: o `reload()` não faz nada em `loading`, e numa recarga em andamento a reinicia e cancela a anterior. Ressalva de redação (LOW, no relatório): o fecho "não acumulam consultas" deveria dizer que cada clique envia uma nova requisição e cancela a anterior.
- [x] CHK019 O custo de abrir o diálogo enquanto a paleta ainda carrega está coberto (o diálogo abre sem esperar a
      paleta), além da "Cor personalizada" sobrescrita pela paleta que chega atrasada? [Cobertura, Spec §Casos-limite,
      Spec §FR-009] — ok: os Casos-limite (paleta buscada na primeira abertura; a paleta que chega depois de o usuário
      escolher uma cor) pressupõem que o diálogo abre sem esperá-la, e o Data-model §Paleta padrão diz que `colors()` é
      `[]` enquanto carrega.
- [x] CHK020 O comportamento com muitas categorias (rolagem dentro do cartão, sem paginação) tem limite ou
      degradação aceitável definidos? [Edge Case, Spec §Casos-limite, Spec §Premissas] — ok: segunda rodada (2026-10-06): coberto pela premissa do CHK002 (revisão da falta de paginação se uma medição mostrar lentidão), decidida pelo usuário.

## Dependências e premissas

- [x] CHK021 A premissa de que a API faz o trabalho pesado (busca, filtros, ordenação e agregação por mês) está ligada a
      requisitos ou metas do back ([001-category-management](../../../../back/specs/001-category-management/spec.md)),
      e não só citada? [Assumption, Dependência, Spec §Premissas, Spec §FR-022] — ok: FR-022, Premissas e Research
      §Contrato consumido apontam para a spec e o contrato do back 001, que definem busca, filtros, ordenação e
      agregação (SC-004 e SC-005 do back). O back não tem meta formal de desempenho (plan do back 001, "Metas de
      desempenho"), o que alimenta a pergunta do CHK001.
- [x] CHK022 O plan avalia o efeito no bundle inicial e nas consultas dos bug-fix que bloqueiam a entrega (por exemplo,
      um `MatDatepickerIntl` em português provido para toda a aplicação, ou a mudança nos componentes compartilhados de
      ordenação e de mês)? [Gap, Plan §Dependências e riscos] — ok: segunda rodada (2026-10-06): o Plan §Dependências e riscos registra o risco do `MatDatepickerIntl` no bundle inicial (margem de cerca de 67 kB) e manda o `/speckit-bug-assess` medir com `npm run build` e preferir os chunks lazy.
- [x] CHK023 A restrição de estilos por componente abaixo de 4 kB está registrada com a medição atual dos componentes
      próprios desta feature (`category-form.scss`, `categories-table.scss`)? [Completude, Plan §Constitution Check,
      Restrições técnicas] — ok: Plan §Constitution Check, linha Restrições técnicas ("nenhum estilo de componente acima
      de 4 kB", no build de 2026-10-05, sem aviso de budget).

## Desvios conhecidos (documentação)

- [x] CHK024 A consulta repetida (FR-023, SC-006) está documentada como violação do princípio III, e não como exceção,
      com destino (fluxo de bugs do front), bloqueio da entrega (D10) e critério de correção objetivo (nenhuma consulta
      ao repetir a ordenação, o mesmo mês ou o texto aplicado)? [Plan §Constitution Check III, Plan §Desvios conhecidos,
      Research §D10] — ok: Plan §Constitution Check III, §Desvios conhecidos, resultado do portão, Research §D10, e o
      critério na FR-023 e na SC-006.
- [x] CHK025 Está registrado que o caso do "Nome" é deduzido do código e ainda não reproduzido, e que a correção começa
      por um teste que o reproduza, com o que fazer se o teste não reproduzir o problema? [Clareza, Spec §FR-023, Plan
      §Desvios conhecidos] — ok: FR-023, Esclarecimentos de 2026-10-05 e Plan §Desvios conhecidos; o que fazer se não
      reproduzir é decidido no portão do `/speckit-bug-assess` (AGENTS.md raiz, Bugs, passo 2), destino que o plan já
      indica.
- [x] CHK026 O escopo da correção (ordenação e mês compartilhados, "Nome" de categorias e "Descrição" de transações,
      válida para categorias, transações e relatórios) está listado da mesma forma na spec, no plan e no research?
      [Consistência, Spec §FR-023, Plan §Desvios conhecidos, Research §Filtros, mês e ordenação] — ok: a FR-023 e o Plan
      §Desvios conhecidos listam o mesmo escopo; o Research §Filtros, mês e ordenação aponta para o plan, sem
      contradizê-lo.
- [x] CHK027 O plan diz como o `/speckit-converge` confere a conclusão do bug-fix da consulta repetida antes de dar a
      feature como entregue (que evidência conta: o `test.md` do bug, a SC-006 reavaliada)? [Clareza, Plan §Dependências
      e riscos, Research §D10] — ok: segunda rodada (2026-10-06): o Plan §Dependências e riscos e o Research §D16 definem a evidência: o `front/bugs/<slug>/test.md` com `verified`, o teste de reprodução e a suíte completa passando, e o converge reavaliando os FR/SC afetados (FR-023 e SC-006; FR-020; FR-012 e FR-013, do novo bloqueio do clique repetido).
- [x] CHK028 A exceção à F1 (o `CategoryForm` provoca o `GET` da paleta) registra o ganho de performance que a justifica
      (a paleta só na primeira abertura do diálogo) e a alternativa rejeitada por ir contra a SC-009? [Completude, Plan
      §Acompanhamento de complexidade, Research §D4] — ok: Plan §Acompanhamento de complexidade (atende à SC-009; a
      alternativa iria "contra a SC-009 se buscada ao abrir a página") e Research §D4.

## Observações

- Marque `[x]` só depois que a revisão confirmar que o critério de qualidade do requisito está atendido.
- Deixe desmarcados os itens que ainda precisam de esclarecimento, correção ou avaliação do revisor.
- O `/speckit-implement` lê o estado das marcações como portão e não deve alterá-las.
- O `checklists/requirements.md` tem ciclo próprio, mantido pelo `/speckit-specify` e pelo `/speckit-clarify`.
- Registre comentários e achados junto ao item.
- Os itens são numerados em sequência para facilitar a referência.
