# Especificação da feature: Gestão de categorias

**Branch da feature**: `002-gestao-de-categorias`

**Criada em**: 2026-10-04

**Status**: Aprovada

**Entrada**: Descrição do usuário: "Especificação retroativa da página de categorias do front (`/categories`), já
implementada antes da adoção do Spec Kit, derivada do código atual em `front/src/app/categories` e dos seus testes.
Cobre o que o usuário vê e faz: o item 'Categorias' no menu e a rota carregada sob demanda; a lista de categorias com
nome na cor da categoria, quantidade de transações e saldo em BRL; os filtros por nome, por ter ou não transações e por
mês; a ordenação pelo menu; a criação e a edição num diálogo (nome com limite de caracteres e cor escolhida na paleta
padrão ou personalizada); a exclusão, bloqueada para categorias com transações, com a opção de desfazer; os estados de
carregamento, lista vazia e erro; as mensagens de sucesso e de erro, incluindo as vindas da API; e o link da quantidade
de transações de uma categoria para a página de transações filtrada por ela. A página consome a API de categorias
documentada em `back/specs/001-category-management/contracts/` e não muda o back."

## Esclarecimentos

### Sessão 2026-10-04

- P: A busca por nome, que hoje diferencia maiúsculas, minúsculas e acentos por uma regra da API, é intencional, um
  defeito ou uma melhoria futura? → R: Defeito. O esperado é ignorar maiúsculas e minúsculas, como a busca por
  descrição da página de transações já faz ([[003-gestao-de-transacoes]]), mas continuar diferenciando acentos: "mer"
  encontra "Mercado", e "educacao" continua sem encontrar "Educação". O atual é um desvio conhecido da API, a corrigir
  no back, num ciclo próprio (`/speckit-bug-assess` em `back/` ou, se o diagnóstico concluir que é comportamento novo,
  o fluxo de features), com o contrato atualizado e fora desta spec; a busca de transações e o front não mudam.
- P: O link da quantidade de transações, que leva só a categoria e não o mês escolhido, é intencional, um defeito ou
  uma melhoria futura? → R: Defeito. Com um mês escolhido, o link leva a categoria e o mês
  (`/transactions?categoryId=<id>&month=YYYY-MM`) e abre as transações da categoria naquele mês; sem mês, continua
  levando só a categoria. O atual é um desvio conhecido, a corrigir pelo fluxo de bugs do front, fora desta spec.
- P: O diálogo que fecha ao salvar, antes da resposta da API, e perde o que foi digitado quando ela recusa, é
  intencional, um defeito ou uma melhoria futura? → R: Defeito, e a decisão vale também para a página de transações. O
  diálogo continua aberto enquanto a API responde, sem aceitar um segundo "Salvar", e só fecha quando ela aceita; na
  recusa, continua aberto com o que foi digitado, e a mensagem da API aparece. O atual é um desvio conhecido, a
  corrigir pelo fluxo de bugs do front, fora desta spec.
- P: Ocultar os filtros sem limpá-los, e sem nada que indique que a lista continua filtrada, é intencional, um defeito
  ou uma melhoria futura? → R: Melhoria futura, e a decisão vale também para a página de transações. Ocultar continua
  sem limpar os filtros, e isso é requisito; um indicador de filtro em uso fica como trabalho futuro, numa feature
  própria.
- P: Os nomes acessíveis do calendário do campo "Mês", hoje em inglês, e das cores da paleta, hoje o código
  hexadecimal, são intencionais, defeitos ou melhorias futuras? → R: O calendário em inglês é um defeito: o esperado
  são rótulos em português em todos os calendários da aplicação. O desvio foi corrigido pelo fluxo de bugs do front
  (`front/bugs/calendarios-em-ingles/`), fora desta spec. As cores ficam como melhoria futura: o código hexadecimal
  continua como requisito, e um nome para cada cor fica como trabalho futuro, numa feature própria.
- P: Escolher um dia na visão de dias do campo "Mês", aberta pelo botão do ano, muda o texto do campo sem mudar o
  filtro: isso é intencional, um defeito ou uma melhoria futura? → R: Defeito, decidido no esclarecimento da
  [[003-gestao-de-transacoes]] e válido para o campo compartilhado, usado também nesta página e nos relatórios. O
  esperado é aplicar ao filtro o mês do dia escolhido, com o campo e a lista sempre de acordo e "Limpar mês" à vista.
  O atual é um desvio conhecido, deduzido do código e ainda não reproduzido, a corrigir no campo compartilhado pelo
  fluxo de bugs do front, fora desta spec.
- P: Escolher de novo a ordenação em uso, ou o mesmo mês, que hoje refaz a consulta e troca as linhas pelo indicador de
  carregamento, é intencional, um defeito ou uma melhoria futura? → R: Defeito, decidido depois da revisão
  pós-esclarecimento e válido também para a [[003-gestao-de-transacoes]]. O esperado é não refazer a consulta: a lista
  fica como está. O desvio, que contrariava o princípio III, foi corrigido de uma vez pelo fluxo de bugs do front
  (`front/bugs/consulta-repetida-sem-mudanca/`), fora desta spec: a comparação por conteúdo fica no estado de cada
  página (filtros e ordenação; nos relatórios, o mês), e não nos componentes compartilhados de ordenação e de mês, e
  vale para categorias, transações e relatórios.
- P: Os demais pontos suspeitos (o nome só com espaços, que passa pelo diálogo e só a API recusa; o "Recarregar" sem
  indicador de carregamento; a paleta, com a falha silenciosa, o preto como cor inicial e a "Cor personalizada"
  sobrescrita; e a exclusão sem confirmação, com o "Desfazer" substituído por outra mensagem) devem mudar? → R: O
  usuário viu e não pediu mudança; esses pontos ficam documentados como comportamento atual.

### Sessão 2026-10-05

