# Checklist de UX: Gestão de transações

**Objetivo**: Testar a qualidade dos requisitos de experiência do usuário da página de transações (estados de
carregamento, vazio e erro, mensagens e textos em português, acessibilidade, formulário, filtros e consistência com as
outras páginas), como base para a aprovação do plan. Não testa o código.

**Criado em**: 2026-10-06

**Feature**: [spec.md](../spec.md) · [plan.md](../plan.md) · [data-model.md](../data-model.md) ·
[contracts/transactions-ui.md](../contracts/transactions-ui.md) · [quickstart.md](../quickstart.md)

**Observação**: Checklist gerado pelo `/speckit-checklist` a partir da spec, dos artefatos do plan e da constituição
(`AGENTS.md` da raiz, seção Idioma, e `front/AGENTS.md`, F1 e F4), comparando com as specs de
[002-gestao-de-categorias](../../002-gestao-de-categorias/spec.md) e
[001-reports-dashboard](../../001-reports-dashboard/spec.md).

**Responsável pela revisão**: Este checklist é um artefato de revisão da qualidade dos requisitos e pertence ao revisor.
Marque um item `[x]` só quando o revisor concluir que o critério de qualidade do requisito foi atendido.

**Semântica da marcação**: `[x]` quer dizer que o critério foi revisado e atendido quanto à qualidade do requisito. Não
quer dizer que a implementação está pronta.

## Estados de carregamento, vazio e erro

- [x] CHK001 Os estados de carregamento, vazio e erro estão definidos para todas as combinações das duas listas
      (transações e categorias), e a tabela de estados do data-model é coerente com a FR-008, a FR-009 e os
      casos-limite? [Completude, Spec §FR-008, §FR-009, §Casos-limite; Data-model §Estados do recurso] — ok: o
      Data-model §Estados do recurso cobre as combinações (o carregamento tem precedência sobre o erro), coerente com a
      FR-008, a FR-009, os Casos-limite e `transactions-table.html`.
- [x] CHK002 A mesma mensagem "Não foi possível carregar as transações." para uma falha só das categorias está
      registrada como intencional, sabendo que o usuário não consegue distinguir qual lista falhou? [Clareza, Spec
      §FR-009, §Casos-limite] — ok: Casos-limite ("Pelo mesmo motivo, uma falha só das categorias também mostra...") e
      Research §Estados (alternativa rejeitada).
- [x] CHK003 Está registrado como intencional que "nenhuma transação registrada" e "nenhuma transação com os filtros em
      uso" mostram a mesma mensagem, sem um estado próprio para o primeiro uso? [Ambiguity, Spec §FR-009, §Casos-limite]
      — ok: FR-009 ("Sem transações para mostrar", qualquer que seja a causa) e Casos-limite (combinação sem resultado);
      um estado de primeiro uso não foi pedido.
- [x] CHK004 O caminho de recuperação de um erro de carregamento (só o "Recarregar" do cabeçalho) está definido, e a
      mensagem de erro ou a página indicam esse caminho ao usuário? A escolha está alinhada com a 002 e com a 001 (onde
      um bloco que falha pode ser tentado de novo sozinho)? [Consistência, Spec §História 1 cenário 7, §FR-010; 001
      §SC-003] — ok: História 1 cenário 7 e FR-010, no mesmo padrão da FR-006 da 002; a nova tentativa por bloco da 001
      vale para blocos independentes, e não para uma lista só.
- [x] CHK005 Há requisito de retorno visual enquanto uma exclusão ou uma restauração está em andamento (entre o clique e
      a resposta), como a FR-016 passa a pedir para o salvamento? [Gap, Spec §FR-020, §FR-021] — ok: segunda rodada:
      FR-020 (retorno visual durante a exclusão e a restauração; a forma sai do `/speckit-bug-assess`), Casos-limite e
      Esclarecimentos de 2026-10-06; alinhado às FR-012 e FR-013 da 002.
