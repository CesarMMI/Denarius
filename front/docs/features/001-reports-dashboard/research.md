# Pesquisa da Fase 0: Painel de relatórios

## A biblioteca de gráficos e como ela é adicionada

- **Decisão**: `npx ng add ng2-charts`, que instala `ng2-charts` e `chart.js` e configura
  `provideCharts(withDefaultRegisterables())`. O ng2-charts 11.x exige o Angular 22; o `ng add` escolhe a versão mais
  nova cujas peer dependencies combinam com o Angular instalado, então o projeto recebe a 10.0.0 (peer
  `@angular/core >=21`). Nada é instalado à mão.
- **Justificativa**: O pedido nomeia a biblioteca e pede o seu schematic.
- **Alternativas consideradas**: Instalar `ng2-charts@11` — falha nas peer dependencies do Angular 21.

## Onde o Chart.js é provido

- **Decisão**: Manter o provider do schematic, mas movê-lo de `app.config.ts` para os `providers` da rota de
  relatórios (`reports.routes.ts`), que o `app.routes.ts` já carrega sob demanda. O provider também define a fonte
  padrão do Chart.js como a Roboto do app.
- **Justificativa**: O `withDefaultRegisterables()` importa todos os controllers, elementos, escalas e plugins do
  Chart.js (~200 kB). Provido em `app.config.ts`, tudo isso entra no bundle inicial (630 kB hoje, aviso do budget em
  700 kB) e é baixado por usuários que nunca abrem os relatórios — o tipo de regressão que o budget existe para pegar.
  Um provider na rota dá à página de relatórios a mesma configuração, e o Chart.js carrega com a rota.
- **Alternativas consideradas**: Deixar o provider em `app.config.ts` — segue o `ng add` ao pé da letra, mas quebra a
  regra do budget; registrar só as peças necessárias do Chart.js — menor, mas se afasta do
  `withDefaultRegisterables()` do pedido para um chunk que já é lazy.

## Quem carrega os dados de cada bloco

- **Decisão**: A `ReportsPage` é dona de cinco `httpResource`s, um por relatório, guiados pelo seu signal `month`.
  Cada componente de bloco recebe o seu próprio `Resource` como input e emite `retry`; a página chama `reload()` só
  naquele resource.
- **Justificativa**: O pedido quer que cada card busque os próprios dados "para que carregamento e erro sejam
  independentes"; a constituição do front (Princípio II) põe o estado de `httpResource` nas páginas e mantém
  `components/` de apresentação, sem HTTP. Um resource por bloco mantém cada bloco independente — a sua própria
  requisição, carregamento, erro e nova tentativa — e respeita a fronteira. É também o formato que as tabelas já usam
  (o `TransactionsTable` recebe um `Resource`).
- **Alternativas consideradas**: Um `httpResource` dentro de cada card — o que o pedido diz literalmente, mas uma
  violação da constituição sem ganho em relação a um resource por bloco na página.

## Estados de carregamento, vazio e erro

- **Decisão**: Um componente `report-card` — `mat-card` com `mat-card-title` e, depois, um `mat-progress-spinner`
  (carregando, sem nada para mostrar), o texto de erro com um `matButton` "Tentar novamente" (erro), o texto de vazio
  (carregado, sem nada para mostrar) ou o conteúdo do bloco passado como `ng-template`. Recarregar um bloco que tem
  dados os mantém na tela com opacidade reduzida, em vez de trocá-los pelo spinner. Textos de vazio: "Sem movimentações
  neste mês." para o resumo e a lista (o texto do pedido), "Sem despesas neste mês." para a rosca, "Sem despesas neste
  mês nem no anterior." para a linha e "Sem movimentações neste período." para as barras, que cobrem doze meses.