- P: A busca por nome, que pelo código consulta a lista de novo com o mesmo texto (ao redigitar o texto já aplicado,
  ao digitar e apagar uma letra em menos de 300 ms e, depois, se ele sai do campo sem digitar mais nada), é
  intencional, um defeito ou uma melhoria futura? → R: Defeito, o mesmo da consulta repetida. O esperado é o "Nome"
  não refazer a consulta quando o texto aplicado não mudou. O desvio, que contrariava o princípio III, foi reproduzido
  num teste e corrigido junto com a consulta repetida pelo fluxo de bugs do front
  (`front/bugs/consulta-repetida-sem-mudanca/`), fora desta spec, pela mesma comparação por conteúdo nos filtros da
  página; a decisão vale também para a "Descrição" da [[003-gestao-de-transacoes]].
- P: O que o diálogo faz enquanto espera a resposta da API ("Cancelar", Esc, clique fora ou uma resposta que chega com
  o diálogo já fechado)? → R: Fica em aberto nesta spec: o `/speckit-bug-assess` da correção do diálogo propõe o
  comportamento, e o usuário decide no portão desse bug.

### Sessão 2026-10-06

Entradas registradas depois da revisão dos checklists de segurança, performance e UX. São redação e registro das decisões do
usuário, sem requisito além do que o usuário decidiu; cada uma diz o que mudou nesta spec.

- P: O "Excluir", que não se protege contra clique repetido nem dá retorno visual durante a exclusão e a restauração, é
  intencional, um defeito ou uma melhoria futura? → R: Defeito, válido também para a [[003-gestao-de-transacoes]]. Um
  segundo clique no "Excluir" da mesma linha, antes de a lista recarregar, envia outro pedido de exclusão, inútil, que a
  API recusa ("Categoria não encontrada."), e essa mensagem de erro toma o lugar da que oferece "Desfazer". O atual é
  um desvio conhecido, contrário ao princípio III (pedidos repetidos sem necessidade), a corrigir pelo fluxo de bugs do
  front, fora desta spec; a forma da correção (por exemplo, desabilitar o "Excluir" da linha durante a exclusão) fica
  para o `/speckit-bug-assess`. Registrado na FR-012, na FR-013 e nos Casos-limite. Esta correção, a da consulta
  repetida (FR-023) e a do calendário em português (FR-020) bloqueiam a entrega desta feature e da
  [[003-gestao-de-transacoes]].
- P: A acessibilidade da página (teclado, foco, anúncios, tempos e contraste dos controles) deve mudar? → R: Não nesta
  spec. O comportamento atual, apoiado no Material, fica documentado nas Premissas e nos Casos-limite; os pontos fracos
  (o link da quantidade anunciado só pelo número, o foco perdido depois de excluir, o "Desfazer" com 5 segundos fixos,
  as cores #F6BF26 e #C0CA33 com cerca de 1,7:1 contra o fundo claro, o título da aba fixo e os estados da lista não
  anunciados) ficam como trabalho futuro de uma feature própria de acessibilidade, para esta página e a
  [[003-gestao-de-transacoes]], sem bloquear a entrega.
- P: Há meta de tempo para carregar a lista, salvar ou excluir, e um volume a partir do qual a lista sem paginação deve
  ser revista? → R: Não há meta de tempo: o uso é pessoal, com dezenas de categorias, e as metas de contagem de consultas
  (SC-006) e de carregamento sob demanda (SC-009) ocupam esse lugar. A falta de paginação é revista se uma medição
  mostrar lentidão ("primeiro meça", princípio III). Registrado nas Premissas.
- P: Comportamentos atuais ainda não documentados (categorias com o mesmo nome e a mesma cor, mensagens padrão que não
  dizem o que fazer, a quantidade sem separador de milhar, a quantidade "0" como link e o nome longo no rótulo) devem
  mudar? → R: Não; ficam documentados como comportamento atual, nos Casos-limite. A FR-005 define o formato só do
  saldo e não contradiz a quantidade sem separador de milhar ("1234"), que fica assim por decisão do usuário.
- P: Como os nomes e as mensagens "exibidos como texto" (FR-021) são conferidos? → R: Por um exemplo nos Casos-limite
  (um nome com `<b>teste</b>` aparece literalmente), conferido por um teste de caracterização da tabela; o texto das
  mensagens é exibido como texto pelo próprio Material, sem teste próprio.
- Ajustes de redação, sem mudança de comportamento: a FR-001 passa a dizer "título visível 'Categorias' no cabeçalho
  da página" (a aba do navegador mostra "Denarius" em todas as páginas); a SC-004 cita a condição de a mensagem de
  exclusão estar à vista, já registrada nos Casos-limite; a SC-007 deixa explícito que a falha silenciosa da paleta,
  mantida pelo usuário em 2026-10-04, fica de fora; e o ponto em aberto da FR-010 inclui o que indica a espera, que já
  fazia parte da pergunta de 2026-10-05.

## Cenários de usuário e testes _(obrigatório)_

### História de usuário 1 - Ver as categorias e o uso de cada uma (Prioridade: P1)

Como alguém que organiza as próprias finanças por categorias, quero abrir a página de categorias pelo menu e ver todas
as minhas categorias, cada uma na sua cor, com a quantidade de transações e o saldo, para saber num relance como uso
cada categoria.

**Por que esta prioridade**: É a base da página: criar, editar, excluir, filtrar e ir às transações partem dessa lista.

**Teste independente**: Com categorias cadastradas (com e sem transações, com saldo negativo, zero e positivo), abrir
"Categorias" no menu e conferir os nomes, as cores, as quantidades e os saldos; depois, fazer a API falhar e conferir a
mensagem de erro e o botão "Recarregar".

**Cenários de aceitação**:

1. **Dado** o menu de navegação, **Quando** o usuário escolhe "Categorias", **Então** a página "Categorias" abre em
   `/categories`, e o item "Categorias" aparece marcado como a página atual.
2. **Dado que** existem categorias, **Quando** a lista é exibida, **Então** cada linha mostra o nome da categoria num
   rótulo nas cores dela, a quantidade de transações e o saldo em BRL (por exemplo, "R$ 1.842,55"), em ordem de nome de
   A a Z.
3. **Dado que** uma categoria tem saldo negativo, **Quando** a lista é exibida, **Então** o saldo dela aparece em
   vermelho; saldos zero ou positivos aparecem na cor normal do texto.
4. **Dado que** a lista ainda está carregando, **Quando** a página é exibida, **Então** a tabela mostra um indicador de
   carregamento no lugar das linhas.
