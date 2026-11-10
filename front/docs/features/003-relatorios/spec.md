# Painel de relatórios

## Objetivo

Uma página `/reports` que mostre, de relance, como está um mês das finanças do usuário: resumo, gastos por categoria, tendência de receitas e despesas, ritmo de gastos e as transações mais recentes. O usuário escolhe o mês e todos os blocos o acompanham. Os dados vêm de [back/docs/features/003-financial-reports](../../../../back/docs/features/003-financial-reports/); a identidade visual está em [design.md](../../design.md).

## Cenários

- **Dado** o menu, **quando** o usuário escolhe "Relatórios", **então** a página abre no mês atual.
- **Dado** um mês com receitas e despesas, **então** os cinco cards mostram saldo, receitas, despesas (valor negativo), taxa de poupança e despesa projetada, em BRL.
- **Dado** que as despesas subiram em relação ao mês anterior, **então** o card mostra seta para cima, o percentual e o nome do mês anterior na cor "ruim"; receitas ou saldo subindo aparecem na cor "boa".
- **Dado** um mês sem receitas, **então** o card da taxa de poupança indica que não há taxa; **dado** que o mês anterior não tem valor para comparar, o card diz que não há comparação, em vez de mostrar um percentual.
- **Dado** que o usuário escolhe outro mês (no seletor ou em "Mês anterior" / "Próximo mês"), **então** os cinco blocos mostram esse mês, cada um carregado uma única vez.
- **Dado** que um bloco falhou, **quando** o usuário tenta de novo, **então** só esse bloco recarrega.
- **Dado** um mês com despesas em várias categorias, **então** a rosca tem uma fatia por categoria, na cor dela, da maior para a menor, com legenda; com mais de cinco categorias, as quatro maiores e uma quinta, "Outras", em cor neutra. O tooltip mostra a categoria, o valor em BRL e a participação nas despesas do mês.
- **Dado** o mês selecionado, **então** as barras cobrem os doze meses que terminam nele, do mais antigo ao mais recente, receitas em verde e despesas em vermelho; um mês sem movimentação continua no eixo, zerado. O tooltip mostra o mês por extenso e os dois valores em BRL.
- **Dado** o mês atual, **então** a linha do mês (`primary`) vai até hoje ou até a última despesa lançada no mês, o que vier depois, e a do mês anterior (`secondary`) cobre o mês inteiro. A série vem pronta da API (back FR-013, alterado em 2026-10-09); o front só a desenha. O tooltip mostra os totais dos dois meses até o dia, em BRL.
- **Dado** um mês com transações, **então** a lista mostra as dez mais recentes, da mais nova à mais antiga, com data (dd/MM), descrição, categoria e valor, despesas em vermelho, sob um título que conta todas as transações do mês; só a lista rola, não a página.
- **Dado** a lista, **quando** o usuário escolhe "Ver todas", **então** a página de transações abre com os filtros visíveis e definidos para o mês.
- Casos-limite: mês com dez transações ou menos lista todas e mantém "Ver todas"; mês futuro tem projeção zero e nenhuma linha para os dias que não chegaram; nomes de categoria vêm do usuário e são exibidos como texto; recarregar um bloco que já tem dados os mantém visíveis, esmaecidos, até os novos chegarem.

## Requisitos