- **Justificativa**: Cinco blocos compartilham um comportamento de estados; o input de template faz o conteúdo só
  renderizar quando há dados, então um gráfico nunca é criado sobre um relatório vazio ou que falhou. O resource do
  Angular limpa o `value()` quando a requisição muda (um mês novo → spinner) e o mantém no `reload()` (→ conteúdo
  esmaecido), o que foi conferido na implementação de resource do `@angular/core`. Um spinner do Material, em vez de
  um skeleton feito à mão, segue a regra "Material primeiro" do projeto. A rosca e a linha tratam de despesas, então
  "sem movimentação" seria falso num mês só com receitas.
- **Alternativas consideradas**: Repetir os estados em cada bloco — cinco cópias do mesmo markup e dos mesmos
  estilos; `@if` em volta de `<ng-content>` — o conteúdo projetado é criado mesmo escondido, então o gráfico seria
  criado sobre dados ausentes.

## Layout

- **Decisão**: O host da página ocupa `height: 100%` do `mat-sidenav-content` (o container da sidenav é `fullscreen`,
  então isso é a viewport menos o padding do conteúdo) — o equivalente, neste projeto, a `100dvh`. Abaixo do cabeçalho
  da página, uma grade CSS (`gap: 1rem`) com `grid-template-areas`:

  ```
  "summary    summary  summary  summary"
  "categories bars     bars     transactions"
  "categories line     line     transactions"
  ```

  com colunas `repeat(3, minmax(0, 1fr)) minmax(18rem, 1.2fr)` e linhas `auto minmax(0, 1fr) minmax(0, 1fr)`. Os
  cinco cards de resumo dividem a primeira linha; a rosca e as transações são blocos altos, um de cada lado; as duas
  séries temporais ocupam o meio, mais largo. Abaixo de 1200 px de largura ou de 600 px de altura, uma media query
  passa para uma coluna (`summary`, `categories`, `bars`, `line`, `transactions`), dá alturas fixas aos blocos e deixa
  a página rolar.

- **Justificativa**: As áreas do pedido, ajustadas como ele permite: quatro blocos lado a lado na segunda linha
  deixariam cada série temporal com um quarto da largura, estreito demais para doze meses de barras em pares ou 31 dias
  de linhas; empilhar as duas séries numa coluna de largura dupla lhes dá espaço, enquanto a rosca e a lista usam a
  altura toda.
- **Alternativas consideradas**: A segunda linha literal, com quatro colunas — séries apertadas; `100dvh` na página —
  ignora o cabeçalho, o padding e o layout da sidenav e transbordaria.

## Cores

- **Decisão**: Toda cor é um token `--mat-sys-*`. Despesa: `error`, nos gráficos e na lista (a classe global
  `.negative` já o usa); os cards de resumo mostram as despesas como valores negativos, sem cor. Receita: `tertiary`, o
  verde do tema. Indicadores de variação: "bom" em `tertiary`, "ruim" em `error`, sempre com ↑/↓ e o percentual em
  texto. A rosca usa a cor de cada categoria, "Outras" em `outline` (a neutra) e um espaço na cor da superfície do card
  entre as fatias. As linhas da comparação acumulada usam `primary` (mês atual) e `secondary` (mês anterior). As
  linhas de grade usam `surface-container-high`, e o texto dos gráficos, `on-surface`. O canvas não lê variáveis CSS,
  então o `ChartThemeService` resolve esses tokens para cores concretas por meio de um elemento de sondagem, de novo a
  cada mudança de `prefers-color-scheme`, e os gráficos recalculam as suas opções a partir dele.
- **Justificativa**: A primeira versão tinha a receita em `outline`, o par que passou na verificação de daltonismo
  (CVD) do dataviz; depois, o dono escolheu o verde do tema, e a sua semente foi dessaturada (`#4CAF50` → `#71A96C`)
  para não pesar mais que o vermelho no modo escuro, em que `error` é um tom pastel 80. Verde contra vermelho falha na
  verificação de deuteranopia (ΔE 7,0 no claro / 2,8 no escuro), então a legenda, o tooltip e a ordem fixa das barras
  também carregam a identidade. O vermelho nos cards de resumo chamava atenção demais, então o dono o removeu dali. Ler
  os tokens mantém os gráficos no tema, no claro e no escuro. As linhas da comparação acumulada usam o par da cor
  primária por escolha do dono (Esclarecimentos, 2026-10-06); as duas têm a mesma luminância, então a legenda e o
  tooltip as distinguem. A grade em `surface-container-high` é o mesmo token das divisórias da tabela.