5. **Dado que** não há nenhuma categoria a mostrar, **Quando** a lista termina de carregar, **Então** a tabela mostra
   "Nenhuma categoria encontrada.".
6. **Dado que** a lista não pôde ser carregada, **Quando** a página é exibida, **Então** a tabela mostra "Não foi
   possível carregar as categorias.", e o botão "Recarregar" do cabeçalho busca a lista de novo.

---

### História de usuário 2 - Criar e editar categorias (Prioridade: P2)

Como alguém que organiza as transações por categorias, quero criar uma categoria com nome e cor e, depois, mudar o nome
ou a cor, num diálogo na própria página, para manter a organização do meu jeito.

**Por que esta prioridade**: Sem categorias não há como registrar transações; a edição mantém a organização correta ao
longo do tempo.

**Teste independente**: Criar uma categoria por "Nova categoria" e conferir a mensagem e a nova linha; editar o nome e a
cor de outra e conferir a lista atualizada; tentar salvar sem nome e conferir o aviso; fazer a API recusar um
salvamento e conferir a mensagem e o diálogo ainda aberto, com o que foi digitado (desvio conhecido; ver FR-010).

**Cenários de aceitação**:

1. **Dada** a página de categorias, **Quando** o usuário escolhe "Nova categoria", **Então** abre o diálogo "Nova
   categoria", com o "Nome" vazio, o contador "0/100" e a primeira cor da paleta padrão já escolhida.
2. **Dado** o diálogo aberto, **Quando** o usuário informa um nome, escolhe uma cor da paleta ou uma "Cor personalizada"
   e escolhe "Salvar", **Então** o diálogo continua aberto, sem aceitar um segundo "Salvar", até a API responder;
   quando ela aceita, o diálogo fecha, a categoria é criada com esse nome e essa cor, a mensagem "Categoria criada."
   aparece e a lista é atualizada (desvio conhecido: hoje o diálogo fecha antes da resposta; ver FR-010).
3. **Dado** o diálogo aberto com o nome vazio, **Quando** o usuário escolhe "Salvar", **Então** o diálogo continua
   aberto, nada é enviado e o campo mostra "Informe um nome".
4. **Dada** uma categoria na lista, **Quando** o usuário escolhe "Editar" na linha dela, **Então** abre o diálogo
   "Editar categoria" com o nome e a cor atuais, e a cor aparece marcada na paleta quando faz parte dela.
5. **Dado** o diálogo de edição, **Quando** o usuário muda o nome ou a cor e escolhe "Salvar", **Então** o diálogo
   continua aberto, sem aceitar um segundo "Salvar", até a API responder; quando ela aceita, o diálogo fecha, a
   categoria é atualizada, a mensagem "Categoria salva." aparece e a lista é atualizada (desvio conhecido: hoje o
   diálogo fecha antes da resposta; ver FR-010).
6. **Dado** o diálogo aberto, sem um "Salvar" em andamento, **Quando** o usuário escolhe "Cancelar" ou fecha o
   diálogo, **Então** nada é enviado e nenhuma mensagem aparece. O que "Cancelar", Esc ou um clique fora fazem enquanto
   a API responde a um "Salvar" está em aberto (ver FR-010).
7. **Dado que** a API recusa a criação ou a edição, **Quando** a resposta chega, **Então** o diálogo continua aberto,
   com o que foi digitado, aparece a mensagem de erro da API (ou "Não foi possível salvar a categoria.", quando ela não
   traz uma) com a ação "Fechar", e a lista não é recarregada (desvio conhecido: hoje o diálogo já fechou, e o que foi
   digitado se perde; ver FR-010).

---

### História de usuário 3 - Excluir categorias sem uso e desfazer (Prioridade: P3)

Como alguém que mantém a lista de categorias enxuta, quero excluir as categorias que não uso, com a chance de desfazer
um engano, sem o risco de excluir uma categoria que já tem transações.

**Por que esta prioridade**: Limpar a lista é menos frequente que criar e editar, e o bloqueio protege o histórico de
transações.

**Teste independente**: Excluir uma categoria sem transações e conferir a mensagem com "Desfazer" e a lista sem ela;
desfazer e conferir que ela volta; conferir que o "Excluir" de uma categoria com transações está desabilitado e explica
o motivo.

**Cenários de aceitação**:

1. **Dada** uma categoria sem transações em nenhum mês, **Quando** o usuário escolhe "Excluir" na linha dela, **Então**
   ela é excluída na hora, sem pedido de confirmação, a lista é atualizada sem ela e a mensagem "Categoria excluída."
   aparece por 5 segundos, com a ação "Desfazer".
2. **Dada** a mensagem "Categoria excluída." visível, **Quando** o usuário escolhe "Desfazer", **Então** uma categoria
   com o mesmo nome e a mesma cor é criada de novo, a mensagem "Categoria restaurada." aparece e a lista é atualizada.
3. **Dada** uma categoria com transações em qualquer mês, **Quando** a lista é exibida, **Então** o "Excluir" dela
   aparece desabilitado, com a dica "Não é possível excluir uma categoria com transações, mesmo que em outros meses.", e
   escolhê-lo não faz nada.
4. **Dado que** a API recusa a exclusão (por exemplo, porque a categoria ganhou transações depois de a lista carregar),
   **Quando** a resposta chega, **Então** aparece a mensagem de erro da API (ou "Não foi possível excluir a categoria.",
   quando ela não traz uma) com a ação "Fechar", e a lista não é recarregada.

---

### História de usuário 4 - Ver o uso de um mês (Prioridade: P4)

Como alguém que revisa as finanças mês a mês, quero escolher um mês e ver a quantidade de transações e o saldo de cada
categoria só naquele mês, para comparar como usei cada categoria em cada período.

**Por que esta prioridade**: Refina a visão geral da História 1 com um recorte de tempo, que ganha valor quando já há
histórico.

**Teste independente**: Com transações em meses diferentes, exibir os filtros pelo botão "Exibir filtros" (o da
História 5, porque o campo "Mês" fica no painel de filtros), escolher um mês e conferir as quantidades e os saldos
daquele mês; limpar o mês e conferir os totais de todo o período.

**Cenários de aceitação**:

1. **Dado que** nenhum mês foi escolhido, **Quando** a lista é exibida, com os filtros ocultos ou visíveis, **Então** a
   quantidade e o saldo de cada categoria somam as transações de todo o período; com os filtros visíveis, o campo "Mês"
   mostra "Todos os meses".
2. **Dados** os filtros visíveis, **Quando** o usuário escolhe um mês diferente do atual no campo "Mês", que abre na
   visão dos meses do ano, **Então** o campo mostra o mês como "MM/aaaa" (por exemplo, "09/2026") e a lista é
   recarregada com a quantidade e o saldo de cada categoria restritos àquele mês; as categorias sem transações nele
   continuam na lista, zeradas. O mesmo vale quando o usuário vai, pelo botão do ano, à visão de dias e escolhe um dia
   de outro mês (ou sem mês escolhido): vale o mês daquele dia, e "Limpar mês" aparece (desvio conhecido, ainda não
   reproduzido: hoje o campo passa a mostrar o mês do dia escolhido, mas a lista não muda; ver FR-016).
3. **Dado** um mês escolhido, **Quando** o usuário escolhe "Limpar mês", **Então** o campo volta a "Todos os meses", e a
   lista volta aos totais de todo o período.
4. **Dada** uma categoria sem transações no mês escolhido, mas com transações em outros meses, **Quando** a lista é
   exibida, **Então** ela mostra quantidade zero, e o "Excluir" dela continua desabilitado.
5. **Dado** um mês escolhido, **Quando** o usuário escolhe de novo o mesmo mês, **Então** a lista fica como está, sem
   nova consulta.

---

### História de usuário 5 - Encontrar e ordenar categorias (Prioridade: P5)

Como alguém com muitas categorias, quero buscar pelo nome, mostrar só as categorias com (ou sem) transações e ordenar a
lista por nome, quantidade de transações ou saldo, para achar rápido a categoria que procuro e comparar as categorias
entre si.

**Por que esta prioridade**: Ajuda a navegar numa lista que já funciona (Histórias 1 a 4) e ganha valor quando a lista
cresce.

**Teste independente**: Com categorias de nomes, quantidades e saldos variados, buscar por parte de um nome, filtrar
com e sem transações e escolher cada opção de ordenação, conferindo a lista a cada passo; depois, com um mês escolhido
(História 4), conferir que o filtro e a ordenação consideram só aquele mês.

**Cenários de aceitação**:

1. **Dado que** a página acabou de abrir, **Quando** ela é exibida, **Então** os filtros estão ocultos, e o botão
   "Exibir filtros" do cabeçalho os mostra; com eles visíveis, o mesmo botão vira "Ocultar filtros".
2. **Dados** os filtros visíveis, **Quando** o usuário digita parte de um nome em "Nome", **Então** a lista é
   recarregada depois de uma pausa de 300 ms na digitação, ou na hora, ao sair do campo, só com as categorias cujo nome
   contém o texto, sem diferenciar maiúsculas de minúsculas, mas diferenciando acentos: "mer" encontra "Mercado", e
   "educacao" não encontra "Educação" (desvio conhecido: hoje a API também diferencia maiúsculas de minúsculas, e "mer"
   não encontra "Mercado"; ver FR-017).
3. **Dados** os filtros visíveis, **Quando** o usuário escolhe "Com transações" ou "Sem transações" em "Transações no
   período", **Então** só aparecem as categorias com pelo menos uma transação no período, ou sem nenhuma; "Todas" volta
   a mostrar todas.
