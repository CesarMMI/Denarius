# Especificação da feature: Painel de relatórios

**Branch da feature**: `001-reports-dashboard`

**Criada em**: 2026-10-03

**Status**: Rascunho

**Entrada**: Descrição do usuário: "Uma página de relatórios (`/reports`, carregada sob demanda e com link no menu
de navegação) organizada como uma grade bento de altura total: uma linha de cinco cards de resumo (saldo, receitas,
despesas, taxa de poupança, projeção) e, abaixo, um gráfico de rosca das despesas por categoria, barras de receitas vs.
despesas ao longo dos meses, uma linha das despesas acumuladas do mês contra as do mês anterior e um bloco alto com as
transações do mês, com rolagem própria. Um seletor de mês no topo atualiza todos os blocos. Cada bloco carrega
sozinho, com estados independentes de carregamento, vazio ('Sem movimentações neste mês') e erro com nova tentativa.
Formatação pt-BR (BRL, datas dd/MM, meses por extenso), despesas em vermelho em todo lugar, variações em relação ao mês
anterior com ↑/↓ e uma cor semântica (para despesas, subir é ruim), tema claro/escuro, tooltips em BRL. Sem rolagem da
página no desktop; em telas menores, a grade vira uma coluna e a página pode rolar. Gráficos com ng2-charts,
adicionado com `ng add`."

## Esclarecimentos

### Sessão 2026-10-03

- P: O que muda antes de a feature ser concluída? → R: A rosca mostra no máximo cinco fatias, sendo a quinta "Outras"
  quando necessário; a lista mostra as dez transações mais recentes, com um botão "Ver todas" que abre a página de
  transações filtrada pelo mês. Uma linha de saldo sobre as barras foi testada e descartada: poluía o gráfico.
- P: Que cor "Outras" usa? → R: A neutra (`outline`), como antes.
- P: As despesas ficam vermelhas nos cards de resumo? → R: Não. Os cards as mostram como valores negativos, sem cor,
  porque o vermelho chamava atenção demais; os gráficos e a lista mantêm o vermelho.

### Sessão 2026-10-06

- P: Que cores usam as linhas da comparação acumulada? → R: O par da cor primária do tema: o mês atual em `primary` e
  o mês anterior em `secondary`, como foi entregue. Isso ressalva, só para a comparação acumulada, a resposta de
  2026-10-03 de que os gráficos mantêm o vermelho; os demais gráficos e a lista mantêm o vermelho das despesas. A
  legenda e o tooltip distinguem as duas linhas. (Decisão do dono no bug `chart-theme-teste-tokens-desatualizados`.)

## Cenários de usuário e testes _(obrigatório)_

### História de usuário 1 - Ver como está o mês atual (Prioridade: P1)

Como alguém que acompanha as próprias finanças, quero uma página de relatórios no menu que abra no mês atual e mostre
o saldo, as receitas, as despesas, a taxa de poupança e a projeção de fim de mês, cada um comparado com o mês
anterior, para saber num relance se o mês está no rumo certo.

**Por que esta prioridade**: É a porta de entrada e o destaque do painel; todos os outros blocos detalham o que esses
cinco números resumem.

**Teste independente**: Abrir a página de relatórios pelo menu e conferir os cinco cards com o resumo do mês,
incluindo a comparação com o mês anterior e as cores de cada variação.

**Cenários de aceitação**:

1. **Dado** o menu de navegação, **Quando** o usuário escolhe "Relatórios", **Então** a página de relatórios abre no
   mês atual.
2. **Dado que** um mês tem receitas e despesas, **Quando** a página mostra o seu resumo, **Então** os cards mostram o
   saldo, as receitas, as despesas (como valor negativo), a taxa de poupança e a despesa projetada, em BRL.
3. **Dado que** as despesas subiram em relação ao mês anterior, **Quando** o card de despesas é exibido, **Então** ele
   mostra uma seta para cima, o percentual e o nome do mês anterior, na cor "ruim"; já receitas ou saldo subindo
   aparecem na cor "boa".
