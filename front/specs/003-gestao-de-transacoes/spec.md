# Especificação da feature: Gestão de transações

**Branch da feature**: `003-gestao-de-transacoes`

**Criada em**: 2026-10-04

**Status**: Rascunho

**Entrada**: Descrição do usuário: "Especificação retroativa da página de transações do front (`/transactions`, também a
página inicial da aplicação), já implementada antes da adoção do Spec Kit, derivada do código atual em
`front/src/app/transactions` e dos seus testes. Cobre o que o usuário vê e faz: o item 'Transações' no menu e a rota
carregada sob demanda; a lista de transações com data, descrição, categoria (nome na cor dela) e valor em BRL, com as
transações do mesmo dia agrupadas visualmente; os filtros por descrição, tipo (entradas ou saídas), categoria e mês; a
abertura já filtrada por categoria ou por mês a partir do endereço (`?categoryId=` vindo da página de categorias e
`?month=YYYY-MM` vindo do painel de relatórios); a ordenação pelo menu; a criação e a edição num diálogo (tipo
entrada/saída, valor em reais, data, categoria e descrição opcional); a exclusão com a opção de desfazer; os estados de
carregamento, lista vazia e erro, inclusive quando as categorias não carregam; e as mensagens de sucesso e de erro,
incluindo as vindas da API. A página consome as APIs de transações e de categorias documentadas em
`back/specs/002-transaction-management/contracts/` e `back/specs/001-category-management/contracts/` e não muda o
back."

## Esclarecimentos

### Sessão 2026-10-04

- P: Quando o usuário digita a data no campo "Data", em vez de escolhê-la no calendário, como o campo deve interpretar
  o texto? → R: Defeito. O campo lê o texto como dia/mês/ano, com o ano em quatro algarismos e com ou sem zeros à
  esquerda ("05/09/2026" e "5/9/2026" são 5 de setembro de 2026), e recusa com "Informe uma data válida" qualquer outro
  formato, inclusive "2026-09-24", e as datas inexistentes. O atual é um desvio conhecido, corrigido pelo fluxo de
  bugs do front, fora desta spec.
- P: O formulário deve recusar, antes de fechar o diálogo, o valor zero e os valores grandes demais para serem gravados
  exatamente como digitados? → R: Sim; é um defeito nos dois casos. Com o diálogo aberto e a mensagem "Informe um
  valor válido", o formulário recusa o zero e os valores com mais de 13 algarismos na parte inteira (acima de
  R$ 9.999.999.999.999,99). O atual é um desvio conhecido, corrigido pelo fluxo de bugs do front, fora desta spec.
- P: O que "Nova transação" deve fazer quando ainda não há categorias para escolher? → R: Defeito nos dois casos. Sem
  categoria cadastrada, "Nova transação" não abre o diálogo e mostra "Cadastre uma categoria antes de registrar uma
  transação.", com a ação "Ver categorias", que abre a página de categorias; até as categorias chegarem na primeira
  carga, o botão fica desabilitado. O atual é um desvio conhecido, corrigido pelo fluxo de bugs do front, fora desta
  spec.
- P: Quando a transação salva não atende aos filtros em uso e por isso não aparece na lista recarregada, a página deve
  avisar o usuário? → R: Sim; é um defeito. A mensagem de sucesso avisa que a transação ficou fora dos filtros em uso
  (por exemplo, "Transação criada, mas ela não aparece com os filtros em uso."), e os filtros não mudam. O atual é um
  desvio conhecido, corrigido pelo fluxo de bugs do front, fora desta spec.
- P: Se a suspeita se confirmar, o que deve acontecer quando o usuário chega à visão de dias do filtro "Mês" e escolhe
  um dia? → R: Defeito. Escolher um dia aplica ao filtro o mês daquele dia, o campo e a lista sempre concordam, e
  "Limpar mês" aparece. O problema ainda precisa ser reproduzido: a correção, pelo fluxo de bugs do front e fora desta
  spec, começa por um teste que o reproduz no campo compartilhado e vale também para as páginas de categorias e de
  relatórios.
- P: O diálogo que fecha ao salvar, antes da resposta da API, e perde o que foi digitado quando ela recusa, é
  intencional, um defeito ou uma melhoria futura? → R: Defeito, decidido junto com [[002-gestao-de-categorias]]. O
  diálogo continua aberto enquanto a API responde, sem aceitar um segundo "Salvar", e só fecha quando ela aceita; na
  recusa, continua aberto com o que foi digitado, e a mensagem da API aparece. O atual é um desvio conhecido, corrigido
  pelo fluxo de bugs do front, fora desta spec.
- P: Ocultar os filtros sem limpá-los, e sem nada que indique que a lista continua filtrada, é intencional, um defeito
  ou uma melhoria futura? → R: Melhoria futura, decidida junto com [[002-gestao-de-categorias]]. Ocultar continua sem
  limpar os filtros, e isso é requisito; um indicador de filtro em uso fica como trabalho futuro, numa feature própria.
- P: Os nomes acessíveis dos calendários dos campos "Data" e "Mês", hoje em inglês, são intencionais, um defeito ou uma
  melhoria futura? → R: Defeito, decidido junto com [[002-gestao-de-categorias]]. O esperado são rótulos em português
  em todos os calendários da aplicação. O desvio foi corrigido pelo fluxo de bugs do front
  (`front/bugs/calendarios-em-ingles/`), fora desta spec.
- P: A busca por descrição, que não diferencia maiúsculas de minúsculas, mas diferencia acentos, é intencional?
  → R: Sim. O comportamento atual vira requisito como está, sem mudança nesta página nem no back; a busca por nome da
  página de categorias passa a seguir a mesma regra ([[002-gestao-de-categorias]]).
- P: O link da quantidade de transações, na página de categorias, deve levar também o mês escolhido nela? → R: Sim,
  como decidido em [[002-gestao-de-categorias]], onde fica a correção do link: com um mês escolhido, o link leva
  `?categoryId=<id>&month=YYYY-MM`. Esta página já aceita os dois juntos (FR-028) e não muda.
- P: Escolher de novo a ordenação em uso, ou o mesmo mês, refaz a consulta e troca as linhas pelo indicador de
  carregamento, embora nada tenha mudado: isso é intencional, um defeito ou uma melhoria futura? → R: Defeito, decidido
  depois da revisão pós-clarify e válido também para a [[002-gestao-de-categorias]]. O esperado é não refazer a
  consulta: a lista fica como está. O desvio, que contrariava o princípio III, foi corrigido uma só vez pelo fluxo de
  bugs do front (`front/bugs/consulta-repetida-sem-mudanca/`), fora desta spec: a comparação por conteúdo fica no
  estado de cada página (filtros e ordenação; nos relatórios, o mês), e não no menu de ordenação nem no campo "Mês"
  compartilhados, e vale também para as páginas de categorias e de relatórios.
- P: O `categoryId` inválido no endereço, o endereço que só define os filtros iniciais e a falta de divisória entre
  transações vizinhas da mesma data, em qualquer ordenação, são intencionais, defeitos ou melhorias futuras? → R: O
  usuário viu e não pediu mudança; os três ficam documentados como comportamento atual.

### Sessão 2026-10-05

- P: Na busca por descrição, redigitar o texto já aplicado, ou digitar e apagar uma letra em menos de 300 ms, refaz a
  consulta com os mesmos filtros e, nesses casos, sair do campo a refaz mais uma vez: isso é intencional, um defeito ou
  uma melhoria futura? → R: Defeito, o mesmo da consulta repetida. O esperado é que o campo "Descrição" não refaça a
  consulta quando o texto aplicado não mudou. O desvio, que contrariava o princípio III, foi reproduzido num teste e
  corrigido junto com a consulta repetida pelo fluxo de bugs do front (`front/bugs/consulta-repetida-sem-mudanca/`),
  fora desta spec, pela mesma comparação por conteúdo nos filtros da página; a correção vale também para o campo
  "Nome" da [[002-gestao-de-categorias]].