- [x] CHK006 Está definido o que a célula "Categoria" mostra quando a categoria da transação não está entre as
      carregadas e como o usuário sabe que precisa de "Recarregar"? [Clareza, Spec §Casos-limite, §SC-003] — ok:
      Casos-limite (sem etiqueta até "Recarregar") e Research §Etiqueta; nada indica o "Recarregar", comportamento atual
      aprovado com a spec.

## Mensagens e textos em português

- [x] CHK007 Todos os textos ao usuário (título, rótulos, botões, opções, mensagens, dicas, títulos dos diálogos e nomes
      acessíveis) estão citados literalmente na spec, em português, sem lacuna entre a spec e os contracts? [Completude,
      Spec §Requisitos funcionais; Contracts §Textos e nomes acessíveis] — ok: segunda rodada: FR-012 ("Tipo", prefixo
      "R$" e marcador "0,00"), FR-014 ("N/255") e Contracts §Textos (FR-001 a FR-032).
- [x] CHK008 O texto do aviso da FR-017 está fixado ou só exemplificado ("por exemplo")? A variante da edição
      ("Transação salva…" fora dos filtros) está definida? [Ambiguity, Spec §FR-017, §História 2 cenário 11] — ok:
      segunda rodada: FR-017 (o texto final, de criação e de edição, sai do `/speckit-bug-assess`) e Plan §Desvios
      conhecidos.
- [x] CHK009 Durações e ações das mensagens (3 s sem ação para sucesso; 5 s com "Fechar" para recusa; 5 s com "Desfazer"
      para exclusão) são consistentes entre a FR-017, a FR-018 e a FR-020 a FR-022 e com a FR-011, a FR-013 e a FR-014
      da 002? [Consistência, Spec §FR-017, §FR-018, §FR-020, §FR-021, §FR-022; 002 §FR-011, §FR-013, §FR-014] — ok: 3 s
      sem ação (FR-017, FR-021), 5 s com "Fechar" (FR-018, FR-022) e 5 s com "Desfazer" (FR-020), iguais às FR-011,
      FR-013 e FR-014 da 002.