- **Alternativas consideradas**: Receita em `outline` — a primeira versão, validada, substituída pelo verde do dono;
  valores hex para uma receita azul — quebra a regra de "só tokens" e se afasta do tema. Linhas em `error`/`outline`
  (vermelho e neutra) — a primeira versão, com mais diferença de luminância entre as linhas, substituída pelo par da
  cor primária do dono; `primary`/`on-primary` — `on-primary` quase some sobre o card (1,11:1 no claro). Grade em
  `surface-container-highest` — um pouco mais visível, mas diverge das divisórias da tabela.

## No máximo cinco fatias na rosca

- **Decisão**: O `expenses-by-category-chart` reduz os itens da API a cinco: com mais de cinco, as quatro maiores
  mantêm as suas fatias e o resto — incluindo o próprio "Outras" da API — soma numa quinta, "Outras", cuja
  participação é recalculada a partir do `total` do mês. A legenda, o tooltip e o `aria-label` usam a lista reduzida.
- **Justificativa**: Pedido pelo dono antes de terminar. A API ordena as categorias da maior para a menor e agrupa a
  partir de oito, com o seu "Outras" por último, então os quatro primeiros itens são sempre categorias com nome.
  Agrupar no front mantém o backend como está (tarefas de front não mudam o `back/`).
- **Alternativas consideradas**: Baixar o limite da API de oito para cinco — uma mudança no backend por uma escolha
  de apresentação.

## Transações mais recentes e "Ver todas"

- **Decisão**: O `transactions-list` mostra as dez primeiras transações do mês (a API as envia das mais recentes para
  as mais antigas) e continua contando todas no título. Abaixo da tabela, um link `matButton` "Ver todas" abre
  `/transactions?month=YYYY-MM` (novo input `month`, vindo da página). A página de transações lê `month` da query
  string, como já lê `categoryId` (`DateUtils.fromMonthKey`, que ignora tudo que não for um `YYYY-MM` válido), e abre
  com os filtros visíveis naquele mês.
- **Justificativa**: Pedido pelo dono antes de terminar. O mesmo padrão de link com query string da contagem de
  transações da tabela de categorias; um link (e não estado do router) sobrevive a um reload e pode ser compartilhado.
- **Alternativas consideradas**: Um `limit` na API — uma mudança no backend que perderia a contagem do mês.

## Linha de saldo sobre as barras (descartada)

- **Decisão**: Nenhuma — as barras continuam como estavam.
- **Justificativa**: Uma linha com o saldo de cada mês, na cor do texto, sobre as barras, foi construída e testada; o
  dono achou que ela poluía o gráfico e pediu para removê-la.

## Formatação

- **Decisão**: Os templates usam `CurrencyPipe` (`'BRL'`), `DatePipe` (`'dd/MM'` em `UTC` para as datas da API, como
  na tabela de transações) e `PercentPipe`, sob o `LOCALE_ID` do app (`pt-BR`). Os callbacks dos gráficos (tooltips,
  eixos) usam `Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })`, compacto nos eixos de valor. Os
  meses aparecem por extenso onde são nomeados — "vs. agosto", legendas e tooltips como "setembro de 2026" — e
  abreviados só no eixo das barras: "set.", com o ano numa segunda linha sob o primeiro mês e sob cada janeiro, para
  que os rótulos não precisem inclinar.
- **Justificativa**: As regras pt-BR do pedido, com a formatação que o app já usa.

## Seletor de mês

- **Decisão**: O `app-month-field` compartilhado no cabeçalho da página, sem opção de limpar, começando no mês atual
  em São Paulo (`DateUtils.currentMonth()`, a partir de `Intl.DateTimeFormat` com
  `timeZone: 'America/Sao_Paulo'`). O service o envia como `month=YYYY-MM` (`DateUtils.toMonthKey`).