- P: O que o diálogo faz durante a espera pela resposta da API ("Cancelar", Esc, clique fora e uma resposta que chega
  depois de ele fechar)? → R: Fica para o `/speckit-bug-assess` da correção do diálogo (FR-016), que vai propor o
  comportamento para o usuário decidir no portão do bug; até lá, o ponto continua em aberto nesta spec.

### Sessão 2026-10-06

Edição autorizada pelo usuário depois da revisão dos checklists do plan, só para redação e para o registro das decisões
abaixo, sem requisito novo além do comportamento esperado dos desvios que elas classificam.

- P: A data digitada lida como mês/dia/ano e os valores com 14 ou mais algarismos inteiros, que o formulário altera ao
  converter o texto em número, ferem a integridade dos dados financeiros? → R: Sim. Os dois gravam, sem aviso, um dado
  diferente do digitado: são violações conhecidas do princípio II do `AGENTS.md` e bloqueiam a entrega desta feature até
  os seus bug-fix. O zero não fere o princípio II, porque a API o recusa; continua defeito, sem bloquear (FR-014 e
  Casos-limite).
- P: A API, que no valor só recusa o zero, deve validar a faixa e a escala? → R: Sim; é um desvio conhecido do back.
  Hoje ela arredonda sem aviso as casas além da segunda e, a partir de cerca de 10^16, responde com um erro inesperado;
  a correção, pelo fluxo de bugs do back, a faz recusar esses valores com uma mensagem clara. Não bloqueia esta feature
  (Casos-limite e Premissas).
- P: O clique repetido em "Excluir", que envia outro pedido que a API recusa, e a falta de retorno visual durante a
  exclusão e a restauração são intencionais? → R: Defeito, decidido também para a [[002-gestao-de-categorias]]. Um novo
  "Excluir" na mesma linha não deve enviar outro pedido, e a página deve mostrar que a exclusão ou a restauração está em
  andamento; a forma fica para o `/speckit-bug-assess`. O atual é um desvio conhecido, que contraria o princípio III,
  corrigido pelo fluxo de bugs do front, fora desta spec, e bloqueia a entrega desta feature (FR-020 e Casos-limite).
- P: Quais desvios bloqueiam a entrega desta feature, e quando o bug-fix de cada um conta como concluído? → R: Bloqueiam
  a consulta repetida (FR-031) e o clique repetido em "Excluir" (FR-020), pelo princípio III; os calendários em inglês
  (FR-030), pela regra de idioma; e a data digitada e os valores com 14 ou mais algarismos (FR-014), pelo princípio II.
  Um bug-fix está concluído quando o seu `front/bugs/<slug>/test.md` registra o resultado `verified`, o teste que
  reproduz o bug e a suíte completa passam e o `/speckit-converge` reavalia contra o código os requisitos e critérios
  afetados (por exemplo, o cenário 8 da História 4, as partes da SC-007 sobre repetição e a FR-030) (Premissas).
- P: E se a reprodução de um desvio deduzido do código (a busca com o mesmo texto e o dia escolhido no "Mês") não o
  confirmar? → R: O requisito continua valendo, e a nota de desvio sai da spec (Casos-limite e Premissas).
- P: Quais textos dos desvios ainda não estão fixados? → R: O texto final do aviso da FR-017, na criação e na edição; a
  duração da mensagem com "Ver categorias" (FR-019); a forma de mostrar que o "Salvar" está em andamento (FR-016); e o
  conjunto completo dos rótulos do calendário (FR-030). Os três primeiros saem do `/speckit-bug-assess` da correção, e o
  último, do bug-fix da FR-030.
- P: Em quais lugares vale a exibição como texto da SC-008? → R: Na célula "Descrição" e na etiqueta da categoria da
  lista, nas opções e no valor escolhido do filtro "Categoria" e do campo "Categoria" do formulário e nas mensagens
  (snack bars), que também exibem como texto as mensagens da API (SC-008).
- P: Como fica a acessibilidade além dos nomes dos botões (FR-030)? → R: O comportamento atual, apoiado no Angular
  Material, fica documentado nas Premissas; melhorias em teclado, anúncio dos estados da lista, prazo do "Desfazer" e
  contraste do vermelho das saídas ficam para uma feature própria de acessibilidade, junto com a
  [[002-gestao-de-categorias]], sem bloquear esta.
- P: A página tem meta de tempo, e a falta de paginação e as observações de desempenho do back são aceitas? → R: Não há
  meta de tempo, pelo uso pessoal, com centenas de transações por mês; as metas são de contagem de consultas (SC-007). A
  falta de paginação e as observações do back ficam aceitas até uma medição mostrar lentidão (Premissas).
- P: Duas categorias com o mesmo nome e a mesma cor, e mensagens padrão que não dizem o que fazer, são aceitas? → R:
  Sim, como comportamento atual (Casos-limite).
- P: Que ajustes de redação a revisão pediu? → R: A SC-004 passa a valer enquanto a mensagem da exclusão está à vista; o
  mês aparece como "MM/aaaa", como na [[002-gestao-de-categorias]]; "até as categorias chegarem na primeira carga" vira
  "até a primeira carga das categorias terminar"; os textos do "Valor" (prefixo "R$" e marcador "0,00"), o nome
  acessível "Tipo" do grupo do diálogo e o contador "N/255" ficam citados; os "Limpar …" ficam sem dica, como na 002; a
  falha das categorias aparece por 5 segundos com "Fechar"; e as mensagens da API são sempre em português, com a recusa
  automática de um pedido malformado sem mensagem (Premissas).

## Cenários de usuário e testes _(obrigatório)_

### História de usuário 1 - Revisar as transações registradas (Prioridade: P1)

Como alguém que acompanha as próprias finanças, quero abrir a aplicação e ver as minhas transações, das mais recentes
para as mais antigas, com data, descrição, categoria e valor, para conferir de onde veio e para onde foi o meu dinheiro.

**Por que esta prioridade**: É a página inicial da aplicação e a base de tudo o que esta página faz: registrar, editar,
excluir e filtrar partem da lista e se confirmam nela.

**Teste independente**: Com transações já registradas em categorias diferentes, abrir a aplicação e conferir as linhas,
as cores, os valores e os estados de carregamento, de lista vazia e de erro.

**Cenários de aceitação**:

1. **Dado que** o usuário abre a aplicação no endereço raiz ou escolhe "Transações" no menu lateral, **Quando** a página
   carrega, **Então** a lista de transações abre, com o item "Transações" marcado como a página atual.
2. **Dado que** existem transações, **Quando** a lista é exibida, **Então** cada linha mostra a data (dd/mm/aaaa), a
   descrição, o nome da categoria numa etiqueta na cor dela e o valor em reais, das mais recentes para as mais antigas.
3. **Dado que** uma transação é uma saída, **Quando** a lista é exibida, **Então** o valor aparece negativo e em
   vermelho (por exemplo, "-R$ 186,42"); uma entrada aparece sem destaque (por exemplo, "R$ 8.600,00").
4. **Dado que** duas transações vizinhas na lista têm a mesma data, **Quando** a lista é exibida, **Então** não há linha
   divisória entre elas, e a divisória só separa datas diferentes.
5. **Dado que** a página acabou de abrir, ou um filtro ou a ordenação acabou de mudar, **Quando** a lista ainda não
   chegou, **Então** um indicador de carregamento aparece no lugar das linhas.