- [x] CHK010 As mensagens da FR-019 ("Não foi possível carregar as categorias. Tente novamente." e "Cadastre uma
      categoria antes de registrar uma transação.", com "Ver categorias") têm duração e ação de fechar definidas, como
      as outras mensagens? [Gap, Spec §FR-019] — ok: segunda rodada: FR-019 (5 s com "Fechar"; a duração da mensagem com
      "Ver categorias" sai do `/speckit-bug-assess`) e Contracts §Erros.
- [x] CHK011 "Informe um valor válido" vale para todos os motivos de recusa (formato, sinal, separador de milhar, zero,
      mais de 13 algarismos inteiros). Isso está registrado como intencional, ou há requisito de uma dica do formato
      aceito, já que a lista mostra "R$ 8.600,00" e o campo recusa "8.600,00"? [Clareza, Spec §FR-014, §Casos-limite] —
      ok: a FR-014 define uma mensagem para todas as recusas, e os Casos-limite citam "8.600,00" recusado com ela; uma
      dica de formato seria comportamento novo, não pedido.
- [x] CHK012 Os formatos regionais (data dd/mm/aaaa, mês mm/aaaa, moeda pt-BR, vírgula ou ponto como separador decimal)
      estão especificados da mesma forma na lista, nos filtros e no formulário, e com a mesma grafia da 002 (que escreve
      "MM/aaaa")? [Consistência, Spec §FR-004, §FR-006, §FR-014, §FR-024; 002 §FR-016] — ok: segunda rodada: "MM/aaaa"
      na FR-024 e nos Casos-limite, como na 002.
- [x] CHK013 O desvio dos calendários em inglês está documentado com destino (fluxo de bugs do front), alcance (todos os
      calendários da aplicação), bloqueio da entrega e critério de correção? [Completude, Spec §FR-030; Plan
      §Constitution Check Idioma, §Desvios conhecidos] — ok: Plan §Constitution Check Idioma e §Desvios conhecidos
      (destino, todos os calendários, bloqueia a entrega); o critério é a FR-030, com o teste que reproduz no bug-fix.
- [x] CHK014 A FR-030 dá só "Abrir calendário" como exemplo. Os demais rótulos do calendário (mês e ano anterior e
      seguinte, escolher data, trocar de visão) estão enumerados, ou está dito que o bug-fix os define? [Ambiguity, Spec
      §FR-030, §Casos-limite] — ok: segunda rodada: FR-030 (o conjunto completo dos rótulos é definido no bug-fix) e
      Plan §Constitution Check Idioma.

## Acessibilidade

- [x] CHK015 Os nomes acessíveis dos botões só com ícone estão enumerados igualmente na spec e nos contracts, e a regra
      de dica está clara para todos (a FR-030 pede dica no cabeçalho e nas linhas e não diz nada dos "Limpar …", que a
      002 define "sem dica")? [Consistência, Ambiguity, Spec §FR-030; Contracts §Textos e nomes acessíveis; 002 §FR-020]
      — ok: segunda rodada: FR-030 e Contracts §Textos (os "Limpar …" ficam sem dica, como na 002).
- [x] CHK016 Há requisitos de teclado para as ações das linhas, o menu de ordenação, os filtros e o diálogo (foco
      inicial, ordem de tabulação, Enter para salvar, volta do foco ao fechar o diálogo)? [Gap] — ok: segunda rodada:
      Premissas (teclado dos componentes do Material) e Plan §Desvios conhecidos (melhorias numa feature própria de
      acessibilidade, sem bloqueio).
- [x] CHK017 Há requisitos para anunciar a leitores de tela as mudanças de estado da lista (carregando, vazia, erro,
      recarregada) e as mensagens que aparecem nos snackbars? [Gap, Spec §FR-008, §FR-009, §FR-017] — ok: segunda
      rodada: Premissas (as mensagens são anunciadas; os estados da lista não) e melhorias numa feature própria de
      acessibilidade.
- [x] CHK018 O prazo de 5 segundos do "Desfazer" está avaliado para quem precisa de mais tempo (leitor de tela,
      teclado): há requisito para estender o prazo, ou o limite está registrado como aceito? [Gap, Spec §FR-020,
      §SC-004] — ok: segunda rodada: Premissas (5 s, sem como estender, registrado como atual) e melhorias numa feature
      própria de acessibilidade.
- [x] CHK019 O vermelho das saídas tem requisito de contraste e de aparência no tema escuro, como a etiqueta da
      categoria tem (FR-005, 7:1), sabendo que o sinal garante que a cor não é a única pista (SC-003)? [Gap, Spec
      §FR-006, §SC-003] — ok: segunda rodada: Premissas (cor de erro do tema, sem contraste medido; o sinal é a outra
      pista) e melhorias numa feature própria de acessibilidade.
- [x] CHK020 O contraste de pelo menos 7:1 da etiqueta (FR-005) tem método de verificação definido para as cores
      escolhidas livremente pelo usuário? [Mensurabilidade, Spec §FR-005] — ok: Research §Etiqueta
      (`mat-chip-color.spec.ts:37` confere 7:1 para várias cores, e os tons 90 e 30 dão cerca de 7,2:1 por construção).
- [x] CHK021 Os rótulos dos campos do formulário e dos filtros e as mensagens de erro de cada campo estão definidos e
      associados ao campo a que se referem? [Completude, Spec §FR-012, §FR-014, §FR-024] — ok: FR-012 e FR-024 (rótulos)
      e FR-014 (uma mensagem por campo); o `mat-error` dentro do `mat-form-field` associa a mensagem ao campo.

## Formulário em diálogo

- [x] CHK022 Os valores iniciais dos diálogos de criação e de edição estão definidos para todos os campos, inclusive "a
      primeira categoria da lista" (em ordem de nome) e "hoje" (o dia no computador do usuário)? [Completude, Spec
      §FR-013, §Premissas] — ok: FR-012, FR-013, Premissas ("Hoje" é o dia no computador do usuário) e Data-model
      §`Transaction` → formulário de edição.
