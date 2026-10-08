# Checklist de performance: Gestão de transações

**Objetivo**: Testar a qualidade dos requisitos de desempenho da página de transações (metas mensuráveis, trabalho na
fonte dos dados, consultas repetidas, concorrência, carregamento sob demanda e bundle), como base para a aprovação do
plan. Não testa o código.

**Criado em**: 2026-10-06

**Feature**: [spec.md](../spec.md) · [plan.md](../plan.md) · [research.md](../research.md) ·
[data-model.md](../data-model.md) · [contracts/transactions-ui.md](../contracts/transactions-ui.md) ·
[quickstart.md](../quickstart.md)

**Observação**: Checklist gerado pelo `/speckit-checklist` a partir da spec, dos artefatos do plan e da constituição
(`AGENTS.md` da raiz, princípio III, e `front/AGENTS.md`, restrições técnicas de performance).

**Responsável pela revisão**: Este checklist é um artefato de revisão da qualidade dos requisitos e pertence ao revisor.
Marque um item `[x]` só quando o revisor concluir que o critério de qualidade do requisito foi atendido.

**Semântica da marcação**: `[x]` quer dizer que o critério foi revisado e atendido quanto à qualidade do requisito. Não
quer dizer que a implementação está pronta.

## Metas e mensurabilidade

- [x] CHK001 A ausência de meta de tempo (Premissas) é coerente com o princípio III, que pede metas que importam ao
      usuário como critérios mensuráveis na spec? Ela está registrada como decisão explícita, com o motivo, e não como
      lacuna? [Assumption, Spec §Premissas; Plan §Metas de desempenho; AGENTS.md §III] — ok: Premissas (sem meta de
      tempo; volume de finanças pessoais) e Plan §Metas de desempenho; a meta mensurável do III é a contagem de
      consultas da SC-007.
- [x] CHK002 O volume esperado ("centenas a poucos milhares de transações") está quantificado o bastante para servir de
      limite de projeto, com o ponto a partir do qual a lista sem paginação deve ser revista? [Clareza, Spec §FR-003,
      §Premissas] — ok: segunda rodada: Premissas e Plan §Metas de desempenho (aceita até uma medição mostrar lentidão).
      Obs. (LOW): "centenas de transações por mês" (Esclarecimentos de 2026-10-06, Premissas, Plan §Metas) e "centenas a
      poucos milhares de transações" (Premissas, Plan §Escala) só concordam para cerca de um ano de uso; uniformizar a
      redação.
- [x] CHK003 Cada parte da SC-007 (abertura, mudança de filtro ou de ordenação, repetição da mesma escolha, busca) pode
      ser verificada objetivamente, com o método de medição definido (testes com `HttpTestingController` e `verify()`,
      passo 8 do quickstart)? [Mensurabilidade, Spec §SC-007; Research §Abordagem de testes; Quickstart §2] — ok:
      Research §Abordagem de testes (`HttpTestingController` e `verify()`), o debounce em `transactions-filters.spec.ts`
      e o passo 8 do Quickstart §2; as partes da repetição nascem com o bug-fix da FR-031.