6. **Dado que** não há transação para mostrar, **Quando** a lista carrega, **Então** aparece "Nenhuma transação
   encontrada.".
7. **Dado que** as transações ou as categorias falharam ao carregar, **Quando** a página é exibida, **Então** aparece
   "Não foi possível carregar as transações." no lugar das linhas, e o botão "Recarregar" do cabeçalho busca as duas de
   novo.
8. **Dado que** há mais transações do que cabem na janela, **Quando** o usuário rola a lista, **Então** só a lista rola,
   e o cabeçalho da página, com as suas ações, e os filtros continuam à vista.

---

### História de usuário 2 - Registrar e corrigir transações (Prioridade: P2)

Como alguém que anota o que recebe e o que gasta, quero registrar uma transação num diálogo, dizendo se é entrada ou
saída, o valor em reais, a data, a categoria e, se quiser, uma descrição, e corrigi-la depois, para manter o histórico
fiel.

**Por que esta prioridade**: É o que alimenta a lista: sem registrar e corrigir transações, a página não tem o que
mostrar. Fica atrás da História 1 porque o resultado de cada registro se confere na lista.

**Teste independente**: Criar uma transação por "Nova transação", conferir que ela aparece na lista com o sinal e a
categoria escolhidos, abri-la por "Editar", mudar o valor e conferir a mudança na lista.

**Cenários de aceitação**:

1. **Dado** a página de transações, com categorias cadastradas, **Quando** o usuário escolhe "Nova transação",
   **Então** abre o diálogo "Nova transação" com "Saída" marcado, o valor vazio, a data de hoje e a primeira categoria
   da lista escolhida.
2. **Dado** o diálogo com "Saída", o valor "12,50" e a descrição "Pão" digitada com espaços antes e depois, **Quando** o
   usuário escolhe "Salvar", **Então** o diálogo continua aberto, sem aceitar um segundo "Salvar", até a API responder;
   quando ela aceita, o diálogo fecha, a transação é registrada com o valor -12,50 e a descrição "Pão", aparece
   "Transação criada." e a lista recarrega com os filtros e a ordenação em uso (desvio conhecido: hoje o diálogo fecha
   antes da resposta; ver FR-016).
3. **Dado** o diálogo com "Entrada", o valor "8600" e a descrição em branco, **Quando** o usuário salva, **Então** a
   transação é registrada com o valor positivo de 8.600,00 e sem descrição.
4. **Dado** uma transação na lista, **Quando** o usuário escolhe "Editar" na linha dela, **Então** abre o diálogo
   "Editar transação" com o tipo, o valor sem sinal (por exemplo, "186,42"), a data, a categoria e a descrição dela;
   salvar mantém o diálogo aberto até a API aceitar e então mostra "Transação salva." e recarrega a lista (desvio
   conhecido: hoje o diálogo fecha antes da resposta; ver FR-016).
5. **Dado** um valor vazio ou inválido (com letras, com sinal, com mais de duas casas decimais, como "12,345", igual a
   zero ou com mais de 13 algarismos na parte inteira), **Quando** o usuário tenta salvar, **Então** o diálogo continua
   aberto e mostra "Informe um valor válido"; da mesma forma, sem uma data válida aparece "Informe uma data válida" e,
   sem categoria, "Escolha uma categoria" (desvio conhecido: hoje o zero e os valores com mais de 13 algarismos na
   parte inteira passam; ver FR-014).
6. **Dado** o diálogo aberto, sem um "Salvar" em andamento, **Quando** o usuário escolhe "Cancelar" ou fecha o
   diálogo, **Então** nada é salvo e nenhuma mensagem aparece. O que "Cancelar", Esc ou um clique fora fazem enquanto
   a API responde a um "Salvar" está em aberto (ver FR-016).
7. **Dado que** a API recusa o salvamento (por exemplo, com "Categoria não encontrada."), **Quando** a resposta chega,
   **Então** o diálogo continua aberto, com o que foi digitado, a mensagem da API aparece com a ação "Fechar" e a lista
   não recarrega; quando a API não informa o motivo, aparece "Não foi possível salvar a transação." (desvio conhecido:
   hoje o diálogo já fechou, e o que foi digitado se perde; ver FR-016).
8. **Dado que** as categorias falharam ao carregar, **Quando** o usuário escolhe "Nova transação", **Então** o diálogo
   não abre e aparece "Não foi possível carregar as categorias. Tente novamente.".
9. **Dado** o diálogo aberto, **Quando** o usuário digita "5/9/2026" no campo "Data" e salva, **Então** a transação é
   registrada em 5 de setembro de 2026; digitada em outro formato, como "2026-09-24", a data é recusada com "Informe
   uma data válida" (desvio conhecido: hoje "5/9/2026" é salva como 9 de maio, e "2026-09-24", como 23/09/2026 no fuso
   do Brasil; ver FR-014).
10. **Dado que** não há nenhuma categoria cadastrada, **Quando** o usuário escolhe "Nova transação", **Então** o diálogo
    não abre e aparece "Cadastre uma categoria antes de registrar uma transação.", com a ação "Ver categorias", que abre
    a página de categorias; e, até a primeira carga das categorias terminar, "Nova transação" fica desabilitado
    (desvio conhecido: hoje, nos dois casos, o diálogo abre com a lista de categorias vazia; ver FR-019).
11. **Dado que** a lista está filtrada por um mês passado, **Quando** o usuário cria uma transação com a data de hoje,
    **Então** a mensagem de sucesso avisa que ela não aparece com os filtros em uso (por exemplo, "Transação criada, mas
    ela não aparece com os filtros em uso."), e os filtros continuam como estavam (desvio conhecido: hoje aparece só
    "Transação criada."; ver FR-017).

---

### História de usuário 3 - Excluir uma transação, com a opção de desfazer (Prioridade: P3)

Como alguém que corrige o próprio histórico, quero excluir uma transação com um clique e poder voltar atrás logo em
seguida, para limpar a lista sem medo de apagar a transação errada.

**Por que esta prioridade**: Corrigir com "Editar" (História 2) resolve a maior parte dos enganos; excluir é menos
frequente, mas sem isso um lançamento duplicado ficaria para sempre na lista.

**Teste independente**: Excluir uma transação, conferir que ela some da lista, escolher "Desfazer" e conferir que ela
volta com os mesmos dados.

**Cenários de aceitação**:

1. **Dado** uma transação na lista, **Quando** o usuário escolhe "Excluir" na linha dela, **Então** a transação é
   excluída na hora, sem pedido de confirmação, a lista recarrega e aparece "Transação excluída." com a ação "Desfazer"
   por 5 segundos.
2. **Dado que** a mensagem da exclusão está à vista, **Quando** o usuário escolhe "Desfazer", **Então** a transação é
   registrada de novo com a mesma data, valor, categoria e descrição, aparece "Transação restaurada." e a lista
   recarrega.
3. **Dado que** a exclusão falha (por exemplo, porque a transação já tinha sido excluída em outra janela), **Quando** a
   resposta chega, **Então** aparece a mensagem da API ("Transação não encontrada.") ou, sem motivo informado, "Não foi
   possível excluir a transação.", e a lista não recarrega.

---

### História de usuário 4 - Filtrar e ordenar a lista (Prioridade: P4)

Como alguém com um histórico longo, quero buscar pela descrição, mostrar só as entradas ou só as saídas, uma categoria
ou um mês, e escolher a ordem da lista, para achar rápido uma transação ou olhar um recorte do que recebi e gastei.

**Por que esta prioridade**: Refina a navegação numa lista que já funciona (História 1); ganha valor com o crescimento
do histórico, mas não é necessária para a página ser útil desde o primeiro dia.

