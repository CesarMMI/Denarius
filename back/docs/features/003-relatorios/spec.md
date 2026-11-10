# Relatórios financeiros

## Objetivo

Dar ao painel de relatórios cinco relatórios somente leitura, um por necessidade, cada um sobre um mês do calendário: resumo mensal, despesas por categoria, receitas vs. despesas dos últimos meses, despesa diária acumulada contra a do mês anterior e transações do mês. Os totais saem do banco e as datas seguem o fuso America/Sao_Paulo. Cada transação do mês traz a cor da categoria, para o painel pintá-la sem uma segunda consulta.

## Cenários

**Resumo mensal**

1. **Dado** um mês com 5.000,00 de receitas e 3.000,00 de despesas, **quando** o resumo é pedido, **então** ele traz receita 5.000,00, despesa 3.000,00, saldo 2.000,00 e taxa de poupança de 40%.
2. **Dado** um mês só com despesas, **quando** o resumo é pedido, **então** a taxa de poupança é não aplicável, e não 0% nem erro.
3. **Dado** que o mês anterior teve 4.000,00 de receitas e o selecionado tem 5.000,00, **quando** o resumo é pedido, **então** ele traz a receita anterior e um aumento de 25%. Se o mês anterior não teve despesas, a variação das despesas é não aplicável.
4. **Dado** que hoje é dia 10 de um mês de 30 dias, com 900,00 gastos até hoje e nada lançado depois, **quando** o resumo do mês atual é pedido, **então** a despesa projetada é 2.700,00 e o saldo projetado é a receita menos 2.700,00.
5. **Dado** o mesmo mês, mas com 300,00 lançados para o dia 20, **quando** o resumo é pedido, **então** a despesa projetada é 3.000,00 (900,00 ÷ 10 × 30 + 300,00).
6. **Dado** um mês já terminado, ou o último dia do mês atual, **quando** o resumo é pedido, **então** a despesa e o saldo projetados são iguais aos reais. Num mês que ainda não começou, são zero.
7. **Dado** que nenhum mês foi escolhido, **quando** o resumo é pedido, **então** ele cobre o mês atual em São Paulo.

**Despesas por categoria**

1. **Dado** 600,00 em Mercado, 300,00 em Lazer e 100,00 em Transporte, **quando** o relatório é pedido, **então** vêm nessa ordem, com participações de 60%, 30% e 10%, total de 1.000,00 e o nome e a cor de cada categoria.
2. **Dado** despesas em dez categorias, **quando** o relatório é pedido, **então** as sete maiores aparecem e as outras três somam uma entrada final "Outras". Com exatamente oito categorias, as oito aparecem e não há "Outras".
3. **Dado** um mês só com receitas, **quando** o relatório é pedido, **então** a lista vem vazia e o total é zero.

**Receitas vs. despesas**

1. **Dado** movimentações só em janeiro e março, **quando** são pedidos os três meses que terminam em março, **então** vêm janeiro, fevereiro (zerado) e março, em ordem cronológica.
2. **Dado** que a quantidade não foi informada, **quando** a série é pedida, **então** ela cobre os doze meses que terminam no mês selecionado; quatro meses terminando em fevereiro começam em novembro do ano anterior.
3. **Dado** uma quantidade menor que 1 ou maior que 24, **quando** a série é pedida, **então** a solicitação é rejeitada com uma explicação.

**Despesas acumuladas**

1. **Dado** um mês passado com despesas nos dias 3 e 10, **quando** o acumulado é pedido, **então** há um valor por dia do mês, constante entre uma despesa e outra, e o último valor é a despesa total do mês.
2. **Dado** que hoje é dia 15 e nada foi lançado depois, **quando** o acumulado do mês atual é pedido, **então** a série dele para no dia 15 e a do mês anterior cobre todos os dias dele.
3. **Dado** que hoje é dia 9 de um mês de 31 dias, com despesas de 100,00 no dia 2, 50,00 no dia 7 e 30,00 no dia 15, **quando** o acumulado é pedido, **então** a série vai do dia 1 ao 15, vale 150,00 do dia 7 ao 14 e 180,00 no dia 15. Uma receita lançada depois do dia 15 não a estende.
4. **Dado** que hoje é dia 9 e há uma despesa no último dia do mês, **quando** o acumulado é pedido, **então** a série cobre o mês inteiro.
5. **Dado** que o mês selecionado é janeiro, **quando** o acumulado é pedido, **então** ele é comparado com dezembro do ano anterior, e cada mês informa quantos dias tem.

**Transações do mês**

