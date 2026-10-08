# Checklist de UX: Gestão de categorias

**Objetivo**: Testar a qualidade dos requisitos de experiência do usuário da página de categorias (spec retroativa):
acessibilidade, mensagens, estados de carregamento, erro e vazio, textos em português, consistência com as outras
páginas, paleta de cores e contraste. Confere se o que está escrito na spec e no plan é completo, claro, consistente e
verificável, conforme o `AGENTS.md` da raiz (Idioma) e o `front/AGENTS.md` (F4: Material primeiro, só desktop)
**Criado em**: 2026-10-06
**Feature**: [spec.md](../spec.md) · [plan.md](../plan.md) · [research.md](../research.md) ·
[contracts/categories-ui.md](../contracts/categories-ui.md) · [quickstart.md](../quickstart.md)

**Observação**: Checklist gerado pelo `/speckit-checklist` a partir da spec, do plan e dos artefatos da Fase 1. Os
itens testam os requisitos escritos, e não o código. Como a feature é retroativa, os desvios conhecidos, as melhorias
futuras e os comportamentos que o usuário viu e manteve (Esclarecimentos de 2026-10-04) não são reabertos: os itens só
conferem se estão bem documentados (destino, bloqueio e critério de correção).
**Responsável pela revisão**: Este checklist é um artefato de revisão da qualidade dos requisitos, de posse do revisor.
Marque `[x]` só quando o revisor concluir que o critério está atendido.
**Semântica das marcações**: `[x]` significa que o critério foi revisado e está atendido quanto à qualidade dos
requisitos. Não significa que a implementação esteja concluída.

## Acessibilidade

- [x] CHK001 A FR-020 cobre todos os controles interativos da página e do diálogo, inclusive os que têm texto visível
      ("Nova categoria", "Salvar", "Cancelar"), o link da quantidade, os campos dos filtros e o item do menu?
      [Completude, Spec §FR-020, Contracts §Cabeçalho da página] — ok: a FR-020 trata dos botões só com ícone e do grupo
      de cores; os demais controles se nomeiam pelo texto visível definido em outros requisitos (FR-001, FR-007, FR-010,
      FR-016). O link da quantidade fica no CHK002.
- [x] CHK002 O nome acessível do link da quantidade está especificado de forma que identifique a categoria e o destino,
      e não só o número (por exemplo, "14")? [Gap, Spec §FR-004, Spec §FR-020] — ok: segunda rodada (2026-10-06): documentado nos Casos-limite (bloco de acessibilidade: o link se anuncia só pelo número, com a linha como contexto) como ponto fraco e trabalho futuro de uma feature de acessibilidade (Esclarecimentos 2026-10-06, Research §D13).
- [x] CHK003 Há requisitos de navegação por teclado para a página e o diálogo: ordem de foco, foco inicial ao abrir o
      diálogo, retorno do foco ao fechar, e foco depois de excluir uma linha? [Gap, Spec §FR-007, Spec §FR-012] — ok: segunda rodada (2026-10-06): os Casos-limite registram o comportamento atual (o diálogo prende o foco e o devolve ao botão que o abriu; depois de excluir, o foco se perde), e as Premissas atribuem a acessibilidade ao Material; o foco perdido é trabalho futuro (Research §D13).
- [x] CHK004 A dica do "Excluir" desabilitado está especificada também para quem usa o teclado, e não só para quem
      "aponta para ele"? [Cobertura, Spec §FR-012, Spec §Casos-limite] — ok: segunda rodada (2026-10-06): os Casos-limite dizem que a dica do "Excluir" desabilitado aparece também com o teclado, porque o botão continua alcançável pelo Tab (`disabledInteractive`).
- [x] CHK005 Está especificado que as mensagens (snack bar) são anunciadas às tecnologias assistivas, e se o "Desfazer"
      fica alcançável pelo teclado dentro dos 5 segundos? [Gap, Spec §FR-013, Spec §FR-014] — ok: segunda rodada (2026-10-06): os Casos-limite registram que as mensagens são anunciadas sem mover o foco e que chegar ao "Desfazer" pelo teclado em 5 s é difícil; é trabalho futuro (Research §D13).