- [x] CHK023 O ponto em aberto da FR-016 ("Cancelar", Esc ou clique fora durante a espera, e a resposta que chega depois
      de o diálogo fechar) está marcado como não decidido, com destino no `/speckit-bug-assess`, sem que outro requisito
      suponha uma resposta? [Completude, Spec §FR-016, §História 2 cenário 6] — ok: FR-016, História 2 cenário 6 e
      Esclarecimentos de 2026-10-05; nenhum artefato supõe uma resposta (o Data-model §Transições remete ao
      `/speckit-bug-assess`).
- [x] CHK024 Está definido como o diálogo mostra que um "Salvar" está em andamento (botão desabilitado, indicador ou
      outra forma), além de "sem aceitar um segundo Salvar"? [Ambiguity, Spec §FR-016] — ok: segunda rodada: FR-016 (a
      forma de mostrar o "Salvar" em andamento sai do `/speckit-bug-assess`).
- [x] CHK025 O desvio de "Nova transação" sem categorias está documentado com destino e critério de correção, e está
      definido o estado do botão quando a primeira carga das categorias falha (habilitado, com a mensagem de falha)?
      [Completude, Clareza, Spec §FR-019; Plan §Desvios conhecidos] — ok: segunda rodada: "até a primeira carga das
      categorias terminar" na FR-019, no cenário 10 da História 2 e nos Casos-limite.
- [x] CHK026 A regra da data digitada está definida sem ambiguidade (formatos aceitos, ano com quatro algarismos, datas
      inexistentes), e está dito quando "Informe uma data válida" aparece (ao sair do campo ou ao salvar)? [Clareza,
      Spec §FR-014, §Casos-limite] — ok: FR-014 e Casos-limite (dia/mês/ano, ano com quatro algarismos, com ou sem
      zeros, datas inexistentes recusadas); a FR-014 exige o erro no "Salvar".
- [x] CHK027 O formato do contador da descrição está definido (por exemplo, "N/255", como o "N/100" da 002)?
      [Consistência, Spec §FR-014, §Casos-limite; 002 §FR-008] — ok: segunda rodada: FR-014 e Contracts §Textos
      ("N/255").

## Filtros, ordenação e endereço

- [x] CHK028 Ocultar os filtros sem limpá-los e sem indicador (FR-023) está registrado com o risco de o usuário achar
      que vê todas as transações, com o indicador como trabalho futuro, e da mesma forma que na 002? [Consistência, Spec
      §FR-023; 002 §FR-015] — ok: FR-023 e Casos-limite (nada indica o filtro em uso; o indicador é trabalho futuro),
      como na FR-015 da 002.
- [x] CHK029 O desvio do aviso de transação fora dos filtros está documentado com destino e critério de correção (texto,
      casos em que aparece, criação e edição, e filtros que não mudam)? [Completude, Spec §FR-017, §SC-002; Plan
      §Desvios conhecidos] — ok: segunda rodada: FR-017 (texto final no `/speckit-bug-assess`) e Plan §Desvios
      conhecidos.
- [x] CHK030 O desvio do dia escolhido no filtro "Mês", ainda a reproduzir, tem documentados o alcance (as três páginas)
      e o que acontece se a reprodução falhar? [Completude, Spec §FR-024, §Casos-limite; Plan §Desvios conhecidos] — ok:
      segunda rodada: Casos-limite e Plan §Desvios conhecidos (se a reprodução não confirmar, o requisito continua e a
      nota sai da spec).