4. **Dado** um filtro preenchido, **Quando** o usuário escolhe o botão de limpá-lo ("Limpar nome", "Limpar transações
   no período" ou "Limpar mês"), **Então** o filtro volta ao padrão e a lista é recarregada; cada botão de limpar só
   aparece enquanto o seu filtro está preenchido.
5. **Dada** a lista, **Quando** o usuário abre o menu de ordenação e escolhe outra ordenação entre "Nome (A–Z)",
   "Nome (Z–A)", "Mais transações primeiro", "Menos transações primeiro", "Maior saldo primeiro" e
   "Menor saldo primeiro", **Então** o menu fecha e a lista é recarregada nessa ordem.
6. **Dada** uma ordenação em uso, **Quando** o usuário aponta para o botão de ordenação ou abre o menu, **Então** o
   botão se identifica pela ordenação em uso (por exemplo, "Ordenar: Nome (A–Z)"), e o menu a mostra marcada.
7. **Dado** um mês escolhido (História 4), **Quando** o usuário filtra "Com transações" ou "Sem transações", ou ordena
   por quantidade de transações ou por saldo, **Então** o filtro e a ordenação consideram só as transações daquele mês.
8. **Dados** filtros preenchidos, **Quando** o usuário escolhe "Ocultar filtros", **Então** os filtros continuam
   valendo e a lista continua filtrada; ao exibi-los de novo, eles aparecem como estavam.
9. **Dada** uma ordenação em uso, **Quando** o usuário a escolhe de novo no menu, **Então** o menu fecha e a lista fica
   como está, sem nova consulta.

---

### História de usuário 6 - Ir às transações de uma categoria (Prioridade: P6)

Como alguém que quer entender o saldo de uma categoria, quero ir da quantidade de transações dela direto para essas
transações, para conferir os detalhes sem precisar procurá-las.

**Por que esta prioridade**: É um atalho; a página de transações já permite filtrar por categoria.

**Teste independente**: Escolher a quantidade de transações de uma categoria e conferir que a página de transações abre
filtrada por ela; repetir com um mês escolhido e conferir que ela abre filtrada também por esse mês (desvio conhecido;
ver FR-004).

**Cenários de aceitação**:

1. **Dada** uma categoria na lista, sem mês escolhido, **Quando** o usuário escolhe a quantidade de transações dela,
   **Então** a página de transações abre no endereço `/transactions?categoryId=<id da categoria>`, filtrada por essa
   categoria, como descrito em [[003-gestao-de-transacoes]].
2. **Dado** um mês escolhido (História 4), **Quando** o usuário escolhe a quantidade de transações de uma categoria,
   **Então** a página de transações abre no endereço `/transactions?categoryId=<id da categoria>&month=YYYY-MM`,
   filtrada por essa categoria e por esse mês, com as transações que a quantidade conta (desvio conhecido: hoje o link
   leva só a categoria; ver FR-004).

---

### Casos-limite

- Os nomes das categorias e as mensagens de erro da API vêm de fora da página; são exibidos como texto, nunca
  interpretados: um nome como `<b>teste</b>` aparece literalmente no rótulo, com os sinais `<` e `>`, sem negrito.
- O "Nome" para de aceitar texto em 100 caracteres, contando os espaços, e o contador mostra "100/100".
- Um nome só com espaços passa pelo diálogo e é enviado; a API o recusa, e a página mostra a mensagem dela.
- Quando a API recusa um salvamento, o diálogo continua aberto, com o que foi digitado (FR-010). Desvio conhecido: hoje
  ele já fechou quando a resposta chega, e, para tentar de novo, o usuário abre o diálogo e preenche tudo outra vez.
  O comportamento durante a espera ("Cancelar", Esc, clique fora ou uma resposta que chega com o diálogo já fechado)
  está em aberto (FR-010).
- Os espaços no início e no fim do nome são removidos pela API ([[001-category-management]], do backend), e a lista
  mostra o nome como ela o salvou.
- A paleta padrão é buscada na primeira vez que o diálogo abre e reaproveitada nas seguintes. Se ela não carregar, a
  falha é silenciosa, sem nenhum aviso: o diálogo oferece só a "Cor personalizada", uma categoria nova começa em preto,
  e a paleta só é buscada de novo quando a aplicação é recarregada.
- Na primeira abertura do diálogo de uma categoria nova, se a paleta chegar depois de o usuário escolher uma "Cor
  personalizada", a cor escolhida volta para a primeira da paleta.
- Uma categoria com uma cor fora da paleta abre na edição sem nenhuma cor da paleta marcada, com a cor dela na "Cor
  personalizada".
- O rótulo do nome usa um tom claro e um tom escuro da cor da categoria, trocados no tema escuro, para manter a leitura
  em qualquer cor, inclusive preto e branco; por isso, o tom exibido não é exatamente a cor escolhida.
- O "Excluir" desabilitado continua mostrando a sua dica quando o usuário aponta para ele ou chega a ele pelo
  teclado: ele continua alcançável pelo Tab, e escolhê-lo não faz nada.
- Um segundo clique no "Excluir" da mesma linha, antes de a lista recarregar, não envia outro pedido de exclusão, e a
  página mostra que a exclusão, ou a restauração pelo "Desfazer", está em andamento (FR-012, FR-013).
- No diálogo, cada cor da paleta se anuncia pelo seu código hexadecimal (por exemplo, "#F4511E"), e não por um nome de
  cor; um nome para cada cor é trabalho futuro (FR-020).
- O calendário do campo "Mês" deve se anunciar em português (FR-020).
- No calendário do campo "Mês", o botão do ano leva à visão de dias; escolher um dia ali aplica ao filtro o mês daquele
  dia, e o campo e a lista sempre concordam (FR-016). Desvio conhecido, deduzido do código e ainda não reproduzido:
  hoje o campo passa a mostrar o mês do dia escolhido e o calendário fecha, mas o filtro não muda; a lista continua com
  o mês anterior, ou com todos os meses, e, se não havia mês escolhido, "Limpar mês" nem aparece.
- Só uma mensagem aparece por vez: uma mensagem nova, como a de outra exclusão, substitui a anterior, e uma exclusão
  cuja mensagem foi substituída não pode mais ser desfeita.
- Quando a API não explica um erro (por exemplo, sem conexão), as mensagens usam os textos da própria página.
- Ao recarregar a lista ("Recarregar", ou depois de salvar, excluir ou desfazer), as linhas atuais continuam visíveis
  até a nova lista chegar, sem nenhum indicador de carregamento: o indicador só aparece quando não há linhas a mostrar.
  Já uma mudança de filtro ou de ordenação troca as linhas pelo indicador.
- Escolher de novo a ordenação em uso, ou o mesmo mês, não refaz a consulta, e a lista fica como está (FR-023).
- No "Nome", a lista não é consultada de novo quando o texto aplicado não mudou (FR-023).
- A busca por nome não diferencia maiúsculas de minúsculas, mas diferencia acentos: "MERCADO" e "mer" encontram
  "Mercado", e "educacao" não encontra "Educação" (FR-017). Desvio conhecido: hoje a API também diferencia maiúsculas
  de minúsculas ([[001-category-management]], do backend, cujo contrato não define esse ponto), e "MERCADO" e "mer"
  não encontram "Mercado".
- Uma busca só com espaços é enviada à API, que a ignora: na prática, equivale a não buscar, e a lista mostra as
  categorias que atendem aos outros filtros.
- Ocultar os filtros não os desfaz (FR-015), e, com eles ocultos, nada indica que a lista continua filtrada, que pode
  parecer incompleta; um indicador de filtro em uso é trabalho futuro.
- Com um mês escolhido, a quantidade de transações é a do mês, e o link dela leva também o mês, para a lista que abre
  bater com a quantidade (FR-004). Desvio conhecido: hoje o link leva só a categoria, e a lista que abre mostra as
  transações de todos os meses.
- Com muitas categorias, a lista rola dentro do seu cartão, e o cabeçalho da página e os filtros continuam visíveis.
- Uma busca ou uma combinação de filtros sem resultado mostra "Nenhuma categoria encontrada.", e não um erro.
- Duas categorias com o mesmo nome e a mesma cor (permitido, porque os nomes não precisam ser únicos) não se
  distinguem na lista, a não ser pela quantidade e pelo saldo.
- As mensagens padrão de erro ("Não foi possível salvar a categoria.", "Não foi possível excluir a categoria." e "Não
  foi possível carregar as categorias.") não dizem o que fazer em seguida; para a lista, o caminho é o "Recarregar" do
  cabeçalho, e, para salvar ou excluir, tentar de novo.
- A quantidade de transações aparece como número simples, sem separador de milhar (por exemplo, "1234", e não
  "1.234"); só o saldo tem a formatação pt-BR (FR-005).
- A quantidade "0" continua sendo link e abre a página de transações com a lista vazia.
- Um nome longo, de até 100 caracteres, aparece no rótulo numa linha só, sem quebra e sem dica (o rótulo não reage ao
  ponteiro). O rótulo corta o texto com reticências quando a largura dele é limitada, mas a coluna "Nome" não tem
  largura máxima: com um nome muito longo, a tabela se alarga e o cartão da lista passa a rolar na horizontal.
- Acessibilidade apoiada no Material (comportamento atual): o diálogo prende o foco enquanto está aberto e o devolve
  ao botão que o abriu ao fechar; as mensagens são anunciadas às tecnologias assistivas, sem mover o foco, de modo que
  chegar ao "Desfazer" pelo teclado dentro dos 5 segundos é difícil; depois de uma exclusão, a linha some e o foco se
  perde; o link da quantidade se anuncia só pelo número, com a linha como contexto; os estados da lista (carregando,
  vazia, erro) não são anunciados; as cores #F6BF26 e #C0CA33 da paleta têm cerca de 1,7:1 de contraste contra o fundo
  claro do diálogo, assim como a marca branca da cor escolhida sobre elas; e a aba do navegador mostra "Denarius" em
  todas as páginas. Esses pontos são trabalho futuro de uma feature própria de acessibilidade (Esclarecimentos,
  2026-10-06).

## Requisitos _(obrigatório)_

### Requisitos funcionais

- **FR-001**: A aplicação DEVE oferecer a página de categorias em `/categories`, com o título visível "Categorias" no
  cabeçalho da página e com o item
  "Categorias" no menu de navegação, depois de "Relatórios" e "Transações", marcado como página atual enquanto ela está
  aberta; a página DEVE ser carregada só quando aberta.
- **FR-002**: A página DEVE listar as categorias na ordem em que a API as devolve, numa tabela com as colunas "Nome",
  "Transações" e "Saldo" e as ações de cada linha.
- **FR-003**: O nome de cada categoria DEVE aparecer num rótulo com um tom claro e um tom escuro da cor da categoria,
  trocados no tema escuro, com contraste de pelo menos 7:1 entre o texto e o fundo, qualquer que seja a cor.
- **FR-004**: A coluna "Transações" DEVE mostrar a quantidade de transações da categoria no período (o mês escolhido
  ou, sem mês, todo o período) como um link para a página de transações filtrada pela categoria e pelo mesmo período,
  para que a lista que abre tenha as transações que a quantidade conta. O endereço público do link DEVE ser
  `/transactions?categoryId=<id da categoria>`, com `&month=YYYY-MM` quando há um mês escolhido
  ([[003-gestao-de-transacoes]]). Desvio conhecido: hoje o link leva só a categoria, mesmo com um mês escolhido, e a
  lista que abre mostra as transações de todos os meses; a correção segue o fluxo de bugs do front, fora desta spec.
- **FR-005**: A coluna "Saldo" DEVE mostrar o saldo da categoria no mesmo período, em BRL com formatação pt-BR, em
  vermelho quando negativo.
- **FR-006**: A tabela DEVE mostrar, no lugar das linhas, um indicador de carregamento enquanto a lista carrega sem
  linhas a mostrar, "Nenhuma categoria encontrada." quando a lista vem vazia e "Não foi possível carregar as
  categorias." quando ela falha; o botão "Recarregar" do cabeçalho DEVE buscar a lista de novo, com os filtros e a
  ordenação em uso.
- **FR-007**: O cabeçalho DEVE oferecer "Nova categoria", que abre o diálogo "Nova categoria", e cada linha DEVE
  oferecer "Editar", que abre o diálogo "Editar categoria" com o nome e a cor da categoria.
- **FR-008**: O diálogo DEVE ter o campo "Nome", obrigatório e limitado a 100 caracteres, com um contador "N/100";
  salvar com o nome vazio DEVE mostrar "Informe um nome", manter o diálogo aberto e não enviar nada.
- **FR-009**: O diálogo DEVE oferecer as cores da paleta padrão, com a escolhida marcada, e uma "Cor personalizada"
  livre; uma categoria nova DEVE começar com a primeira cor da paleta, ou com preto se a paleta não carregar, e a "Cor
  personalizada" DEVE continuar disponível sem a paleta.
- **FR-010**: "Salvar" DEVE enviar o nome e a cor à API, criando ou atualizando a categoria. Depois de "Salvar", o
  diálogo DEVE continuar aberto até a resposta da API, sem aceitar um segundo "Salvar", e fechar só quando ela aceitar;
  na recusa, DEVE continuar aberto com o que foi digitado e mostrar a mensagem de erro (FR-014). "Cancelar", ou
  fechar o diálogo sem um "Salvar" em andamento, NÃO DEVE enviar nada. Desvio conhecido: hoje "Salvar" fecha o
  diálogo antes da resposta da API, e, numa recusa, o que foi digitado se perde; a correção segue o fluxo de bugs do
  front, fora desta spec, e a mesma decisão vale para a página de transações ([[003-gestao-de-transacoes]]). Ponto em
  aberto, que o `/speckit-bug-assess` dessa correção propõe e o usuário decide no portão do bug (Esclarecimentos,
  sessão 2026-10-05): o que "Cancelar", Esc ou um clique fora fazem durante a espera, o que acontece com uma resposta
  que chega depois de o diálogo fechar e o que indica a espera.
- **FR-011**: Depois de uma criação ou edição aceita, a página DEVE mostrar "Categoria criada." ou "Categoria salva."
  por 3 segundos e recarregar a lista.
- **FR-012**: Cada linha DEVE oferecer "Excluir", que exclui a categoria na hora, sem confirmação; o botão DEVE ficar
  desabilitado para uma categoria com transações em qualquer mês, mesmo fora do mês escolhido, com a dica "Não é
  possível excluir uma categoria com transações, mesmo que em outros meses.". Do clique até a categoria sair da lista
  ou até a recusa da exclusão, um novo clique no "Excluir" da mesma linha NÃO DEVE enviar outro pedido, e a página DEVE
  mostrar que a exclusão está em andamento.
- **FR-013**: Depois de uma exclusão aceita, a página DEVE recarregar a lista e mostrar "Categoria excluída." por 5
  segundos, com a ação "Desfazer"; "Desfazer" DEVE criar de novo uma categoria com o mesmo nome e a mesma cor, mostrar
  "Categoria restaurada." por 3 segundos e recarregar a lista. Durante a restauração, a página DEVE mostrar que ela está
  em andamento.
- **FR-014**: Quando a API recusar ou falhar ao criar, editar, excluir ou desfazer, a página DEVE mostrar por 5
  segundos, com a ação "Fechar", a mensagem de erro da API ou, sem ela, "Não foi possível salvar a categoria." (criar,
  editar e desfazer) ou "Não foi possível excluir a categoria." (excluir), sem recarregar a lista.
- **FR-015**: O cabeçalho DEVE ter o botão "Exibir filtros"/"Ocultar filtros", que mostra ou oculta os filtros; os
  filtros DEVEM começar ocultos, e ocultá-los NÃO DEVE limpá-los: a lista continua filtrada e, ao exibi-los de novo,
  eles aparecem como estavam. Um indicador de filtro em uso, visível com os filtros ocultos, é trabalho futuro, numa
  feature própria, fora desta spec.
- **FR-016**: Os filtros DEVEM ser "Nome" (parte do nome), "Transações no período" ("Todas", "Com transações" ou "Sem
  transações") e "Mês" (mostrado como MM/aaaa, "Todos os meses" quando vazio, escolhido na visão dos meses do ano);
  cada filtro preenchido DEVE oferecer um botão para limpá-lo, e filtros vazios NÃO DEVEM ser aplicados. No "Mês",
  escolher um dia na visão de dias, aberta pelo botão do ano, DEVE aplicar o mês daquele dia, com o "Limpar mês" à
  vista, e o campo e a lista DEVEM sempre concordar; um dia do mês já aplicado não muda nada (FR-023). Desvio
  conhecido, deduzido do código e ainda não reproduzido: hoje escolher um dia muda o texto do campo sem mudar o filtro
  (ver Casos-limite); a correção segue o fluxo de bugs do front, no campo compartilhado, fora desta spec, e vale
  também para transações e relatórios.
- **FR-017**: Cada mudança de filtro DEVE recarregar a lista com todos os filtros combinados, filtrada pela API; a busca
  por nome DEVE esperar uma pausa de 300 ms na digitação, ou a saída do campo, antes de recarregar, e DEVE encontrar os
  nomes que contêm o texto sem diferenciar maiúsculas de minúsculas, mas diferenciando acentos, como a busca por
  descrição da página de transações já faz ([[003-gestao-de-transacoes]]). Desvio conhecido: hoje a API também
  diferencia maiúsculas de minúsculas; a correção é feita no back, num ciclo próprio (de bugs ou, se o diagnóstico
  concluir que é comportamento novo, de features), com o contrato atualizado, fora desta spec, e a página não muda,
  porque já envia o texto como foi digitado.
- **FR-018**: Com um mês escolhido, a quantidade, o saldo, o filtro "Transações no período" e a ordenação por quantidade
  ou por saldo DEVEM considerar só as transações daquele mês do calendário.
- **FR-019**: O cabeçalho DEVE ter um menu de ordenação com "Nome (A–Z)" (o padrão), "Nome (Z–A)", "Mais transações
  primeiro", "Menos transações primeiro", "Maior saldo primeiro" e "Menor saldo primeiro"; o botão DEVE se identificar
  pela ordenação em uso ("Ordenar: …"), o menu DEVE marcar a opção em uso, e escolher outra opção DEVE recarregar a
  lista ordenada pela API; escolher de novo a opção em uso NÃO DEVE recarregar (FR-023).
- **FR-020**: Os botões só com ícone da página DEVEM ter um nome acessível: "Recarregar", "Exibir filtros"/"Ocultar
  filtros", "Ordenar: …", "Editar" e "Excluir", que também aparecem como dica (no "Excluir" desabilitado, a dica é a
  explicação do bloqueio, e o nome continua "Excluir"), e "Limpar nome", "Limpar transações no período" e "Limpar mês",
  sem dica. No diálogo, as cores DEVEM ficar num grupo chamado "Cor", cada cor da paleta DEVE se identificar pelo seu
  código hexadecimal e informar se está escolhida, também para tecnologias assistivas, e o seletor livre DEVE se chamar
  "Cor personalizada"; um nome para cada cor da paleta é trabalho futuro, numa feature própria, fora desta spec. Os
  nomes acessíveis do calendário do campo "Mês" DEVEM estar em português (por exemplo, "Abrir calendário"), e
  vale para todos os calendários da aplicação.
- **FR-021**: Os nomes das categorias e as mensagens vindas da API DEVEM ser exibidos como texto, nunca interpretados.
- **FR-022**: A página DEVE usar só o que a API de categorias de [[001-category-management]] (do backend) já oferece,
  sem mudar o back; a API continua sendo a autoridade sobre a validação do nome e da cor, a recusa da exclusão de
  categorias em uso, a regra da busca por nome (FR-017, a corrigir no back) e o cálculo da quantidade e do saldo.
- **FR-023**: Escolher de novo a ordenação em uso, ou o mesmo mês, NÃO DEVE refazer a consulta, e a lista DEVE ficar
  como está; o "Nome" também NÃO DEVE refazer a consulta quando o texto aplicado não mudou.

### Entidades principais _(inclua se a feature envolver dados)_

- **Categoria** _(de [[001-category-management]], do backend)_: rótulo definido pelo usuário para agrupar transações,
  com nome e cor; na lista, vem com a quantidade de transações e o saldo do período e com a indicação de se pode ser
  excluída (só quando não tem transações em nenhum mês).
- **Filtros da lista**: parte do nome, transações no período (todas, com ou sem) e mês (ou todos os meses); valem
  enquanto a página está aberta, com o painel de filtros visível ou oculto.
- **Ordenação**: nome, quantidade de transações ou saldo, em ordem crescente ou decrescente; por padrão, nome de A a Z.
- **Paleta padrão**: lista fixa de cores oferecida no diálogo; a primeira é a cor inicial de uma categoria nova.

## Critérios de sucesso _(obrigatório)_

### Resultados mensuráveis

- **SC-001**: A partir de qualquer página, o usuário chega à lista de categorias com um clique no menu, e 100% das
  categorias que a API devolve aparecem com nome, quantidade de transações e saldo.
- **SC-002**: Criar uma categoria leva três ações a partir da lista ("Nova categoria", digitar o nome e "Salvar"),
  porque a cor já vem escolhida.
- **SC-003**: 100% das tentativas de excluir uma categoria com transações são impedidas: pelo botão desabilitado ou,
  com a lista desatualizada, pela recusa da API, explicada ao usuário.
- **SC-004**: Uma exclusão feita por engano pode ser desfeita com uma ação, em até 5 segundos, enquanto a mensagem
  dela está à vista (Casos-limite), e a categoria volta com o mesmo nome e a mesma cor.
- **SC-005**: 100% dos nomes de categoria, em qualquer cor, inclusive preto e branco, têm contraste de pelo menos 7:1
  entre o texto e o fundo, nos temas claro e escuro.
- **SC-006**: Cada mudança de filtro ou de ordenação gera uma única nova consulta da lista, digitar na busca por nome
  gera no máximo uma consulta por pausa de 300 ms na digitação, e escolher de novo a ordenação em uso ou o mesmo mês,
  ou terminar a digitação com o texto já aplicado, não gera nenhuma.
- **SC-007**: 100% das falhas ao carregar a lista, salvar, excluir ou desfazer terminam numa mensagem ao usuário;
  nenhuma passa em silêncio (a falha da paleta, silenciosa por decisão do usuário, fica de fora).
- **SC-008**: O usuário chega às transações de qualquer categoria da lista com um clique.
- **SC-009**: A página de categorias só é baixada quando o usuário a abre, e a paleta de cores, só quando ele abre o
  diálogo pela primeira vez.

## Premissas

Estas premissas documentam o comportamento atual e verificado da página (esta é uma spec retroativa, derivada do código
e dos testes em 2026-10-04), e não padrões em aberto escolhidos para uma feature nova:

- Onde o comportamento esperado difere do atual, o requisito traz o esperado e marca o atual como desvio conhecido,
  conforme os Esclarecimentos de 2026-10-04, 2026-10-05 e 2026-10-06. Esta spec não tem trabalho para esses desvios, a
  corrigir fora dela: no back, num ciclo próprio, a busca por nome (FR-017); no front, pelo fluxo de bugs, o link da
  quantidade com o mês (FR-004), o diálogo aberto até a resposta da API (FR-010), o mês do dia escolhido no calendário
  (FR-016, no campo compartilhado), o calendário em português (FR-020, corrigido em
  `front/bugs/calendarios-em-ingles/`), o "Excluir" protegido contra clique repetido, com retorno visual na exclusão e
  na restauração (FR-012, FR-013, corrigido em `front/bugs/clique-repetido-em-excluir/`), e nenhuma consulta nova ao
  escolher de novo a ordenação em uso ou o mesmo mês, nem quando o texto aplicado no "Nome" não mudou (FR-023). Também
  ficam fora, como trabalho futuro em features próprias, o indicador de filtro em uso (FR-015), um nome para cada cor da
  paleta (FR-020) e os pontos fracos de acessibilidade listados nos Casos-limite.
- A página consome a API de categorias de [[001-category-management]] (do backend), com o contrato em
  `back/specs/001-category-management/contracts/categories-api.yaml`, e não muda o back.
- A validação do diálogo (nome obrigatório e com até 100 caracteres) serve à experiência do usuário; a API valida tudo
  de novo: remove os espaços nas pontas do nome, recusa nomes só com espaços e cores inválidas e devolve a cor
  normalizada.
- Os nomes de categoria não precisam ser únicos, como na API.
- A busca por nome, os filtros, a ordenação e o cálculo da quantidade e do saldo são feitos pela API
  ([[001-category-management]]), e a página mostra o resultado como vem. A página envia o texto da busca como foi
  digitado; a regra de maiúsculas, minúsculas e acentos é da API. O contrato dela hoje só diz que a diferenciação
  entre maiúsculas e minúsculas "depende do banco" e não fala de acentos; o ciclo do back que corrige o desvio passa a
  definir as duas coisas, como na FR-017.
- Um mês é sempre um mês do calendário inteiro, como na API.
- A paleta padrão tem 11 cores, nesta ordem: #F4511E, #F6BF26, #43A047, #8E24AA, #1E88E5, #D81B60, #00897B, #FB8C00,
  #C0CA33, #78909C e #E53935. Ela vem com a aplicação, e não da API, que aceita qualquer cor hexadecimal; por isso,
  mudá-la não muda o back.
- A API não tem como desfazer uma exclusão: "Desfazer" cria uma categoria nova com o mesmo nome e a mesma cor, mas com
  outra identidade e outra data de criação. Como só se exclui categoria sem transações, nenhuma transação é afetada.
- Os filtros e a ordenação valem só enquanto a página está aberta: não vão para o endereço da página e voltam ao padrão
  (filtros ocultos e vazios, nome de A a Z) a cada visita.
- A lista não é paginada: vêm juntas todas as categorias que atendem aos filtros.
- Não há meta de tempo para carregar a lista, salvar ou excluir: o uso é pessoal, com dezenas de categorias, e as
  metas de contagem de consultas (SC-006) e de carregamento sob demanda (SC-009) ocupam esse lugar. A falta de
  paginação é revista se uma medição mostrar lentidão (primeiro meça, princípio III do `AGENTS.md`).
- A acessibilidade da página é a que o Angular Material oferece (foco no diálogo, anúncio das mensagens, nomes
  acessíveis da FR-020); os pontos fracos conhecidos estão nos Casos-limite e são trabalho futuro.
- O alvo são navegadores desktop; a página segue os temas claro e escuro do sistema e não tem layout próprio para telas
  pequenas.
- O endereço do link da quantidade (FR-004) é público como as rotas da aplicação (o usuário pode salvá-lo e
  compartilhá-lo); a página de transações, que já aceita no endereço a categoria e o mês, inclusive o modo como ela os
  recebe e mostra, é da [[003-gestao-de-transacoes]].
- Quem pode gerenciar as categorias (usuários, contas, permissões) está fora do escopo, como no resto da aplicação.