- [x] CHK006 Os tempos de 3 e 5 segundos das mensagens, em especial o do "Desfazer", estão justificados frente à
      necessidade de tempo ajustável para quem usa leitor de tela ou teclado? [Assumption, Spec §FR-011, Spec §FR-013,
      Spec §SC-004] — ok: segunda rodada (2026-10-06): os 5 s fixos do "Desfazer" estão registrados como ponto fraco aceito pelo usuário e trabalho futuro (Esclarecimentos 2026-10-06, Casos-limite, Plan §Dependências e riscos).
- [x] CHK007 O estado escolhido de cada cor da paleta está especificado com um indicador visual que não dependa só da
      cor, além do `aria-pressed` para tecnologias assistivas? [Clareza, Spec §FR-009, Spec §FR-020] — ok: FR-009 ("com
      a escolhida marcada") e US2, cenário 4; a marca é o ícone `check` (`category-form.html:21-23`), e o
      `aria-pressed` está no Data-model §Paleta padrão. O contraste dessa marca fica no CHK012.
- [x] CHK008 "Título 'Categorias'" (FR-001) diz se é o título visível da página, o título da aba do navegador, ou os
      dois? [Ambiguity, Spec §FR-001] — ok: segunda rodada (2026-10-06): a FR-001 diz "título visível 'Categorias' no cabeçalho da página", e a aba fixa em "Denarius" está nos Casos-limite como trabalho futuro.

## Paleta de cores e contraste

- [x] CHK009 O contraste de 7:1 (FR-003, SC-005) está definido com o método de medição (tons usados, temas claro e
      escuro) e com o conjunto de cores que o comprova (as 11 da paleta e os extremos preto e branco)? [Mensurabilidade,
      Spec §FR-003, Spec §SC-005, Research §Rótulo da categoria] — ok: SC-005 (qualquer cor, inclusive preto e branco,
      nos dois temas) e Research §Rótulo da categoria (tons 90 e 30, `light-dark`, conferidos em
      `mat-chip-color.spec.ts:37-45` para as cores críticas e os extremos; método da WCAG no comentário da diretiva).
- [x] CHK010 O vermelho do saldo negativo (FR-005) está definido por um token do tema, com contraste mínimo nos dois
      temas, e o sinal negativo continua visível para quem não distingue a cor? [Clareza, Gap, Spec §FR-005] — ok: a
      "formatação pt-BR" do BRL (FR-005) inclui o sinal de menos, então o negativo não depende só da cor; o token
      (`--mat-sys-error` na classe global `.negative`, `styles.scss:4-6`) é detalhe de implementação coberto pela F4.
- [x] CHK011 Há requisito de contraste para os botões da paleta contra o fundo do diálogo (componentes não textuais,
      3:1), já que cores claras como #F6BF26 e #C0CA33 podem se confundir com o fundo claro? [Gap, Spec §FR-009, Spec
      §Premissas] — ok: segunda rodada (2026-10-06): o contraste de cerca de 1,7:1 de #F6BF26 e #C0CA33 está nos Casos-limite como ponto fraco e trabalho futuro (Research §D13).
- [x] CHK012 O indicador de foco e o de cor escolhida têm contraste definido sobre qualquer uma das 11 cores da paleta,
      nos dois temas? [Gap, Spec §FR-009] — ok: segunda rodada (2026-10-06): a marca branca sobre as cores claras, com cerca de 1,7:1, está nos Casos-limite como trabalho futuro; o indicador de foco segue o do navegador e do Material (Premissas).
- [x] CHK013 A diferença entre a cor escolhida e o tom exibido no chip (Casos-limite) está explicada de forma que o
      usuário não a tome por erro, e é coerente com a "Cor personalizada" que mostra a cor exata? [Consistência, Spec
      §Casos-limite, Spec §FR-003] — ok: a FR-003 e os Casos-limite documentam a diferença como intencional (leitura em
      qualquer cor), e o diálogo mostra a cor escolhida, o que é coerente.
- [x] CHK014 Duas categorias com o mesmo nome e a mesma cor (permitido, já que os nomes não precisam ser únicos) têm
      alguma forma definida de serem distinguidas na lista? [Edge Case, Gap, Spec §Premissas] — ok: segunda rodada (2026-10-06): os Casos-limite registram que categorias com o mesmo nome e a mesma cor só se distinguem pela quantidade e pelo saldo, como comportamento aceito (Research §D15).

## Mensagens e textos em português