- **FR-001**: A aplicação DEVE oferecer uma página de relatórios em `/reports`, com link no menu de navegação, carregada só quando aberta.
- **FR-002**: A página DEVE abrir no mês atual (America/Sao_Paulo) e oferecer, no topo, um seletor de mês que atualiza todos os blocos, com botões para o mês anterior e o próximo.
- **FR-003**: A página DEVE mostrar cinco cards de resumo (saldo, receitas, despesas, taxa de poupança e projeção), com saldo, receitas e despesas comparados com o mês anterior por uma seta (↑/↓), o percentual, o nome do mês anterior e uma cor semântica (subir é bom para receitas e saldo, e ruim para despesas).
- **FR-004**: A página DEVE mostrar um gráfico de rosca das despesas do mês por categoria, nas cores das categorias, com no máximo cinco fatias: com mais de cinco categorias, as quatro maiores e "Outras" (a soma do resto) numa cor neutra.
- **FR-005**: A página DEVE mostrar barras de receitas e despesas dos doze meses que terminam no mês selecionado.
- **FR-006**: A página DEVE mostrar uma linha das despesas acumuladas do mês contra as do mês anterior.
- **FR-007**: A página DEVE listar as dez transações mais recentes do mês (data dd/MM, descrição, categoria e valor), da mais nova à mais antiga, com rolagem dentro do bloco, e uma ação "Ver todas" que abre a página de transações filtrada pelo mês.
- **FR-008**: Cada bloco DEVE carregar de forma independente e mostrar os seus próprios estados de carregamento, vazio e erro, o de erro com um botão que tenta de novo só aquele bloco.
- **FR-009**: Os valores monetários DEVEM aparecer em BRL com formatação pt-BR, inclusive nos tooltips e eixos dos gráficos; os meses DEVEM aparecer por extenso onde são nomeados.
- **FR-010**: As despesas DEVEM aparecer no mesmo vermelho nos gráficos e na lista, exceto nas linhas da comparação acumulada, que DEVEM usar `primary` no mês atual e `secondary` no mês anterior; os cards de resumo as mostram como valores negativos, sem cor. Nas barras, as receitas usam o verde do tema (`tertiary`); legenda, tooltips e ordem fixa das barras as distinguem para daltônicos.
- **FR-011**: A página DEVE ocupar a altura disponível da janela sem rolagem da página no desktop, numa grade de cards com cantos arredondados, espaçamentos consistentes e títulos; janelas estreitas ou baixas DEVEM recolhê-la numa única coluna com rolagem.
- **FR-012**: A página DEVE seguir os temas claro e escuro da aplicação; os gráficos são redesenhados quando o tema do sistema muda.
- **FR-013**: A página de transações DEVE aceitar um mês no seu endereço (`?month=YYYY-MM`) e abrir com os filtros visíveis e definidos para esse mês.

## Fluxo e estados

- Cabeçalho com o seletor de mês (campo de mês da aplicação, `MM/yyyy`); abaixo, a grade: cinco cards de resumo e, embaixo, rosca, barras, linha e a lista de transações.
- Carregando: indicador no bloco. Erro: o que falhou e "Tentar novamente".
- Vazio: "Sem movimentações neste mês." no resumo e na lista; a rosca e a linha dizem que não houve despesas; as barras só ficam vazias se o período inteiro não teve movimentação.
- O recolhimento da grade em telas estreitas é o único comportamento responsivo.

## Critérios de sucesso

- **SC-001**: Numa janela de desktop de 1366×768 ou maior, os cinco blocos ficam visíveis ao mesmo tempo, sem rolar a página.
- **SC-002**: Escolher um mês atualiza os cinco blocos com essa única ação.
- **SC-003**: Um bloco que falha nunca esconde nem bloqueia os outros quatro, e pode ser tentado de novo sozinho.
- **SC-004**: 100% dos valores monetários da página estão em BRL com formatação pt-BR, e 100% das despesas nos gráficos e na lista usam o mesmo vermelho, exceto as linhas da comparação acumulada (`primary` e `secondary`).
- **SC-005**: Usuários que nunca abrem a página de relatórios não baixam nada a mais quando a aplicação inicia.

## Fora de escopo

Mudanças no back (a API já limita as categorias a oito e devolve todas as transações do mês; a página reduz a cinco fatias e dez linhas); escolher a quantidade de meses das barras (doze, o padrão da API); uma linha de saldo sobre as barras (testada e descartada: poluía o gráfico); responsividade além de recolher a grade.