4. **Dado que** um mês não tem receitas, **Quando** o card da taxa de poupança é exibido, **Então** ele indica que não
   há taxa, em vez de mostrar um número.
5. **Dado que** o mês anterior não tem valor para comparar, **Quando** um card é exibido, **Então** ele diz que não há
   comparação, em vez de mostrar um percentual.

---

### História de usuário 2 - Ver outro mês (Prioridade: P2)

Como alguém que revisa as próprias finanças, quero escolher um mês no topo da página e que todos os blocos o
acompanhem, para revisar qualquer mês do mesmo jeito.

**Por que esta prioridade**: Sem ela, a página só mostra o mês atual.

**Teste independente**: Escolher outro mês e confirmar que todos os blocos recarregam para ele.

**Cenários de aceitação**:

1. **Dado que** a página está no mês atual, **Quando** o usuário escolhe outro mês, **Então** o resumo, os três
   gráficos e a lista de transações mostram esse mês.
2. **Dado que** um bloco falhou ao carregar, **Quando** o usuário tenta de novo, **Então** só esse bloco carrega outra
   vez, e os outros ficam como estão.
3. **Dado que** a página está num mês, **Quando** o usuário escolhe "Mês anterior" ou "Próximo mês" no cabeçalho,
   **Então** todos os blocos mostram esse mês, carregado uma única vez.

---

### História de usuário 3 - Ver para onde foi o dinheiro (Prioridade: P3)

Como alguém tentando gastar menos, quero um gráfico de rosca das despesas do mês por categoria, na cor de cada
categoria, para ver quais categorias pesam mais.

**Por que esta prioridade**: É o primeiro detalhamento que qualquer pessoa pede depois do resumo.

**Teste independente**: Abrir um mês com despesas em várias categorias e conferir as fatias, as cores delas, a fatia
"Outras" e os tooltips.

**Cenários de aceitação**:

1. **Dado que** um mês tem despesas em várias categorias, **Quando** a rosca é exibida, **Então** cada categoria é uma
   fatia na sua própria cor, da maior para a menor, com legenda.
2. **Dado que** um mês tem mais de cinco categorias com despesas, **Quando** a rosca é exibida, **Então** as quatro
   maiores mantêm as suas fatias e o resto aparece junto numa quinta, "Outras", numa cor neutra.
3. **Dado que** o ponteiro está sobre uma fatia, **Quando** o tooltip abre, **Então** ele mostra a categoria, o valor
   em BRL e a sua participação nas despesas do mês.

---

### História de usuário 4 - Comparar receitas e despesas ao longo dos meses (Prioridade: P4)

Como alguém que planeja o futuro, quero barras de receitas e despesas dos últimos doze meses, terminando no mês
selecionado, para identificar os meses que deram errado.

**Por que esta prioridade**: Uma visão de tendência compensa quando já há algum histórico.

**Teste independente**: Abrir um mês e conferir doze meses consecutivos de barras, incluindo meses sem movimentação,
com tooltips em BRL.

**Cenários de aceitação**:

1. **Dado** o mês selecionado, **Quando** as barras são exibidas, **Então** elas cobrem os doze meses que terminam
   nele, do mais antigo para o mais recente, com as receitas em verde e as despesas em vermelho.
2. **Dado que** um mês do período não tem movimentação, **Quando** as barras são exibidas, **Então** esse mês continua
   com o seu lugar no eixo, zerado.
3. **Dado que** o ponteiro está sobre um mês, **Quando** o tooltip abre, **Então** ele mostra o nome do mês por
   extenso e as suas receitas e despesas em BRL.

---

### História de usuário 5 - Acompanhar o ritmo de gastos (Prioridade: P5)

Como alguém que acompanha os gastos durante o mês, quero uma linha das despesas do mês acumuladas dia a dia ao lado da
do mês anterior, para ver cedo se estou gastando mais rápido que o normal.

**Por que esta prioridade**: É um refinamento do card de projeção, mais útil durante o mês.

**Teste independente**: Abrir o mês atual e conferir que a linha dele vai até hoje ou até a última despesa lançada no
mês, o que vier depois, enquanto a linha do mês anterior cobre todos os dias dele, com tooltips em BRL.