- [x] CHK015 Todos os textos exibidos (títulos, rótulos, dicas, mensagens, estados) estão listados num só lugar e
      coincidem entre a spec e o `contracts/categories-ui.md`? [Consistência, Spec §FR-006, Spec §FR-011, Spec §FR-013,
      Spec §FR-014, Contracts §Mensagens] — ok: a spec é a fonte dos textos, e o Contracts reproduz os do cabeçalho
      (FR-020) e das mensagens (FR-011, FR-013 e FR-014) sem divergência (conferido); uma lista única não é exigida.
- [x] CHK016 A pontuação e o estilo dos textos são consistentes (por exemplo, "Informe um nome" sem ponto final, frente
      a "Categoria criada." e "Nenhuma categoria encontrada." com ponto)? [Consistência, Spec §FR-008, Spec §FR-011] —
      ok: o padrão é consistente na aplicação: rótulos, dicas curtas e erros de campo sem ponto; frases de mensagem e de
      estado com ponto. A 003 segue o mesmo padrão ("Escolha uma categoria", "Nenhuma transação encontrada.").
- [x] CHK017 As mensagens de erro de fallback dizem ao usuário o que fazer em seguida (tentar de novo, conferir a
      conexão), ou isso está explicitamente fora do escopo? [Clareza, Spec §FR-006, Spec §FR-014] — ok: segunda rodada (2026-10-06): os Casos-limite registram que as mensagens padrão não dizem o que fazer (o caminho é o "Recarregar" ou tentar de novo), como comportamento aceito (Research §D15).
- [x] CHK018 Está especificado que as mensagens vindas da API chegam em português, ou o que a página faz quando não
      chegam? [Assumption, Spec §FR-014, Spec §FR-022] — ok: segunda rodada (2026-10-06): o Research §Mensagens registra que os `detail` do back são sempre em português e que o 400 automático do ASP.NET vem sem `detail` e cai no texto da página.
- [x] CHK019 Os formatos pt-BR estão definidos para todos os números da página: o saldo em BRL (FR-005), a quantidade
      de transações (separador de milhar) e o contador "N/100"? [Completude, Spec §FR-005, Spec §FR-008] — ok: segunda rodada (2026-10-06): os Casos-limite registram a quantidade sem separador de milhar ("1234") e o formato pt-BR só no saldo, por decisão do usuário (Esclarecimentos 2026-10-06, Research §D15); o contador não passa de 100.
- [x] CHK020 O desvio do calendário em inglês está documentado como violação de Idioma, e não como exceção, com
      destino (fluxo de bugs do front, para todos os calendários), bloqueio da entrega (D10) e critério de correção
      completo, isto é, o texto em português de cada rótulo ("Open calendar", "Choose date", "Previous year", "Next
      year", "Close calendar"), e não só o exemplo "Abrir calendário"? [Clareza, Spec §FR-020, Plan §Constitution
      Check, Research §D10] — ok: Plan §Constitution Check (linha Idioma), §Desvios conhecidos, Research §D10, FR-020
      e Casos-limite (rótulos em inglês listados). O critério "nomes acessíveis em português" é verificável contra essa
      lista; a tradução exata de cada rótulo é detalhe do `/speckit-bug-fix`, e não da spec. Fechamento (2026-10-08):
      resolvido com o `PtBrDatepickerIntl`; bug `front/bugs/calendarios-em-ingles/` `verified`.

## Estados de carregamento, erro e vazio

- [x] CHK021 Os estados da lista (carregando sem linhas, vazia, erro, recarregando com linhas) estão todos definidos,
      com o texto e o elemento visual de cada um? [Completude, Spec §FR-006, Spec §Casos-limite, Data-model §Transições]
      — ok: FR-006 (indicador, "Nenhuma categoria encontrada.", "Não foi possível carregar as categorias."), Casos-limite
      (recarga mantém as linhas) e Data-model §Transições.
- [x] CHK022 Está especificado o que acontece quando uma recarga falha com linhas na tela: as linhas dão lugar ao erro,
      ficam com um aviso, ou aparece uma mensagem? [Edge Case, Gap, Spec §FR-006, Spec §Casos-limite] — ok: a FR-006
      vale para qualquer carga ("no lugar das linhas… quando ela falha"), e os Casos-limite mantêm as linhas só "até a
      nova lista chegar"; na falha, as linhas dão lugar ao texto de erro, que é o comportamento atual
      (`categories-table.ts:36-40`, `hasValue()`).