1. **Dado** um mês com 30 transações, **quando** o relatório é pedido, **então** voltam as 30, sem paginação nem limite.
2. **Dado** uma despesa de 50,00 em Mercado, **quando** o relatório é pedido, **então** ela traz o nome "Mercado", o tipo "saída" e o valor 50,00.
3. **Dado** duas transações no mesmo dia, **quando** o relatório é pedido, **então** a registrada por último vem primeiro.
4. **Dado** um mês com transações em categorias de cores diferentes, **quando** o relatório é pedido, **então** cada item traz a cor da categoria da sua transação, como `#` e 6 dígitos hexadecimais em maiúsculas, o mesmo formato do relatório de despesas por categoria.
5. **Dado** uma categoria cuja cor foi alterada depois de criada, **quando** o relatório é pedido, **então** os itens dela trazem a cor atual.
6. **Dado** um cliente que só conhece os campos anteriores do relatório, **quando** ele o consulta, **então** continua recebendo todos eles, com os mesmos nomes, tipos e valores.

**Casos-limite**

- O mês é `YYYY-MM` (`2026-09`); `2026-9`, `2026-13`, `09-2026`, `2026-09-01` ou texto são rejeitados em todos os relatórios. Sem mês, vale o mês atual em São Paulo: às 22:00 de 30 de setembro lá ainda é setembro, mesmo que seja 1º de outubro em UTC.
- A data de uma transação é um dia do calendário, e não um instante: 1º de setembro pertence a setembro em qualquer fuso.
- Um mês sem transações devolve totais zerados, listas vazias e séries zeradas, nunca um erro.
- A taxa de poupança sem receita e a variação a partir de um valor anterior zero são não aplicáveis (sem valor), nunca zero, infinito ou erro. A variação é medida pelo tamanho do valor anterior, então um saldo de −100,00 para 50,00 é um aumento de 150%.
- Projeção do mês atual: o ritmo só conta despesas com data até hoje, e hoje conta como dia decorrido (no dia 1º, a despesa do dia vezes os dias do mês). Despesa lançada para depois de hoje no mesmo mês é tratada como certa: soma-se à projeção, fora do ritmo, sem contar duas vezes. Uma despesa única até hoje (o aluguel) entra no ritmo e é extrapolada; o usuário aceitou essa regra simples. No dia 1º, sem despesa até hoje, a projeção é só a soma das lançadas para depois. Despesas de meses seguintes não entram no mês atual, nem na série nem na projeção. Só a despesa é projetada; a receita do mês é tida como conhecida.
- O mês anterior a janeiro é dezembro do ano anterior, no resumo e no acumulado; a série de meses atravessa a virada do ano do mesmo jeito.
- Categorias com o mesmo gasto vêm em ordem alfabética. "Outras" vem sempre por último, mesmo que seu total passe o de categorias listadas.
- Participações e variações são arredondadas para duas casas, então as participações podem não somar exatamente 100%.
- Mês que ainda não começou não tem dia algum na série acumulada; mês passado tem todos. Quando o selecionado é o seguinte ao atual, a série dele fica vazia e a do mês anterior (o atual) segue o FR-013.
- Na lista de transações do mês, várias transações da mesma categoria trazem a mesma cor, e um mês sem transações continua devolvendo lista vazia.

## Requisitos