**Cenários de aceitação**:

1. **Dado** o mês atual, **Quando** o gráfico de linha é exibido, **Então** a linha do mês, na cor primária do tema
   (`primary`), vai até hoje ou até a última despesa lançada no mês, o que vier depois, e a linha do mês anterior, na
   cor secundária (`secondary`), cobre o mês inteiro. A série vem pronta da API (`back/specs/003-financial-reports`,
   FR-013, alterado em 2026-10-09); o front só a desenha.
2. **Dado que** o ponteiro está sobre um dia, **Quando** o tooltip abre, **Então** ele mostra os totais dos dois meses
   até aquele dia, em BRL, com os nomes dos meses por extenso.

---

### História de usuário 6 - Conferir os detalhes (Prioridade: P6)

Como alguém que revisa um mês, quero as transações mais recentes do mês listadas da mais nova para a mais antiga, e um
jeito de ver todas, para conferir os detalhes por trás dos gráficos.

**Por que esta prioridade**: A página de transações já as lista; aqui elas acompanham os gráficos.

**Teste independente**: Abrir um mês com mais de dez transações, conferir que o bloco lista as dez mais recentes e
seguir "Ver todas" até a página de transações filtrada por esse mês.

**Cenários de aceitação**:

1. **Dado que** um mês tem transações, **Quando** a lista é exibida, **Então** as dez mais recentes aparecem com data
   (dd/MM), descrição, categoria e valor, da mais nova para a mais antiga, com as despesas em vermelho, sob um título
   que conta todas as transações do mês.
2. **Dado que** há mais transações do que cabem no bloco, **Quando** o usuário rola, **Então** só a lista rola, e não a
   página.
3. **Dada** a lista, **Quando** o usuário escolhe "Ver todas", **Então** a página de transações abre com os filtros
   visíveis e definidos para esse mês.

---

### Casos-limite

- Enquanto um bloco carrega, ele mostra um indicador de carregamento; os outros blocos não são afetados.
- Um bloco que falha mostra o que falhou e um botão "Tentar novamente" que tenta de novo só aquele bloco.
- Um mês sem movimentação mostra "Sem movimentações neste mês." no resumo e na lista; a rosca e a linha, que tratam de
  despesas, dizem que não houve despesas; as barras só mostram um estado vazio quando o período inteiro não teve
  movimentação.
- Um mês com dez transações ou menos lista todas; "Ver todas" continua abrindo a página de transações do mês.
- Recarregar um bloco que já mostra dados os mantém visíveis, esmaecidos, até os novos dados chegarem.
- Um mês futuro mostra projeção zero e nenhuma linha para os dias que ainda não chegaram.
- Janelas de desktop mostram a página inteira sem rolagem; janelas estreitas ou baixas empilham os blocos numa coluna e
  deixam a página rolar.
- No modo escuro, os gráficos usam as cores do tema escuro e são redesenhados quando o tema do sistema muda.
- Os nomes das categorias vêm do usuário; são exibidos como texto, nunca interpretados.

## Requisitos _(obrigatório)_

### Requisitos funcionais

- **FR-001**: A aplicação DEVE oferecer uma página de relatórios em `/reports`, com link no menu de navegação,
  carregada só quando aberta.
- **FR-002**: A página DEVE abrir no mês atual (America/Sao_Paulo) e oferecer, no topo, um seletor de mês que atualiza
  todos os blocos, com botões para o mês anterior e o próximo.
- **FR-003**: A página DEVE mostrar cinco cards de resumo — saldo, receitas, despesas, taxa de poupança e projeção —,
  com saldo, receitas e despesas comparados com o mês anterior por uma seta (↑/↓), o percentual, o nome do mês anterior
  e uma cor semântica (subir é bom para receitas e saldo, e ruim para despesas).
- **FR-004**: A página DEVE mostrar um gráfico de rosca das despesas do mês por categoria, nas cores das categorias,
  com no máximo cinco fatias: com mais de cinco categorias, as quatro maiores e "Outras" — a soma do resto — numa cor
  neutra.