- [x] CHK004 "No máximo uma consulta por pausa de 300 ms na digitação" está definido sem ambiguidade: de onde a pausa é
      contada e o que vale quando o usuário sai do campo logo depois de uma pausa que já aplicou o texto? [Clareza, Spec
      §SC-007, §FR-025, §Casos-limite] — ok: FR-025 (pausa na digitação) e Casos-limite ("Sair do campo depois de uma
      pausa que aplicou um texto novo não refaz nada").
- [x] CHK005 A parte da SC-007 que diz que mudar filtro ou ordenação não busca as categorias de novo tem cobertura
      declarada, e o plan deixa claro que ela é só implícita (o `verify()` quebra com uma requisição a mais)?
      [Rastreabilidade, Spec §SC-007; Research §Abordagem de testes] — ok: Research §Abordagem de testes declara a
      cobertura implícita pelo `verify()`.

## Trabalho na fonte dos dados

- [x] CHK006 A FR-031 exige filtragem e ordenação na API. O plan deixa claro que, do lado do back, a API carrega tudo e
      filtra e ordena em memória, que isso está fora do escopo do front e onde está documentado no back? [Clareza, Spec
      §FR-031; Plan §Constitution Check III; Research §Observação de desempenho fora do escopo do front] — ok: Plan
      §Constitution Check III e Research §Observação de desempenho; conferido em
      `back/specs/002-transaction-management/plan.md:24-27,51-54`.
- [x] CHK007 As observações de desempenho fora do escopo (o `GET /categories` calcula quantidade, saldo e `canDelete`,
      que a página não usa; o back filtra em memória) têm destino ou responsável registrados, ou ficam só para "uma
      eventual decisão do usuário"? [Gap, Research §Observação de desempenho fora do escopo do front] — ok: segunda
      rodada: Research §Observação de desempenho (decisão de 2026-10-06), Premissas e Plan §Trabalho futuro (aceitas até
      uma medição; se ela mostrar lentidão, a decisão volta ao usuário num ciclo do back).
- [x] CHK008 A decisão de recarregar a lista depois de cada criação, edição, exclusão e restauração, em vez de usar o
      corpo da resposta, está justificada no research frente ao princípio III (uma consulta a mais por mutação)? [Gap,
      Contracts §API consumida; AGENTS.md §III] — ok: segunda rodada: Research §Recarga depois de cada mutação,
      Contracts §API consumida e Plan §Constitution Check III.
- [x] CHK009 A FR-010 ("Recarregar" busca as duas listas), a FR-031 (categorias só ao abrir e em "Recarregar") e as
      Entidades (voltar à página começa do zero) são consistentes, e está definido que voltar à página depois de ir a
      outra refaz as duas consultas, como comportamento aceito? [Consistência, Spec §FR-010, §FR-031, §Entidades
      principais] — ok: FR-010, FR-031 ("ao abrir a página") e Entidades (voltar começa do zero) são coerentes: voltar é
      abrir de novo e refaz as duas consultas.

## Consultas repetidas (desvio da FR-031)

- [x] CHK010 O desvio da consulta repetida (mesma ordenação, mesmo mês, mesmo texto na busca) está documentado com o
      fluxo de destino, o alcance (menu de ordenação e campo "Mês" compartilhados, filtros de nome e de descrição das
      features), o bloqueio da entrega e o critério de correção (testes que reproduzem e a SC-007)? [Completude, Spec
      §FR-031; Plan §Desvios conhecidos, §Dependências e riscos] — ok: Plan §Desvios conhecidos (FR-031, SC-007, III,
      destino, alcance e bloqueio) e §Dependências e riscos; a FR-031 diz que a correção começa por testes que
      reproduzem.
- [x] CHK011 O critério de "entregue" depois do bug-fix bloqueante está definido de forma verificável: quais cenários e
      critérios precisam passar (por exemplo, o cenário 8 da História 4 e a SC-007) e em que etapa isso é conferido (o
      fim do `/speckit-converge`)? [Clareza, Plan §Resultado do portão, §Dependências e riscos] — ok: segunda rodada:
      Plan §Resultado do portão (`test.md` com `verified`, teste de reprodução e suíte completa passando, reavaliação no
      `/speckit-converge`), Premissas e Esclarecimentos de 2026-10-06; é o mesmo critério da 002.
- [x] CHK012 Para o caso da busca com o mesmo texto, "deduzido do código e ainda não reproduzido", está definido o que
      acontece se a reprodução falhar (o requisito continua valendo e o desvio sai da spec, ou volta ao usuário)? [Gap,
      Spec §FR-031, §Casos-limite; Plan §Desvios conhecidos] — ok: segunda rodada: Casos-limite (as duas notas de desvio
      deduzido), Premissas e Plan §Resultado do portão e §Desvios conhecidos.
- [x] CHK013 A FR-024 ("um dia do mês já aplicado não muda nada") e a FR-031 ("o mesmo mês não refaz a consulta") usam a
      mesma definição de "mesmo mês", qualquer que seja o dia escolhido? [Consistência, Spec §FR-024, §FR-031] — ok: a
      FR-024 remete à FR-031, e os Casos-limite usam o mesmo mês do calendário ("um dia do mês já aplicado não muda
      nada").
- [x] CHK014 A regra de não repetir consultas está escrita da mesma forma nesta spec e na
      [002-gestao-de-categorias](../../002-gestao-de-categorias/spec.md), já que a correção é uma só nos componentes
      compartilhados? [Consistência, Spec §FR-031; 002 §FR-023] — ok: a FR-031 desta e a FR-023 da 002 têm a mesma regra
      (ordenação, mês e texto aplicado) e a mesma correção única.

## Concorrência e respostas fora de ordem

- [x] CHK015 Há requisito para mudanças de filtro ou de ordenação em sequência rápida: só a resposta da consulta mais
      recente aparece, e as anteriores são canceladas ou descartadas? [Gap, Spec §FR-008, §FR-031] — ok: segunda rodada:
      Research §Estados de carregamento, vazio e erro (comportamento atual do `httpResource`, aceito).
- [x] CHK016 Há requisito para cliques repetidos em "Recarregar" (uma consulta por clique, ou nenhuma nova enquanto
      outra está em andamento)? [Gap, Spec §FR-010] — ok: segunda rodada: Research §Estados de carregamento, vazio e
      erro (comportamento atual do `reload()`, aceito).
- [x] CHK017 O clique duplo em "Excluir", que envia uma segunda requisição que a API recusa, está registrado como aceito
      frente ao princípio III ("nada de requisições repetidas sem necessidade")? [Assumption, Spec §Casos-limite;
      AGENTS.md §III] — ok: segunda rodada: FR-020, Casos-limite e Esclarecimentos de 2026-10-06 (defeito do III que
      bloqueia a entrega); Plan §Constitution Check III, §Resultado do portão e §Desvios conhecidos.
- [x] CHK018 O requisito "sem aceitar um segundo Salvar" (FR-016) está ligado explicitamente à prevenção de requisições
      de criação duplicadas, e o desvio atual (o diálogo fecha antes da resposta) está avaliado também por esse efeito?
      [Cobertura, Spec §FR-016; Plan §Desvios conhecidos] — ok: a FR-016 ("sem aceitar um segundo Salvar") é clara; hoje
      o diálogo fecha no primeiro "Salvar", então o desvio não duplica criações, e o efeito dele (o que foi digitado se
      perde) está registrado.

## Volume e renderização

- [x] CHK019 Há meta ou critério para exibir a lista inteira, sem paginação nem rolagem virtual, no maior volume
      previsto, ou a decisão está registrada como aceita até uma medição mostrar o contrário ("primeiro meça, depois
      otimize")? [Gap, Spec §FR-003, §Premissas; AGENTS.md §III] — ok: segunda rodada: Premissas e Plan §Metas de
      desempenho (aceita até uma medição mostrar lentidão).
- [x] CHK020 Os requisitos de carregamento distinguem com clareza a primeira carga e a mudança de filtro ou de ordenação
      (indicador no lugar das linhas) da recarga da mesma lista (linhas mantidas), de forma consistente com a tabela de
      estados do data-model? [Consistência, Spec §FR-008, §Casos-limite; Data-model §Estados do recurso] — ok: FR-008,
      Casos-limite e Data-model §Estados do recurso, coerentes entre si.

## Debounce

- [x] CHK021 Os 300 ms da busca e a aplicação imediata ao sair do campo (FR-025) estão justificados e alinhados com a
      busca por nome da 002 e com a SC-007? [Consistência, Spec §FR-025, §SC-007; 002 §FR-017] — ok: a FR-025 e a FR-017
      da 002 têm os mesmos 300 ms e a saída do campo, e a SC-007 usa a mesma pausa.

## Carregamento sob demanda e bundle

- [x] CHK022 O "carregada só quando aberta" da FR-001 tem critério mensurável (a página é um chunk lazy próprio e nada
      dela entra no bundle inicial), com a forma de conferir definida no quickstart? [Mensurabilidade, Spec §FR-001;
      Quickstart §1; front/AGENTS.md §Restrições técnicas] — ok: FR-001, Plan §Constitution Check (restrições técnicas:
      `loadChildren`) e Quickstart §1 (chunk lazy `transactions-page`).
- [x] CHK023 Os números de referência do bundle ("Initial total" de 633,08 kB, com aviso em 700 kB; chunk
      `transactions-page` de 39,88 kB) estão datados e ligados a um commit, e está definido o que conta como regressão
      (só os budgets do `angular.json` ou também uma variação do chunk)? [Clareza, Plan §Metas de desempenho; Quickstart
      §1] — ok: segunda rodada: Plan §Metas de desempenho e Quickstart §1 (só os budgets do `angular.json` são portão;
      os números são referência). Obs. (LOW): o Quickstart espera o build "sem erro nem aviso de budget", mais estrito
      que o `front/AGENTS.md` ("sem erro de budget"); deixar claro se o aviso também reprova.
- [x] CHK024 Há meta ou limite para o chunk lazy da página, ou está registrado que só os budgets do bundle inicial e dos
      estilos por componente se aplicam? [Gap, Plan §Metas de desempenho; front/AGENTS.md §Restrições técnicas] — ok:
      segunda rodada: Plan §Metas de desempenho e Quickstart §1 (não há budget para chunk lazy).
- [x] CHK025 As correções previstas nos componentes compartilhados (os rótulos do calendário em português, que tendem a
      ser providos na configuração da aplicação, o campo "Mês" e o menu de ordenação) estão avaliadas quanto ao efeito
      no bundle inicial, que tem cerca de 67 kB de margem até o aviso? [Gap, Plan §Dependências e riscos, §Metas de
      desempenho] — ok: segunda rodada: Plan §Dependências e riscos (bundle inicial na correção da FR-030, com a margem
      de cerca de 67 kB), como na 002.
- [x] CHK026 Está documentado que a feature não traz dependência pesada e que o Chart.js, dos relatórios, não entra no
      chunk dela? [Completude, Plan §Contexto técnico; front/AGENTS.md §Restrições técnicas] — ok: Plan §Contexto
      técnico.
- [x] CHK027 O limite de estilos por componente (aviso em 4 kB) tem evidência no plan para os `.scss` da feature?
      [Rastreabilidade, Plan §Contexto técnico, §Constitution Check F4; front/AGENTS.md §Restrições técnicas] — ok: Plan
      §Constitution Check F4 (os quatro `.scss` têm de 5 a 18 linhas) e restrições técnicas (budgets respeitados no
      build de 2026-10-05).

## Notas

- Marque `[x]` só depois de a revisão confirmar que o critério de qualidade do requisito foi atendido.
- Deixe desmarcados os itens que ainda pedem esclarecimento, correção ou avaliação do revisor; um item que depende de
  decisão do usuário vira pergunta no portão do plan.
- Feature retroativa: a consulta repetida (princípio III, FR-031) já foi decidida pelo usuário como defeito que bloqueia
  a entrega. Os itens sobre ela conferem se está bem documentada (destino, bloqueio e critério de correção), e não
  reabrem a decisão.
- O `/speckit-implement` lê o estado das caixas como portão e não altera as marcações.
- O `checklists/requirements.md` tem ciclo próprio, mantido pelo `/speckit-specify` e pelo `/speckit-clarify`.
- Registre achados e comentários ao lado do item; os IDs são sequenciais para facilitar a referência.
- Converge final (2026-10-08): a consulta repetida (FR-031, SC-007) e o clique repetido em "Excluir" (FR-020) foram
  corrigidos (`front/bugs/consulta-repetida-sem-mudanca/` e `front/bugs/clique-repetido-em-excluir/`, `test.md`
  `verified`) e reavaliados contra o código sem achados. Nenhum item mudou de estado.