- [x] CHK023 A mesma mensagem "Nenhuma categoria encontrada." para "não há categorias" e para "nenhuma atende aos
      filtros" é intencional, e há orientação definida para o primeiro uso (nenhuma categoria cadastrada)? [Clareza, Spec
      §FR-006, Spec §Casos-limite] — ok: os dois casos estão escritos de forma explícita (US1, cenário 5, e
      Casos-limite: "Uma busca ou uma combinação de filtros sem resultado mostra 'Nenhuma categoria encontrada.'"); no
      primeiro uso, o caminho é o "Nova categoria" do cabeçalho (FR-007).
- [x] CHK024 A exclusão da falha silenciosa da paleta da SC-007 ("nenhuma falha passa em silêncio") está explícita, já
      que a SC-007 lista só lista, salvar, excluir e desfazer? [Consistência, Spec §SC-007, Spec §Casos-limite] — ok: segunda rodada (2026-10-06): a SC-007 exclui de forma explícita a falha silenciosa da paleta, mantida pelo usuário em 2026-10-04.
- [x] CHK025 O feedback visual do diálogo enquanto a API responde (FR-010, comportamento esperado) está definido: o que
      mostra que o "Salvar" está em andamento e que um segundo "Salvar" não é aceito? [Gap, Spec §FR-010] — ok: segunda rodada (2026-10-06): o ponto em aberto da FR-010 inclui "o que indica a espera", que o `/speckit-bug-assess` propõe.

## Fluxos e casos-limite

- [x] CHK026 O link da quantidade está especificado para a categoria com quantidade zero (continua sendo link, para uma
      lista vazia, ou não)? [Edge Case, Gap, Spec §FR-004] — ok: segunda rodada (2026-10-06): os Casos-limite registram que a quantidade "0" continua link e abre a lista vazia, como comportamento aceito (Research §D15).
- [x] CHK027 A exibição de nomes longos (até 100 caracteres) no chip da tabela está definida: quebra de linha, corte
      com reticências ou largura máxima? [Edge Case, Gap, Spec §FR-003, Spec §FR-008] — ok: segunda rodada (2026-10-06): os Casos-limite, o Research §D15 e o passo 9 do Quickstart descrevem o nome longo. Conferido no CSS: o `.mat-mdc-table` do Material tem `table-layout: auto` e `min-width: 100%`, sem largura máxima na coluna; o rótulo do chip tem `white-space: nowrap`, `overflow: hidden` e `text-overflow: ellipsis`, mas, no layout automático, a célula cresce até o conteúdo, então não aparecem reticências; a tabela se alarga e o cartão rola na horizontal (`overflow: auto`, `categories-page.scss`). Com a janela larga, o nome pode caber sem rolagem, o que "nome muito longo" já ressalva.
- [x] CHK028 "Com muitas categorias" (Casos-limite: a lista rola dentro do cartão) está quantificado, ou descrito pela
      altura disponível? [Ambiguity, Spec §Casos-limite] — ok: "a lista rola dentro do seu cartão, e o cabeçalho da
      página e os filtros continuam visíveis" descreve o caso pela altura disponível, como a FR-011 da 003 ("mais
      transações do que cabem na janela").
- [x] CHK029 A perda do "Desfazer" quando outra mensagem substitui a de exclusão é coerente com a SC-004 ("pode ser
      desfeita com uma ação, em até 5 segundos"), ou a SC-004 deveria citar essa condição? [Conflict, Spec §SC-004, Spec
      §Casos-limite] — ok: segunda rodada (2026-10-06): a SC-004 cita a condição "enquanto a mensagem dela está à vista (Casos-limite)".
- [x] CHK030 Os comportamentos que o usuário viu e manteve (nome só com espaços aceito pelo diálogo, "Recarregar" sem
      indicador, falha silenciosa da paleta, preto como cor inicial sem paleta, "Cor personalizada" sobrescrita pela
      paleta atrasada, exclusão sem confirmação) estão todos registrados como comportamento atual aceito, e não como
      desvio? [Completude, Spec §Esclarecimentos, Spec §Casos-limite] — ok: o último item dos Esclarecimentos de
      2026-10-04 e os Casos-limite correspondentes, sem a marca de desvio.