- [x] CHK031 Está registrado como aceito que um `month` inválido no endereço é ignorado em silêncio e que um
      `categoryId` inválido mostra o erro genérico ou a lista vazia, sem nada que explique ao usuário que o endereço
      estava errado? [Assumption, Spec §FR-029, §Casos-limite] — ok: FR-029 (mês ignorado) e Esclarecimentos de
      2026-10-04 (o usuário viu o `categoryId` inválido e não pediu mudança).
- [x] CHK032 O link da página de categorias que ainda não leva o mês está registrado como desvio da 002, e a FR-028
      desta deixa claro que esta página já aceita os dois parâmetros e não muda? [Rastreabilidade, Spec §FR-028,
      §História 5 cenário 1; Plan §Desvios conhecidos] — ok: Plan §Desvios conhecidos (FR-004 da 002), FR-028, História
      5 cenário 1 e Esclarecimentos.
- [x] CHK033 O padrão do menu de ordenação (opção marcada, "Ordenar: …" no nome acessível e na dica, padrão inicial) é
      consistente com o da 002? [Consistência, Spec §FR-027; 002 §FR-019] — ok: a FR-027 desta e as FR-019 e FR-020 da
      002 (opção marcada, "Ordenar: …" no nome e na dica, padrão inicial).
- [x] CHK034 Os rótulos "Menor valor primeiro" e "Maior valor primeiro", que usam o valor com sinal (a maior saída
      aparece em "Menor valor primeiro"), estão registrados como claros o bastante para o usuário, ou pedem uma
      explicação? [Clareza, Spec §FR-027, §Casos-limite] — ok: os Casos-limite documentam a regra do back; comportamento
      atual aprovado com a spec.

## Layout e consistência com as outras páginas

- [x] CHK035 A FR-011 (só a lista rola) tem critério verificável, com a parte que fica no passo manual do quickstart
      identificada? [Mensurabilidade, Spec §FR-011; Plan §Trabalho previsto C5; Quickstart §2] — ok: Plan C5 e
      Quickstart §2 passo 1 (rolagem real no passo manual).
- [x] CHK036 O alvo desktop tem uma janela mínima de referência, como a 1366×768 da 001, para julgar a FR-011 e a grade
      dos filtros? [Gap, Spec §Premissas; 001 §SC-001] — ok: segunda rodada: Plan §Contexto técnico (plataforma-alvo) e
      Quickstart §2 (1366×768, como na 001).
- [x] CHK037 A estrutura da página (cabeçalho com ações, filtros ocultáveis, tabela, formulário em diálogo, snackbars)
      está documentada como o mesmo padrão das páginas de categorias e de relatórios, com as diferenças intencionais
      registradas (por exemplo, a proteção contra o clique duplo em "Excluir")? [Consistência, Plan §Constitution Check
      F1, F4; 002 §FR-012] — ok: Plan §Constitution Check V e F1 (mesmo padrão de página, filtros, tabela, diálogo,
      service e types). O exemplo do item não procede: nenhuma das páginas protege o "Excluir" contra o clique duplo
      (ver o CHK017 de performance).

## Notas

- Marque `[x]` só depois de a revisão confirmar que o critério de qualidade do requisito foi atendido.
- Deixe desmarcados os itens que ainda pedem esclarecimento, correção ou avaliação do revisor; um item que depende de
  decisão do usuário vira pergunta no portão do plan.
- Feature retroativa: os desvios conhecidos (calendários em inglês, data digitada, zero e mais de 13 algarismos, diálogo
  que fecha antes da resposta, "Nova transação" sem categorias, aviso fora dos filtros, link sem o mês, dia do
  calendário do "Mês" e consulta repetida) já foram decididos pelo usuário. Os itens sobre eles conferem se estão bem
  documentados (destino, bloqueio da entrega e critério de correção), e não reabrem a decisão.
- O `/speckit-implement` lê o estado das caixas como portão e não altera as marcações.
- O `checklists/requirements.md` tem ciclo próprio, mantido pelo `/speckit-specify` e pelo `/speckit-clarify`.
- Registre achados e comentários ao lado do item; os IDs são sequenciais para facilitar a referência.