- **FR-001**: Todo relatório DEVE aceitar um mês opcional no formato `YYYY-MM` e DEVE cobrir o mês atual no fuso America/Sao_Paulo quando nenhum for informado.
- **FR-002**: Todo relatório DEVE rejeitar um mês que não seja um `YYYY-MM` válido, explicando o que está errado.
- **FR-003**: O sistema DEVE tratar as datas das transações como dias do calendário: uma transação pertence ao mês e ao dia da sua data, nunca deslocada por fuso horário.
- **FR-004**: O resumo mensal DEVE informar o mês, a receita total (entradas), a despesa total (saídas, como valor positivo) e o saldo (receita menos despesa).
- **FR-005**: O resumo mensal DEVE informar a taxa de poupança como o saldo dividido pela receita total, em percentual, e DEVE informá-la como não aplicável quando o mês não tiver receita.
- **FR-006**: O resumo mensal DEVE projetar a despesa e o saldo do fim do mês: para o mês atual, a despesa do dia 1º até hoje dividida pelos dias decorridos (incluindo hoje) vezes os dias do mês, mais a despesa com data depois de hoje no mesmo mês, arredondada para duas casas, e a receita do mês menos essa despesa; para um mês passado, os valores reais; para um mês futuro, zero.
- **FR-007**: O resumo mensal DEVE informar a receita total, a despesa total e o saldo do mês anterior e a variação percentual de cada um para o mês selecionado, não aplicável quando o valor anterior for zero.
- **FR-008**: O relatório de despesas por categoria DEVE listar, para o mês, cada categoria com despesas (identificador, nome, cor, valor gasto e participação na despesa total do mês), do maior valor para o menor, com a despesa total do mês.
- **FR-009**: Quando mais de oito categorias tiverem despesas no mês, o relatório de despesas por categoria DEVE listar as sete maiores individualmente e somar as demais numa entrada final "Outras", sem identificador nem cor.
- **FR-010**: O relatório de receitas vs. despesas DEVE devolver, em ordem cronológica, uma entrada por mês para a quantidade escolhida, terminando no mês selecionado, com receita, despesa e saldo, incluindo os meses sem movimentação, zerados.
- **FR-011**: O relatório de receitas vs. despesas DEVE cobrir 12 meses por padrão e DEVE rejeitar uma quantidade menor que 1 ou maior que 24, explicando o que está errado.
- **FR-012**: O relatório de despesas acumuladas DEVE devolver, para o mês selecionado e para o anterior, o total acumulado da despesa em cada dia, do dia 1º até onde o FR-013 manda a série parar, e a quantidade de dias de cada um dos dois meses.
- **FR-013**: No relatório de despesas acumuladas, a série de um mês passado DEVE cobrir o mês inteiro; a do mês atual DEVE ir até o dia mais tarde entre hoje e a data da última despesa do mês, incluindo as lançadas para depois de hoje (receitas não a estendem); a de um mês que ainda não começou NÃO DEVE ter nenhum dia. A regra vale para a série do mês selecionado e para a do anterior.
- **FR-014**: O relatório de transações do mês DEVE devolver todas as transações do mês, sem paginação nem limite, da data mais recente para a mais antiga (na mesma data, a registrada mais recentemente primeiro), cada uma com identificador, data, descrição, nome da categoria, tipo (entrada ou saída) e valor (positivo).
- **FR-015**: Os valores monetários DEVEM ser decimais exatos; os valores projetados e os percentuais DEVEM ser arredondados para duas casas.
- **FR-016**: Cada relatório DEVE trabalhar só com os meses que cobre, somando os valores onde estão armazenados, em vez de ler todo o histórico a cada solicitação.
- **FR-017**: O comportamento existente de transações e de categorias e os seus contratos NÃO DEVEM mudar.
- **FR-018**: Cada item do relatório de transações do mês DEVE incluir a cor da categoria da transação.
- **FR-019**: A cor DEVE ter o formato `#` seguido de 6 dígitos hexadecimais em maiúsculas, o mesmo da cor do relatório de despesas por categoria.
- **FR-020**: A cor devolvida DEVE ser a cor atual da categoria no momento da consulta.
- **FR-021**: A inclusão da cor DEVE ser aditiva: os campos, os tipos, a ordenação, os filtros e os status do relatório permanecem como estão.
- **FR-022**: O contrato do relatório de transações do mês DEVE descrever a cor como campo obrigatório e não nulo, em `contracts/monthly-transactions-api.yaml`, que estende o `reports-api.yaml` sem reescrevê-lo.
- **FR-023**: A listagem `/transactions` e os demais relatórios DEVEM permanecer inalterados pela inclusão da cor.

## Regras de negócio e erros

- Entrada e saída seguem o sinal do valor da transação ([transação](../../dominio/transacao.md)); a despesa é informada como positiva. Moeda única implícita (BRL), uso single-tenant.
- Percentuais na escala 0–100 (25,5 é 25,5%), com duas casas. A taxa de poupança (saldo ÷ receita × 100) pode ser negativa quando a despesa passa da receita.
- Mês inválido e quantidade de meses fora de 1 a 24 respondem 400 (`ValidationProblemDetails`), sem executar o relatório.
- A cor da transação do mês nunca é nula: toda transação pertence a uma categoria, que sempre tem cor (o `null` de "Outras", no relatório por categoria, não se aplica aqui). A cor não é dado sensível e não há entrada nova.

## Critérios de sucesso

- **SC-001**: O usuário vê a receita, a despesa, o saldo, a taxa de poupança, a projeção e a comparação com o mês anterior de um mês numa única solicitação.
- **SC-002**: Cada relatório responde em menos de um segundo com um histórico de 10.000 transações.
- **SC-003**: 100% dos meses sem movimentação devolvem zeros ou listas vazias em vez de erros, e 100% dos meses ou quantidades de meses inválidos são rejeitados com uma explicação.
- **SC-004**: Um ano de receitas e despesas volta como exatamente 12 meses consecutivos, sem lacunas, quaisquer que sejam os meses com movimentação.
- **SC-005**: A soma dos valores do relatório de despesas por categoria é sempre igual à despesa total dele e à despesa total do mês no resumo.
- **SC-006**: Em 100% dos casos, o último valor da série acumulada do mês atual é igual à despesa total do mês no resumo, inclusive com despesas lançadas para depois de hoje.
- **SC-007**: No mês atual, a despesa projetada nunca é menor do que a despesa total já lançada no mês.
- **SC-008**: Em 100% dos itens do relatório de transações de qualquer mês, a cor devolvida é igual à cor da categoria da transação.
- **SC-009**: 100% dos testes de contrato anteriores à cor continuam passando sem modificação.
- **SC-010**: O relatório de transações de um mês continua sendo obtido com uma única consulta ao banco, sem consulta por transação, e o tempo de resposta com a mesma massa de dados não passa de 110% do anterior à cor.

## Fora de escopo

Quem pode ver os relatórios (a aplicação é single-tenant) e mudanças no comportamento ou nos contratos de transações e categorias.