**Teste independente**: Exibir os filtros, aplicar cada filtro e cada ordenação sozinhos e combinados, conferir que a
lista recarrega de acordo, e limpar e ocultar os filtros.

**Cenários de aceitação**:

1. **Dado que** a página foi aberta sem filtros no endereço, **Quando** ela carrega, **Então** os filtros estão ocultos
   e o botão "Exibir filtros" do cabeçalho os mostra; com eles à vista, o mesmo botão passa a ser "Ocultar filtros".
2. **Dado que** os filtros estão à vista, **Quando** o usuário digita parte de uma descrição, **Então**, depois de uma
   pausa de 300 ms na digitação ou assim que ele sai do campo, a lista recarrega só com as transações cuja descrição
   contém o texto, sem diferenciar maiúsculas de minúsculas, mas diferenciando acentos.
3. **Dado que** os filtros estão à vista, **Quando** o usuário escolhe o tipo "Saídas", a categoria "Mercado" e o mês
   09/2026, **Então** a lista recarrega só com as saídas de Mercado em setembro de 2026.
4. **Dado** um filtro preenchido, **Quando** o usuário escolhe o botão que o limpa ("Limpar descrição", "Limpar tipo",
   "Limpar categoria" ou "Limpar mês"), **Então** esse filtro volta a aceitar todas as transações, a lista recarrega e o
   botão some.
5. **Dado** a lista, **Quando** o usuário escolhe "Menor valor primeiro" no menu de ordenação, **Então** a lista
   recarrega nessa ordem, mantendo os filtros, e o botão do menu passa a se chamar "Ordenar: Menor valor primeiro".
6. **Dados** filtros preenchidos, **Quando** o usuário escolhe "Ocultar filtros", **Então** os filtros continuam
   valendo e a lista continua filtrada; ao exibi-los de novo, eles aparecem como estavam.
7. **Dado que** os filtros estão à vista, **Quando** o usuário vai à visão de dias do calendário do filtro "Mês" e
   escolhe um dia de outro mês (ou um dia qualquer, sem mês escolhido), **Então** a lista recarrega com o mês daquele
   dia, que o campo mostra, e "Limpar mês" aparece (desvio conhecido, ainda a reproduzir: hoje o campo mostra o mês do
   dia escolhido, mas o filtro não muda; ver FR-024).
8. **Dada** uma ordenação em uso, ou um mês escolhido, **Quando** o usuário escolhe de novo a mesma ordenação no menu,
   ou o mesmo mês no filtro "Mês", **Então** a lista fica como está, sem nova consulta e sem o indicador de
   carregamento.

---

### História de usuário 5 - Chegar à lista já filtrada a partir de outra página (Prioridade: P5)

Como alguém que está revisando uma categoria ou um mês em outra página, quero abrir as transações já filtradas por ela
ou por ele, para ver os detalhes sem escolher os filtros de novo.

**Por que esta prioridade**: Liga esta página às de categorias e de relatórios; depende dos filtros (História 4) e só
tem valor quando as outras páginas estão em uso.

**Teste independente**: Abrir `/transactions?categoryId=<id de uma categoria>`, `/transactions?month=2026-09` e os
dois juntos e conferir os filtros à vista, preenchidos, e a lista filtrada; abrir `/transactions?month=setembro` e
conferir que o mês é ignorado.

**Cenários de aceitação**:

1. **Dado** o link da quantidade de transações de uma categoria na página de categorias ([[002-gestao-de-categorias]]),
   **Quando** o usuário o segue, **Então** a página de transações abre com os filtros à vista, a categoria escolhida no
   filtro "Categoria" e a lista só com as transações dela; quando o link traz também o mês escolhido naquela página
   (depois da correção registrada na [[002-gestao-de-categorias]]), o mês aparece no filtro "Mês", e a lista fica só
   com as transações da categoria nesse mês.
2. **Dado** o "Ver todas" do painel de relatórios ([[001-reports-dashboard]]), **Quando** o usuário o segue, **Então** a
   página abre com os filtros à vista, o mês no filtro "Mês" (por exemplo, "09/2026") e a lista só com as transações
   desse mês.
3. **Dado** um endereço com um mês fora do formato YYYY-MM (por exemplo, `?month=setembro`), sem outro filtro no
   endereço, **Quando** a página abre, **Então** o mês é ignorado: a lista vem sem filtro de mês e os filtros ficam
   ocultos.

---

### Casos-limite

- Uma transação sem descrição aparece com a célula "Descrição" vazia e nunca aparece numa busca por descrição
  ([[002-transaction-management]], do backend).
- A busca por descrição não diferencia maiúsculas de minúsculas, como descreve [[002-transaction-management]], mas
  diferencia acentos, o que é comportamento observado no backend e não descrito naquela spec: "salário" acha
  "Salário", e "salario" não acha (FR-024). A busca por nome da página de categorias passa a seguir a mesma regra
  ([[002-gestao-de-categorias]]).
- A data exibida e a salva são o dia do calendário escolhido, igual em qualquer fuso horário: uma transação de
  24/09/2026 aparece em 24/09/2026 para qualquer usuário e entra no filtro de setembro de 2026.
- Na primeira carga, e ao mudar um filtro ou a ordenação, nenhuma linha aparece até as transações e as categorias
  chegarem, e o indicador de carregamento ocupa o lugar delas, porque cada linha depende do nome e da cor da sua
  categoria. Pelo mesmo motivo, uma falha só das categorias também mostra "Não foi possível carregar as transações.".
- Numa recarga da mesma lista ("Recarregar", ou depois de salvar, excluir ou restaurar), as linhas atuais continuam à
  vista, sem indicador de carregamento, até a nova versão chegar; só quando a lista estava vazia ou em erro o
  indicador ocupa o lugar da mensagem durante a recarga.
- Escolher de novo a ordenação em uso, ou o mesmo mês, não refaz a consulta, e a lista fica como está (FR-031).
- Na busca por descrição, a consulta só é refeita quando o texto aplicado muda (FR-031).
- Uma transação cuja categoria não está entre as carregadas (por exemplo, registrada em outra janela, numa categoria
  criada depois de a página abrir) aparece sem a etiqueta de categoria até o usuário escolher "Recarregar".
- Uma transação salva que não atende aos filtros em uso não aparece na lista recarregada, e a mensagem de sucesso avisa
  isso, sem mudar os filtros (FR-017): com a página aberta por "Ver todas" de um mês passado, criar uma transação com a
  data de hoje mostra, por exemplo, "Transação criada, mas ela não aparece com os filtros em uso.". Desvio conhecido:
  hoje aparece só "Transação criada.", e nada avisa que a nova transação ficou de fora da lista.
- O agrupamento por data vale em qualquer ordenação: ordenada por descrição, valor ou categoria, a lista também omite a
  divisória entre duas linhas vizinhas que por acaso têm a mesma data.
- O campo "Valor" aceita vírgula ou ponto como separador decimal ("12,50" e "12.50" valem o mesmo), mas não separador de
  milhar nem sinal: "8.600,00" e "-12" são recusados com "Informe um valor válido", e "8600" é aceito.