- **Justificativa**: Reaproveita o seletor de mês do app; ele mostra `MM/yyyy`, que o dono definiu para todos os
  campos de mês (commit `40b8e1d`), então o painel escreve os meses por extenso no próprio conteúdo. Começar no mês de
  São Paulo mantém a primeira visão alinhada com o "mês atual" da API.
- **Alternativas consideradas**: Omitir `month` no primeiro carregamento e deixar a API escolher — o campo não
  mostraria nada.

## Formas e marcas dos gráficos

- **Decisão**: Seguindo o método do dataviz: uma linha de KPIs com stat tiles (rótulo pequeno, valor grande,
  variação) para o resumo; uma rosca (como pedido; no máximo oito fatias, a API agrupa a cauda em "Outras"); colunas
  agrupadas para receitas vs. despesas (barras de no máximo 24 px, topos arredondados de 4 px, um tooltip por mês
  listando as duas séries); e linhas (2 px, sem marcadores de ponto, exceto no hover) para a comparação acumulada, com
  o mês atual na cor primária (`primary`) e o mês anterior na secundária (`secondary`), com tooltip em modo índice.
  Todos os gráficos usam a legenda e o tooltip do próprio Chart.js — os rótulos da rosca trazem a participação de cada categoria
  ("Mercado (23,08%)") e o seu tooltip acrescenta o valor. As linhas de grade são finíssimas, os eixos discretos,
  `maintainAspectRatio: false` para o gráfico ocupar o card. Cada canvas recebe um `aria-label` com os valores que
  desenha.
- **Justificativa**: Marcas finas e moldura discreta, a cor fazendo um único trabalho por gráfico, tooltips que nunca
  são o único acesso a um valor (a legenda, os eixos e o `aria-label` também o trazem). A legenda nativa, em vez de
  uma lista em HTML, reduz ao mínimo o código próprio do front (o dono pediu isso durante a implementação).
- **Alternativas consideradas**: Uma legenda em HTML com valor e participação ao lado da rosca — construída primeiro e
  substituída pela legenda nativa: cerca de sessenta linhas de template, estilos e testes para o que a legenda e o
  tooltip já mostram.

## Achados da validação (renderizado no Chrome, 2026-10-03)

- A legenda nativa da rosca precisa de altura: num bloco de uma linha, ela cortava as três últimas de oito categorias,
  então a rosca fica com a área alta `categories` (linhas 2–3) do layout acima.
- Palavras longas numa descrição impediam a tabela de transações de encolher e empurravam o valor para fora do card em
  1366 px; a coluna de descrição agora ocupa a largura restante e trunca com reticências, e a tabela lateral usa
  padding de célula de 8 px.
- Títulos de card em duas linhas ("Projeção de despesas") desalinhavam a linha de resumo; os rótulos dos cards usam o
  estilo de título pequeno, para que o valor seja a parte mais forte de cada tile.
- Em 1366×768 e 1440×900, a altura de rolagem da página é igual à sua altura visível (sem rolagem da página); com
  1024 px de largura, os blocos se empilham e a página rola.

## Abordagem de testes

- **Decisão**: Specs modelo, conforme a skill `write-front-tests`: o service com `HttpTestingController`; os
  componentes dos blocos com `resourceFromSnapshots`; a página com `HttpTestingController` e providers fake sem
  `MatDialog`; o `app.spec.ts` ganha o novo link. Os componentes de gráfico são testados pelos inputs que entregam ao
  gráfico (`type`, `data`, `options`, callbacks do tooltip): os specs substituem a `BaseChartDirective` do ng2-charts
  por uma diretiva fake com o mesmo seletor e os mesmos inputs.
- **Justificativa**: O jsdom não tem canvas; o fake mantém os testes focados no que os componentes decidem (dados,
  cores, formatação), e não no desenho do Chart.js.