- [x] CHK031 Os desvios de UX que não bloqueiam a entrega (diálogo que fecha antes da resposta, dia do "Mês" que não
      muda o filtro, link sem o mês) estão documentados com destino e critério de correção, e o ponto em aberto do
      FR-010 ("Cancelar", Esc, clique fora durante a espera) tem dono e portão definidos? [Clareza, Spec §FR-004, Spec
      §FR-010, Spec §FR-016, Plan §Desvios conhecidos] — ok: FR-004, FR-010 e FR-016 trazem o esperado (critério), o Plan
      §Desvios conhecidos traz o destino, e a FR-010 dá o ponto em aberto ao `/speckit-bug-assess`, com decisão no
      portão do bug.
- [x] CHK032 As melhorias futuras (indicador de filtro em uso com os filtros ocultos e um nome para cada cor) estão
      marcadas como fora do escopo de forma coerente entre spec e plan, com o risco de a lista "parecer incompleta"
      registrado? [Consistência, Spec §FR-015, Spec §FR-020, Plan §Desvios conhecidos] — ok: FR-015, FR-020, Premissas e
      Plan ("Melhorias futuras, em features próprias"); o risco está nos Casos-limite ("pode parecer incompleta").

## Consistência com as outras páginas

- [x] CHK033 Os padrões compartilhados com a [003-gestao-de-transacoes](../../003-gestao-de-transacoes/spec.md) e com os
      relatórios (cabeçalho com "Recarregar", "Exibir filtros"/"Ocultar filtros" e "Ordenar: …", filtros ocultos por
      padrão, campo "Mês" em `MM/aaaa`, tempos e ações das mensagens, textos de vazio e de erro) estão descritos da
      mesma forma nas specs? [Consistência, Spec §FR-006, Spec §FR-015, Spec §FR-016, Spec §FR-019] — ok: conferido
      com a FR-008, FR-009, FR-017, FR-018, FR-022, FR-023, FR-024 e FR-030 da 003; só muda a grafia do formato
      ("MM/aaaa" aqui, "mm/aaaa" na 003), sem efeito no comportamento.
- [x] CHK034 As decisões que a spec diz valerem também para transações (diálogo aberto até a resposta, filtros ocultos
      sem limpar, consulta repetida, dia do "Mês", calendário em português) estão registradas com a mesma redação nas
      duas specs? [Consistência, Spec §Esclarecimentos] — ok: o conteúdo coincide (FR-010/FR-016, FR-015/FR-023,
      FR-023/FR-031, FR-016/FR-024 e FR-020/FR-030 da 002/003); a redação não é idêntica, mas nenhuma diverge no
      comportamento.
- [x] CHK035 A posição do item "Categorias" no menu (depois de "Relatórios" e "Transações") e a marcação de página
      atual seguem a mesma regra dos outros itens do menu? [Consistência, Spec §FR-001] — ok: FR-001 e Research
      §Estrutura (um único `routerLinkActive` para todos os itens, `app.html:9-15`; ordem em `app.ts:24-26`).
- [x] CHK036 O uso de componentes e tokens do Material (F4), e o código próprio justificado (paleta de botões com
      `input type="color"` e a diretiva de contraste do chip), estão registrados como as únicas exceções visuais ao
      padrão das outras páginas? [Consistência, Plan §Constitution Check F4] — ok: Plan §Constitution Check F4.
- [x] CHK037 A premissa "só desktop" (largura do diálogo, sem layout para telas pequenas) é coerente entre a spec, o plan
      e a F4 do `front/AGENTS.md`? [Assumption, Spec §Premissas, Plan §Constitution Check F4] — ok: Spec §Premissas, Plan
      §Contexto técnico e §Constitution Check F4 (diálogo de 400 px) e `front/AGENTS.md` F4.

## Observações

- Marque `[x]` só depois que a revisão confirmar que o critério de qualidade do requisito está atendido.
- Deixe desmarcados os itens que ainda precisam de esclarecimento, correção ou avaliação do revisor.
- O `/speckit-implement` lê o estado das marcações como portão e não deve alterá-las.
- O `checklists/requirements.md` tem ciclo próprio, mantido pelo `/speckit-specify` e pelo `/speckit-clarify`.
- Registre comentários e achados junto ao item.
- Os itens são numerados em sequência para facilitar a referência.