- O formulário recusa, com "Informe um valor válido" e o diálogo aberto, o zero ("0" ou "0,00") e os valores com mais de
  13 algarismos na parte inteira (acima de R$ 9.999.999.999.999,99), porque a partir daí o valor gravado poderia diferir
  do digitado (FR-014). Desvio conhecido: hoje o formulário não limita o valor. A alteração dos valores com 14 ou mais
  algarismos fere a integridade dos dados (princípio II do `AGENTS.md`) e bloqueia a entrega desta feature; o zero, que
  a API recusa, não bloqueia. O zero passa pela validação; ao salvar, o diálogo fecha e a API recusa a transação com "O
  valor da transação não pode ser zero.". Com centavos, a partir de 14 algarismos na parte inteira o valor salvo pode
  diferir do digitado, sem aviso: "99999999999999,99" é salvo como 99.999.999.999.999,98, e "999999999999999,99", como
  1.000.000.000.000.000,00. A partir de cerca de 10^16 (dez quatrilhões), o formulário aceita o valor e a API o recusa
  com "Ocorreu um erro inesperado.". A API só recusa o zero: não limita a faixa nem a escala do valor, arredonda sem
  aviso as casas além da segunda e, a partir de cerca de 10^16, responde com o erro inesperado. É um desvio conhecido do
  back, corrigido pelo fluxo de bugs do back, que não bloqueia esta feature.
- Com os campos válidos, "Salvar" mantém o diálogo aberto até a resposta da API, sem aceitar um segundo "Salvar";
  quando a API recusa, a mensagem aparece, e o diálogo continua aberto com o que foi digitado (FR-016). Desvio
  conhecido: hoje o diálogo fecha assim que o usuário escolhe "Salvar", antes da resposta da API; quando ela recusa, os
  dados digitados não voltam, e o usuário precisa abrir o diálogo e preenchê-lo de novo. O comportamento durante a
  espera ("Cancelar", Esc, clique fora ou uma resposta que chega com o diálogo já fechado) está em aberto (FR-016).
- Uma data digitada no campo "Data", em vez de escolhida no calendário, é lida como dia/mês/ano, com o ano em quatro
  algarismos e com ou sem zeros à esquerda: "05/09/2026" e "5/9/2026" são 5 de setembro de 2026. Qualquer outro formato,
  inclusive "2026-09-24", e as datas inexistentes, como "31/02/2026", são recusados com "Informe uma data válida"
  (FR-014). Desvio conhecido, que fere a integridade dos dados (princípio II) e bloqueia a entrega desta feature: hoje o
  campo não segue o dia/mês/ano que exibe. "05/09/2026" é lida como mês/dia/ano e salva como 9 de maio de 2026;
  "24/09/2026" é recusada; e "2026-09-24" é lida como meia-noite do horário universal, que no fuso do Brasil ainda é
  23/09, e é salva como 23/09/2026. O campo só mostra a data reinterpretada depois que o usuário sai dele; salvando com
  Enter logo depois de digitar, o diálogo fecha sem mostrá-la.
- A descrição para de aceitar texto aos 255 caracteres, e o contador mostra quantos foram usados; espaços nas pontas são
  removidos, e uma descrição só com espaços é salva como "sem descrição".
- O campo "Data" aceita qualquer dia, passado ou futuro, sem limite.
- Sem nenhuma categoria cadastrada, "Nova transação" não abre o diálogo e mostra "Cadastre uma categoria antes de
  registrar uma transação.", com a ação "Ver categorias", que abre a página de categorias; e, até a primeira carga das
  categorias terminar, "Nova transação" fica desabilitado (FR-019). Desvio conhecido: hoje, nos dois casos, o diálogo
  abre com a lista de categorias vazia (as que chegam depois de ele abrir não aparecem nele), e "Salvar" só mostra
  "Escolha uma categoria", sem indicar como cadastrar uma.
- Os botões de navegação do calendário, no campo "Data" e no filtro "Mês", devem se anunciar aos leitores de tela em
  português (FR-030).
- No filtro "Mês", o calendário abre na visão dos meses do ano, mas o botão do ano, no topo dele, leva à visão dos dias;
  escolher um dia ali aplica ao filtro o mês daquele dia, e o campo e a lista sempre concordam (FR-024); um dia do mês
  já aplicado não muda nada (FR-031). Desvio conhecido, deduzido da leitura do código e ainda não reproduzido: hoje
  escolher um dia fecha o calendário e põe no campo o mês daquele dia sem mudar o filtro, e o campo passa a mostrar um
  MM/aaaa que a lista não usa; se antes não havia mês, nem aparece "Limpar mês". Se a reprodução não confirmar o desvio,
  o requisito continua valendo e esta nota sai da spec.
- A exclusão não pede confirmação. "Desfazer" só existe enquanto a mensagem está à vista (5 segundos); qualquer outra
  mensagem que apareça nesse intervalo (por exemplo, a de outra exclusão) toma o lugar dela, e a exclusão anterior não
  pode mais ser desfeita.
- A transação restaurada volta como um registro novo com os mesmos dados (a API não desfaz exclusões), e uma falha ao
  restaurá-la mostra as mesmas mensagens de um salvamento recusado.
- Editar ou excluir uma transação que já não existe (removida em outra janela) é recusado pela API com "Transação não
  encontrada.", e a linha continua na lista até o usuário escolher "Recarregar".
- Escolher "Excluir" de novo na mesma linha antes de a lista recarregar (por exemplo, num clique duplo) não envia outro
  pedido, e a página mostra que a exclusão, ou a restauração, está em andamento (FR-020). Desvio conhecido, que
  contraria o princípio III e bloqueia a entrega desta feature: hoje o segundo clique envia outro pedido, que não exclui
  nada a mais; a API o recusa (por exemplo, com "Transação não encontrada."), e essa mensagem toma o lugar da que
  oferece "Desfazer", então a exclusão não pode mais ser desfeita; e nada na página indica que a exclusão ou a
  restauração está em andamento.
- Editar a mesma transação em duas janelas não gera aviso de conflito: vale a última gravação, e a outra janela continua
  mostrando a versão anterior até o usuário escolher "Recarregar".
- Ocultar os filtros não os limpa (FR-023): a lista continua filtrada, e nada no cabeçalho indica que há um filtro em
  uso; um indicador de filtro em uso é trabalho futuro.
- Duas categorias com o mesmo nome e a mesma cor não se distinguem nas opções do filtro "Categoria" nem do campo
  "Categoria" do formulário; é comportamento atual aceito.