- **FR-005**: A página DEVE mostrar barras de receitas e despesas dos doze meses que terminam no mês selecionado.
- **FR-006**: A página DEVE mostrar uma linha das despesas acumuladas do mês contra as do mês anterior.
- **FR-007**: A página DEVE listar as dez transações mais recentes do mês — data (dd/MM), descrição, categoria e
  valor —, da mais nova para a mais antiga, com rolagem dentro do bloco, e uma ação "Ver todas" que abre a página de
  transações filtrada pelo mês.
- **FR-008**: Cada bloco DEVE carregar de forma independente e mostrar os seus próprios estados de carregamento, vazio
  e erro, o de erro com um botão que tenta de novo só aquele bloco.
- **FR-009**: Os valores monetários DEVEM aparecer em BRL com formatação pt-BR, inclusive nos tooltips e eixos dos
  gráficos; os meses DEVEM aparecer por extenso onde são nomeados.
- **FR-010**: As despesas DEVEM aparecer no mesmo vermelho nos gráficos e na lista, exceto nas linhas da comparação
  acumulada, que DEVEM usar a cor primária do tema (`primary`) no mês atual e a secundária (`secondary`) no mês
  anterior; os cards de resumo as mostram como valores negativos, sem cor.
- **FR-011**: A página DEVE ocupar a altura disponível da janela sem rolagem da página no desktop, numa grade de cards
  com cantos arredondados, espaçamentos consistentes e títulos; janelas estreitas ou baixas DEVEM recolhê-la numa única
  coluna com rolagem.
- **FR-012**: A página DEVE seguir os temas claro e escuro da aplicação.
- **FR-013**: A página de transações DEVE aceitar um mês no seu endereço (`?month=YYYY-MM`) e abrir com os filtros
  visíveis e definidos para esse mês.

### Entidades principais _(inclua se a feature envolver dados)_

- **Mês do relatório**: O mês que todos os blocos mostram; o mês atual por padrão.
- **Resumo mensal**, **Despesa da categoria**, **Receita e despesa mensais**, **Despesa acumulada**, **Transação do
  mês** _(de [[003-financial-reports]], do backend)_: os cinco relatórios que os blocos exibem, como a API os devolve.

## Critérios de sucesso _(obrigatório)_

### Resultados mensuráveis

- **SC-001**: Numa janela de desktop de 1366×768 ou maior, os cinco blocos ficam visíveis ao mesmo tempo, sem rolar a
  página.
- **SC-002**: Escolher um mês atualiza os cinco blocos com essa única ação.
- **SC-003**: Um bloco que falha nunca esconde nem bloqueia os outros quatro, e pode ser tentado de novo sozinho.
- **SC-004**: 100% dos valores monetários da página estão em BRL com formatação pt-BR, e 100% das despesas nos
  gráficos e na lista usam o mesmo vermelho, exceto as linhas da comparação acumulada, que usam `primary` (mês atual) e
  `secondary` (mês anterior).
- **SC-005**: Usuários que nunca abrem a página de relatórios não baixam nada a mais quando a aplicação inicia.

## Premissas

- Os cinco relatórios vêm da feature de backend [[003-financial-reports]], pedida junto com esta página e entregue
  antes; os seus formatos estão em `back/specs/003-financial-reports/contracts/reports-api.yaml`.
- O seletor de mês é o campo de mês que a aplicação já tem, que mostra o mês como `MM/yyyy`; o painel escreve o mês
  por extenso onde nomeia um (comparações, legendas, tooltips).
- Nas barras, as receitas aparecem no verde do tema (`tertiary`) e as despesas em vermelho; a legenda, os tooltips e a
  ordem fixa das barras as distinguem para usuários daltônicos.
- A API mantém os seus próprios limites — categorias agrupadas em "Outras" quando passam de oito, todas as transações
  do mês — e a página os reduz a cinco fatias e dez linhas, então o backend não muda.
- As barras cobrem doze meses, o padrão da API; escolher a quantidade de meses está fora do escopo.
- Recolher a grade em telas estreitas é o único comportamento responsivo; fora isso, a aplicação continua com layout
  de desktop.