- As mensagens padrão ("Não foi possível carregar as transações.", "Não foi possível salvar a transação." e "Não foi
  possível excluir a transação.") não dizem o que fazer em seguida; é comportamento atual aceito.
- Uma combinação de filtros sem resultado mostra "Nenhuma transação encontrada.", e não um erro; uma busca só com
  espaços equivale a não buscar ([[002-transaction-management]]).
- "Menor valor primeiro" e "Maior valor primeiro" usam o valor com sinal: a primeira põe as maiores saídas no topo, e a
  segunda, as maiores entradas ([[002-transaction-management]]).
- Em "Descrição (A–Z)", as transações sem descrição vêm primeiro e, em "Descrição (Z–A)", por último (comportamento
  observado no backend, que põe a ausência de descrição antes de qualquer texto).
- A categoria do endereço é usada como veio: um identificador malformado faz a lista mostrar "Não foi possível carregar
  as transações.", com os filtros à vista e o filtro "Categoria" sem nome escolhido; um identificador bem formado de uma
  categoria que não existe traz a lista vazia. Nos dois casos, "Recarregar" traz o mesmo resultado, e "Limpar
  categoria" volta à lista completa.
- O endereço só define os filtros iniciais: os filtros escolhidos depois não o alteram, e escolher "Transações" no menu
  com a página já aberta não a reabre, então os filtros e a ordenação em uso continuam, inclusive os que vieram do
  endereço.

## Requisitos _(obrigatório)_

### Requisitos funcionais

- **FR-001**: A aplicação DEVE oferecer a página de transações em `/transactions`, com o cabeçalho "Transações",
  carregada só quando aberta, e um item "Transações" no menu lateral, marcado como a página atual enquanto ela está
  aberta.
- **FR-002**: O endereço raiz da aplicação DEVE levar à página de transações, que é a página inicial.
- **FR-003**: A página DEVE listar as transações numa tabela com as colunas "Data", "Descrição", "Categoria" e "Valor" e
  as ações "Editar" e "Excluir" em cada linha, na ordem devolvida pela API, todas de uma vez, sem paginação.
- **FR-004**: A data DEVE aparecer como dd/mm/aaaa e ser o dia do calendário registrado, igual em qualquer fuso horário.
- **FR-005**: A categoria DEVE aparecer como uma etiqueta com o nome dela, preenchida com um tom claro da cor da
  categoria e escrita num tom escuro da mesma cor (os dois trocam de lugar no tema escuro), com contraste de pelo menos
  7:1 entre eles.
- **FR-006**: O valor DEVE aparecer em reais, com formatação pt-BR: as saídas com sinal negativo e em vermelho (por
  exemplo, "-R$ 186,42") e as entradas sem destaque (por exemplo, "R$ 8.600,00").
- **FR-007**: Transações vizinhas na lista com a mesma data DEVEM aparecer agrupadas, sem linha divisória entre elas.
- **FR-008**: Na primeira carga e a cada mudança de filtro ou de ordenação, a página DEVE mostrar um indicador de
  carregamento no lugar das linhas até receber as transações e as categorias; numa recarga da mesma lista, DEVE manter
  as linhas atuais à vista até a nova versão chegar.
- **FR-009**: Sem transações para mostrar, a página DEVE exibir "Nenhuma transação encontrada."; se as transações ou as
  categorias falharem ao carregar, DEVE exibir "Não foi possível carregar as transações." no lugar das linhas.
- **FR-010**: O botão "Recarregar" do cabeçalho DEVE buscar de novo as transações e as categorias.
- **FR-011**: Com mais transações do que cabem na janela, só a lista DEVE rolar; o cabeçalho da página, com as suas
  ações, e os filtros DEVEM continuar à vista.
- **FR-012**: "Nova transação", no cabeçalho, DEVE abrir o diálogo "Nova transação" (salvo nos casos da FR-019), e
  "Editar", numa linha, DEVE abrir o diálogo "Editar transação", ambos com o tipo ("Entrada" ou "Saída", num grupo com o
  nome acessível "Tipo"), o "Valor" em reais e sem sinal (com o prefixo "R$" e o marcador "0,00" quando vazio), a "Data"
  com calendário, a "Categoria", escolhida entre as categorias cadastradas em ordem de nome, e a "Descrição" opcional, e
  com os botões "Cancelar" e "Salvar".
- **FR-013**: O diálogo de uma transação nova DEVE começar com "Saída", o valor vazio, a data de hoje, a primeira
  categoria da lista e a descrição vazia; o de edição DEVE começar com os dados da transação: o tipo pelo sinal do
  valor, o valor sem sinal com vírgula e duas casas decimais (por exemplo, "186,42" e "8600,00"), a data, a categoria e
  a descrição.
- **FR-014**: O formulário DEVE exigir um valor feito só de algarismos, com até duas casas decimais separadas por
  vírgula ou ponto, diferente de zero e com até 13 algarismos na parte inteira ("Informe um valor válido"); uma data
  válida, escolhida no calendário ou digitada como dia/mês/ano, com o ano em quatro algarismos e com ou sem zeros à
  esquerda ("Informe uma data válida" para qualquer outro formato e para as datas inexistentes); e uma categoria
  ("Escolha uma categoria"). DEVE também limitar a descrição a 255 caracteres, com um contador dos caracteres usados no
  formato "N/255"; com algum campo inválido, "Salvar" DEVE apontar os erros e manter o diálogo aberto. Desvio conhecido:
  hoje o formulário aceita o zero e valores de qualquer tamanho e não lê a data digitada como dia/mês/ano (ver
  Casos-limite); a correção segue o fluxo de bugs do front, fora desta spec. A data digitada e os valores com 14 ou mais
  algarismos inteiros, gravados hoje diferentes do digitado, ferem a integridade dos dados (princípio II do `AGENTS.md`)
  e bloqueiam a entrega desta feature; o zero, que a API recusa, não bloqueia.
- **FR-015**: Ao salvar, a transação DEVE ser registrada com o valor negativo para "Saída" e positivo para "Entrada",
  com o dia escolhido como data e com a descrição sem os espaços das pontas, ou sem descrição quando ela estiver em
  branco.
- **FR-016**: Depois de "Salvar", com os campos válidos, o diálogo DEVE continuar aberto até a resposta da API, sem
  aceitar um segundo "Salvar", e fechar só quando ela aceitar; na recusa, DEVE continuar aberto com o que foi digitado e
  mostrar a mensagem (FR-018). "Cancelar", ou fechar o diálogo sem um "Salvar" em andamento, NÃO DEVE salvar nada nem
  mostrar mensagem. Desvio conhecido: hoje "Salvar" fecha o diálogo antes da resposta da API, e, numa recusa, o que foi
  digitado se perde; a correção segue o fluxo de bugs do front, fora desta spec, com a mesma decisão da página de
  categorias ([[002-gestao-de-categorias]]). Ponto em aberto, a decidir com o usuário no `/speckit-bug-assess` dessa
  correção, que vai propor o comportamento para ele decidir no portão do bug (Esclarecimentos de 2026-10-05): o que
  "Cancelar", Esc ou um clique fora fazem durante a espera e o que acontece com uma resposta que chega depois de o
  diálogo fechar. A forma de mostrar que o "Salvar" está em andamento também é definida nesse `/speckit-bug-assess`.
- **FR-017**: Um salvamento bem-sucedido DEVE mostrar "Transação criada." ou "Transação salva." por 3 segundos e
  recarregar a lista com os filtros e a ordenação em uso. Quando a transação salva não atende aos filtros em uso, e por
  isso não aparece na lista recarregada, a mensagem DEVE avisar isso (por exemplo, "Transação criada, mas ela não
  aparece com os filtros em uso."), sem mudar os filtros. Desvio conhecido: hoje a mensagem não avisa; a correção segue
  o fluxo de bugs do front, fora desta spec, e o texto final do aviso, na criação e na edição, é definido no
  `/speckit-bug-assess` dessa correção.
- **FR-018**: Um salvamento recusado DEVE mostrar, por 5 segundos e com a ação "Fechar", a mensagem da API ou, quando
  ela não informar o motivo, "Não foi possível salvar a transação.", sem recarregar a lista.
- **FR-019**: Enquanto as categorias estiverem em falha, "Nova transação" NÃO DEVE abrir o diálogo e DEVE mostrar "Não
  foi possível carregar as categorias. Tente novamente.", por 5 segundos e com a ação "Fechar". Sem nenhuma categoria
  cadastrada, "Nova transação" NÃO DEVE abrir o diálogo e DEVE mostrar "Cadastre uma categoria antes de registrar uma
  transação.", com a ação "Ver categorias", que abre a página de categorias; e, até a primeira carga das categorias
  terminar, "Nova transação" DEVE ficar desabilitado. Desvio conhecido: hoje, sem categoria cadastrada ou antes de elas
  chegarem, o diálogo abre com a lista de categorias vazia e não consegue salvar; a correção segue o fluxo de bugs do
  front, fora desta spec, e a duração da mensagem com "Ver categorias" é definida no `/speckit-bug-assess` dessa
  correção.
- **FR-020**: "Excluir" DEVE excluir a transação na hora, sem pedir confirmação, recarregar a lista e mostrar "Transação
  excluída." com a ação "Desfazer" por 5 segundos. Um novo "Excluir" na mesma linha antes de a lista recarregar NÃO DEVE
  enviar outro pedido, e a página DEVE mostrar que a exclusão, ou a restauração (FR-021), está em andamento. Desvio
  conhecido: hoje um clique repetido envia outro pedido, que a API recusa, e nada indica a exclusão ou a restauração em
  andamento (ver Casos-limite); isso contraria o princípio III e bloqueia a entrega desta feature. A correção segue o
  fluxo de bugs do front, fora desta spec, e a forma do retorno visual é definida no `/speckit-bug-assess` dessa
  correção.
- **FR-021**: "Desfazer" DEVE registrar de novo a transação excluída, com a mesma data, valor, categoria e descrição,
  mostrar "Transação restaurada." por 3 segundos e recarregar a lista; uma recusa DEVE ser tratada como em FR-018.
- **FR-022**: Uma exclusão recusada DEVE mostrar, por 5 segundos e com a ação "Fechar", a mensagem da API ou, sem motivo
  informado, "Não foi possível excluir a transação.", sem recarregar a lista.
- **FR-023**: Os filtros DEVEM começar ocultos, salvo quando o endereço traz um filtro (FR-028), e o botão "Exibir
  filtros"/"Ocultar filtros" do cabeçalho DEVE mostrá-los e ocultá-los; ocultá-los NÃO DEVE limpá-los: a lista continua
  filtrada e, ao exibi-los de novo, eles aparecem como estavam. Um indicador de filtro em uso, visível com os filtros
  ocultos, é trabalho futuro, numa feature própria, fora desta spec.
- **FR-024**: A página DEVE oferecer os filtros "Descrição" (busca por parte da descrição, sem diferenciar maiúsculas de
  minúsculas, mas diferenciando acentos), "Tipo" ("Todos", "Entradas" ou "Saídas"), "Categoria" ("Todas" ou uma das
  categorias, em ordem de nome) e "Mês" (escolhido num calendário de meses, exibido como MM/aaaa, e "Todos os meses"
  quando vazio), e DEVE mostrar só as transações que atendem a todos os filtros preenchidos. No "Mês", escolher um dia
  na visão de dias, aberta pelo botão do ano, DEVE aplicar o mês daquele dia, com o "Limpar mês" à vista, e o campo e
  a lista DEVEM sempre concordar; um dia do mês já aplicado não muda nada (FR-031). Desvio conhecido, deduzido do
  código e ainda não reproduzido: hoje escolher um dia muda o texto do campo sem mudar o filtro (ver Casos-limite); a
  correção segue o fluxo de bugs do front, no campo compartilhado, fora desta spec, começa por um teste que reproduz o
  problema e vale também para as páginas de categorias e de relatórios.
- **FR-025**: A busca por descrição DEVE ser aplicada depois de uma pausa de 300 ms na digitação, ou assim que o usuário
  sai do campo; os outros filtros DEVEM ser aplicados assim que escolhidos.
- **FR-026**: Cada filtro preenchido DEVE oferecer um botão que o limpa ("Limpar descrição", "Limpar tipo", "Limpar
  categoria", "Limpar mês"), visível só enquanto o filtro está preenchido, e limpar o tipo ou a categoria NÃO DEVE abrir
  a lista de opções do filtro.
- **FR-027**: Um menu de ordenação no cabeçalho DEVE oferecer "Mais recentes primeiro" (o padrão), "Mais antigos
  primeiro", "Descrição (A–Z)", "Descrição (Z–A)", "Categoria (A–Z)", "Categoria (Z–A)", "Maior valor primeiro" e
  "Menor valor primeiro", com a opção em uso marcada no menu e nomeada no rótulo acessível e na dica do botão ("Ordenar:
  Mais recentes primeiro"); trocar a ordenação DEVE manter os filtros, e mudar os filtros DEVE manter a ordenação.
- **FR-028**: A página DEVE aceitar no endereço uma categoria (`?categoryId=<id>`) e um mês (`?month=YYYY-MM`), juntos
  ou separados, e abrir com os filtros à vista e preenchidos com eles. A categoria vem do link da página de categorias,
  que deve levar também o mês escolhido nela (desvio conhecido registrado em [[002-gestao-de-categorias]]), e o mês vem
  também do painel de relatórios (FR-013 de [[001-reports-dashboard]]).
- **FR-029**: Um mês no endereço fora do formato YYYY-MM (como `setembro`, `2026-9` ou `2026-09-01`) ou com um mês
  inexistente (como `2026-13` ou `2026-00`) DEVE ser ignorado, como se o endereço não trouxesse mês.
- **FR-030**: Os botões que só mostram um ícone DEVEM ter o nome da ação para leitores de tela ("Recarregar", "Exibir
  filtros"/"Ocultar filtros", "Ordenar: …", "Editar", "Excluir" e os "Limpar …" dos filtros), e os do cabeçalho e das
  linhas DEVEM mostrá-lo também como dica; os "Limpar …" dos filtros ficam sem dica. Os nomes acessíveis dos calendários
  dos campos "Data" e "Mês" DEVEM estar em português (por exemplo, "Abrir calendário"), e vale para todos os
  calendários da aplicação; o conjunto completo desses rótulos está em `front/bugs/calendarios-em-ingles/`.
- **FR-031**: A filtragem e a ordenação DEVEM acontecer na fonte dos dados (a API), com uma única nova consulta da lista
  a cada mudança de filtro ou de ordenação; as categorias DEVEM ser buscadas só ao abrir a página e em "Recarregar".
  Escolher de novo a ordenação em uso, ou o mesmo mês, NÃO DEVE refazer a consulta, e a lista DEVE ficar como está; da
  mesma forma, a busca por descrição NÃO DEVE refazer a consulta quando o texto aplicado não mudou. Esse comportamento
  foi corrigido pelo fluxo de bugs do front (`front/bugs/consulta-repetida-sem-mudanca/`), fora desta spec, com uma
  comparação por conteúdo no estado da página (filtros e ordenação), e não no menu de ordenação nem no campo "Mês"
  compartilhados; a mesma correção vale para as páginas de categorias (inclusive a busca por nome) e de relatórios.
- **FR-032**: Descrições, nomes de categoria e mensagens da API DEVEM ser exibidos como texto, nunca interpretados como
  marcação.

### Entidades principais _(inclua se a feature envolver dados)_

- **Transação** _(de [[002-transaction-management]], do backend)_: o que a página lista, cria, edita e exclui — um dia
  do calendário, um valor com sinal (positivo para entrada, negativo para saída, nunca zero), uma categoria e uma
  descrição opcional de até 255 caracteres.
- **Categoria** _(de [[001-category-management]], do backend)_: dá nome e cor às linhas e é a opção dos filtros e do
  formulário; a página usa a lista completa, na ordem em que a API a devolve (por nome).
- **Filtros e ordenação da lista**: a descrição buscada, o tipo, a categoria, o mês e a ordenação escolhida no menu.
  Valem enquanto a página está aberta, com o painel de filtros visível ou oculto: voltar a ela depois de ir a outra
  página começa sem filtros e com "Mais recentes primeiro", salvo os filtros que o endereço trouxer.

## Critérios de sucesso _(obrigatório)_

### Resultados mensuráveis

- **SC-001**: Ao abrir a aplicação, o usuário vê as suas transações, das mais recentes para as mais antigas, sem nenhuma
  ação além de abrir o endereço.
- **SC-002**: O usuário registra ou corrige uma transação com uma única abertura do diálogo e um "Salvar" e, quando
  ela atende aos filtros em uso, a vê na lista em seguida, sem recarregar a página; quando não atende, a mensagem de
  sucesso o avisa disso (desvio conhecido: hoje a mensagem não avisa; ver FR-017).
- **SC-003**: Em 100% das linhas, o usuário distingue uma entrada de uma saída pelo sinal e pela cor do valor, sem abrir
  a transação; e, em 100% das linhas cuja categoria está entre as carregadas, reconhece a categoria pelo nome e pela
  cor.
- **SC-004**: Uma exclusão feita por engano é desfeita com um único clique, em até 5 segundos, enquanto a mensagem dela
  está à vista (Casos-limite), e a transação volta com a mesma data, valor, categoria e descrição.
- **SC-005**: Vindo da página de categorias ou do painel de relatórios, o usuário vê as transações daquela categoria ou
  daquele mês com um único clique, sem escolher filtros à mão.
- **SC-006**: 100% das falhas de carregamento, de salvamento e de exclusão mostram uma mensagem ao usuário, que é a da
  API ou uma mensagem padrão, e nenhuma mostra detalhes técnicos, como o código de status ou a resposta bruta da API.
- **SC-007**: Abrir a página faz uma única consulta de transações e uma de categorias; cada mudança de filtro ou de
  ordenação faz uma única nova consulta de transações, sem buscar as categorias de novo; escolher de novo a ordenação
  em uso, ou o mesmo mês, não faz nenhuma consulta; e digitar uma busca faz no máximo uma consulta por pausa de 300 ms
  na digitação, e não uma por tecla, e nenhuma quando o texto aplicado não muda.
- **SC-008**: Uma descrição, um nome de categoria ou uma mensagem da API com marcação (por exemplo, `<b>teste</b>`)
  aparece literalmente, como texto, em 100% dos lugares da página: a célula "Descrição" e a etiqueta da categoria na
  lista; as opções e o valor escolhido do filtro "Categoria" e do campo "Categoria" do formulário; e as mensagens (snack
  bars).

## Premissas

Esta é uma spec retroativa: as premissas abaixo documentam o comportamento atual e verificado da página, e não padrões
em aberto escolhidos para uma feature nova; onde o esperado difere do atual, a premissa indica o desvio conhecido.

- A página foi construída antes da adoção do Spec Kit; só a abertura filtrada por mês (`?month=`) veio depois, com
  [[001-reports-dashboard]] (FR-013), e está repetida aqui por fazer parte desta página.
- A página consome, sem mudanças, as APIs de transações e de categorias descritas em
  `back/specs/002-transaction-management/contracts/transactions-api.yaml` e
  `back/specs/001-category-management/contracts/categories-api.yaml`; o backend não muda.
- As regras de negócio são do backend ([[002-transaction-management]]): a recusa de valor zero, de data ausente, de
  descrição longa e de categoria inexistente, o significado de cada filtro e os critérios de ordenação e de desempate. O
  formulário repete a exigência de data e de categoria, o limite da descrição e a recusa do zero, e acrescenta regras
  próprias de formato: o valor sem sinal, com até duas casas decimais, sem separador de milhar e com até 13 algarismos
  na parte inteira, e a data digitada como dia/mês/ano (FR-014, em que a recusa do zero, o limite do valor e o formato
  da data são desvios conhecidos); para o resto, mostra a mensagem da API. As mensagens da API estão sempre em
  português, e a recusa automática de um pedido malformado vem sem mensagem, então cai na mensagem padrão. No valor, a
  API só recusa o zero e não valida a faixa nem a escala, um desvio conhecido do back, corrigido pelo fluxo de bugs do
  back e que não bloqueia esta feature.
- A lista não é paginada: todas as transações que atendem aos filtros chegam e aparecem de uma vez. Não há meta de
  tempo: o uso é pessoal, com centenas de transações por mês, e as metas de desempenho desta página são de contagem de
  consultas (SC-007). O volume esperado é o de finanças pessoais, de centenas a poucos milhares de transações, a mesma
  faixa para a qual o backend foi dimensionado (plan de [[002-transaction-management]]). A falta de paginação e as
  observações de desempenho do back (a filtragem e a ordenação em memória e os campos calculados da lista de categorias,
  que esta página não usa) ficam aceitas até uma medição mostrar lentidão (primeiro meça, depois otimize).
- A página não soma valores; totais e comparações ficam no painel de relatórios ([[001-reports-dashboard]]).
- Os valores estão sempre em reais (BRL); não há campo de moeda.
- "Hoje", no formulário, é o dia no computador do usuário.
- A página de categorias, incluindo o link da quantidade de transações que abre esta página filtrada, é especificada em
  [[002-gestao-de-categorias]]; aqui fica só o lado que recebe o `categoryId` e, quando o link o traz junto, o `month`.
- A acessibilidade segue o que o Angular Material oferece: o teclado é o dos componentes (foco preso no diálogo e
  devolvido ao fechá-lo, teclado do menu e das listas de opções, Enter salva o formulário); as mensagens são anunciadas
  aos leitores de tela, mas as mudanças de estado da lista (carregando, vazia, erro, recarregada) não são; o "Desfazer"
  fica 5 segundos, sem como estender o prazo; e o vermelho das saídas usa a cor de erro do tema, sem contraste medido (o
  sinal negativo garante que a cor não é a única pista). Melhorias nesses pontos ficam para uma feature própria de
  acessibilidade, junto com a [[002-gestao-de-categorias]].
- O alvo são navegadores desktop; a página não tem layout próprio para telas pequenas.
- A aplicação tem um único usuário, como no resto do Denarius; quem pode ver ou mudar as transações está fora do
  escopo.
- Onde o comportamento esperado difere do atual, o requisito traz o esperado e marca o atual como desvio conhecido,
  conforme os Esclarecimentos de 2026-10-04, 2026-10-05 e 2026-10-06. Esta spec não tem trabalho para esses desvios,
  todos corrigidos fora dela, pelo fluxo de bugs do front: a data digitada, a recusa do zero e o limite do valor
  (FR-014), o diálogo aberto até a resposta da API (FR-016), o aviso da transação salva fora dos filtros (FR-017), "Nova
  transação" sem categorias (FR-019), o dia escolhido no filtro "Mês" (FR-024, ainda a reproduzir, no campo
  compartilhado com as páginas de categorias e de relatórios), os calendários em português (FR-030, corrigidos em
  `front/bugs/calendarios-em-ingles/`) e a consulta que não se repete ao escolher de novo a ordenação em uso ou o mesmo
  mês, nem com o mesmo texto na busca por descrição (FR-031, corrigida em `front/bugs/consulta-repetida-sem-mudanca/`
  com uma comparação por conteúdo no estado de cada página, e não no menu de ordenação nem no campo "Mês"
  compartilhados; vale também para as páginas de categorias e de relatórios). Também ficam fora o clique repetido em
  "Excluir" sem retorno visual (FR-020), pelo fluxo de bugs do front; a validação da faixa e da escala do valor pela
  API, pelo fluxo de bugs do back; e, como trabalho futuro em features próprias, o indicador de filtro em uso (FR-023) e
  as melhorias de acessibilidade. Bloqueiam a entrega desta feature, pela Governança do `AGENTS.md`, os desvios que
  contrariam a constituição: a consulta repetida (FR-031) e o clique repetido em "Excluir" (FR-020), pelo princípio III;
  os calendários em inglês (FR-030), pela regra de idioma; e a data digitada e os valores com 14 ou mais algarismos
  (FR-014), pelo princípio II. Um bug-fix conta como concluído quando o seu `front/bugs/<slug>/test.md` registra o
  resultado `verified`, o teste que reproduz o bug e a suíte completa passam e o `/speckit-converge` reavalia contra o
  código os requisitos e critérios afetados. Se a reprodução de um desvio deduzido do código não o confirmar, o
  requisito continua valendo e a nota de desvio sai da spec. Os demais casos-limite descrevem o comportamento atual.
