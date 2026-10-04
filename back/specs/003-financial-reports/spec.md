# Especificação da feature: Relatórios financeiros

**Branch da feature**: `003-financial-reports`

**Criada em**: 2026-10-03

**Status**: Rascunho

**Entrada**: Descrição do usuário: "Relatórios financeiros para um painel de relatórios, um relatório
por necessidade, cada um respondendo por um mês do calendário (`YYYY-MM`; o mês atual quando
omitido; um mês inválido é rejeitado): (1) um resumo mensal — receita total, despesa total, saldo,
taxa de poupança, uma projeção da despesa e do saldo no fim do mês e os totais do mês anterior com a
variação percentual de cada um; (2) despesas por categoria com a participação de cada categoria, das
maiores para as menores, com a cauda agrupada em 'Outras' quando houver mais de 8 categorias; (3)
receitas vs. despesas dos últimos N meses (padrão 12, entre 1 e 24) como uma série contínua, com zeros
nos meses sem movimentação; (4) a despesa diária acumulada do mês ao lado da do mês anterior, com o
mês atual só até hoje; (5) todas as transações do mês, das mais recentes para as mais antigas, com o
nome da categoria, o tipo e o valor. Dinheiro em decimais, datas no fuso horário America/Sao_Paulo,
totais calculados pelo banco e índices em data e categoria onde faltarem."

## Cenários de usuário e testes *(obrigatório)*

### História de usuário 1 - Ver num relance como está o mês (Prioridade: P1)

Como alguém que acompanha as finanças pessoais, quero ver a receita, a despesa, o saldo e a taxa de
poupança de um mês, como o mês deve fechar e como ele se compara com o mês anterior, para saber em
poucos segundos se estou no caminho certo.

**Por que esta prioridade**: É o destaque do painel e o único relatório que responde sozinho "como
estou indo?". Todos os outros relatórios detalham o que este resume.

**Teste independente**: Registrar receitas e despesas em dois meses consecutivos, pedir o resumo do
segundo mês e confirmar todos os totais, a taxa de poupança, a projeção e a comparação com o
primeiro mês — sem precisar de nenhum outro relatório.

**Cenários de aceitação**:

1. **Dado que** um mês tem 5.000,00 de receitas e 3.000,00 de despesas, **Quando** o seu resumo é
   pedido, **Então** ele mostra receita total 5.000,00, despesa total 3.000,00, saldo 2.000,00 e
   taxa de poupança de 40%.
2. **Dado que** um mês tem despesas, mas nenhuma receita, **Quando** o seu resumo é pedido,
   **Então** a taxa de poupança é informada como não aplicável, e não como 0% ou como erro.
3. **Dado que** o mês anterior teve 4.000,00 de receitas e o mês selecionado tem 5.000,00,
   **Quando** o resumo é pedido, **Então** ele mostra a receita do mês anterior e um aumento de 25%.
4. **Dado que** o mês anterior não teve despesas, **Quando** o resumo é pedido, **Então** a
   variação das despesas é informada como não aplicável, e não como erro ou valor infinito.
5. **Dado que** hoje é dia 10 de um mês de 30 dias e foram gastos 900,00 do dia 1º até hoje,
   **Quando** o resumo do mês atual é pedido, **Então** a despesa projetada é 2.700,00 e o saldo
   projetado é a receita do mês menos 2.700,00.
6. **Dado que** o mês já terminou, **Quando** o seu resumo é pedido, **Então** a despesa e o saldo
   projetados são iguais à despesa e ao saldo reais.
7. **Dado que** o mês ainda não começou, **Quando** o seu resumo é pedido, **Então** a despesa e o
   saldo projetados são zero.
8. **Dado que** nenhum mês foi escolhido, **Quando** o resumo é pedido, **Então** ele cobre o mês
   atual no horário de São Paulo.

---

### História de usuário 2 - Ver para onde foi o dinheiro (Prioridade: P2)

Como alguém que revisa um mês, quero ver quanto gastei em cada categoria e que parte dos gastos do
mês cada uma representa, das maiores para as menores, para saber onde cortar.

**Por que esta prioridade**: É o primeiro detalhamento que qualquer pessoa pede depois do resumo. Só
precisa das despesas do mês e das categorias que já existem.

**Teste independente**: Registrar despesas em várias categorias num mesmo mês e confirmar o total de
cada categoria, a sua participação nos gastos do mês, a ordem e o agrupamento em "Outras" quando há
mais de oito categorias.

**Cenários de aceitação**:

1. **Dado que** num mês foram gastos 600,00 em Mercado, 300,00 em Lazer e 100,00 em Transporte,
   **Quando** as despesas por categoria são pedidas, **Então** elas vêm das maiores para as menores,
   com participações de 60%, 30% e 10% e total de 1.000,00, cada uma com o nome e a cor da sua
   categoria.
2. **Dado que** um mês tem despesas em dez categorias, **Quando** as despesas por categoria são
   pedidas, **Então** as sete maiores aparecem individualmente e as outras três são somadas numa
   entrada final "Outras".
3. **Dado que** um mês tem despesas em exatamente oito categorias, **Quando** as despesas por
   categoria são pedidas, **Então** as oito aparecem e não há entrada "Outras".
4. **Dado que** um mês tem receitas, mas nenhuma despesa, **Quando** as despesas por categoria são
   pedidas, **Então** a lista vem vazia e o total é zero.

---

### História de usuário 3 - Acompanhar receitas vs. despesas ao longo dos meses (Prioridade: P3)

Como alguém que planeja o futuro, quero ver receitas e despesas lado a lado nos últimos meses, para
identificar tendências e os meses que deram errado.

**Por que esta prioridade**: Uma visão de tendência só compensa depois de alguns meses de histórico,
então vem depois dos relatórios centrados no mês.

**Teste independente**: Registrar movimentações em alguns, mas não em todos, dos últimos doze meses
e confirmar que a série tem exatamente doze meses em ordem cronológica, com zeros onde nada
aconteceu.

**Cenários de aceitação**:

1. **Dado que** só há movimentações em janeiro e março, **Quando** são pedidos os três meses que
   terminam em março, **Então** vêm janeiro, fevereiro e março, nessa ordem, com fevereiro zerado.
2. **Dado que** nenhuma quantidade de meses foi escolhida, **Quando** a série é pedida, **Então**
   ela cobre os doze meses que terminam no mês selecionado.
3. **Dado que** a série termina em fevereiro, **Quando** são pedidos quatro meses, **Então** ela
   começa em novembro do ano anterior.
4. **Dado que** a quantidade de meses é menor que 1 ou maior que 24, **Quando** a série é pedida,
   **Então** a solicitação é rejeitada com uma explicação.

---

### História de usuário 4 - Comparar o ritmo de gastos deste mês com o do mês passado (Prioridade: P4)

Como alguém que acompanha os gastos durante o mês, quero ver quanto eu já tinha gastado até cada dia
do mês ao lado de quanto tinha gastado até o mesmo dia do mês passado, para saber cedo se estou
gastando mais rápido que o normal.

**Por que esta prioridade**: É um refinamento da projeção do resumo — útil durante o mês, mas o
resumo já diz para onde o mês está indo.

**Teste independente**: Registrar despesas em alguns dias de dois meses consecutivos e confirmar os
dois totais acumulados dia a dia, onde eles param e a quantidade de dias de cada mês.

**Cenários de aceitação**:

1. **Dado que** um mês passado tem despesas nos dias 3 e 10, **Quando** os seus gastos acumulados
   são pedidos, **Então** há um valor para cada dia do mês, que não muda entre uma despesa e outra,
   e o último valor é a despesa total do mês.
2. **Dado que** hoje é dia 15, **Quando** os gastos acumulados do mês atual são pedidos, **Então** a
   série dele para no dia 15, enquanto a série do mês anterior cobre todos os dias dele.
3. **Dado que** o mês selecionado é janeiro, **Quando** os seus gastos acumulados são pedidos,
   **Então** ele é comparado com dezembro do ano anterior, e cada mês informa quantos dias tem.

---

### História de usuário 5 - Listar tudo o que aconteceu no mês (Prioridade: P5)

Como alguém que revisa um mês, quero ver todas as transações desse mês, das mais recentes para as
mais antigas, com a categoria, se foi entrada ou saída e o valor, para conferir os detalhes por trás
dos gráficos.

**Por que esta prioridade**: A lista de transações existente já pode ser restrita a um mês; este
relatório só junta o nome da categoria e o tipo de que o painel precisa.

**Teste independente**: Registrar transações em dois meses e confirmar que só voltam as transações
do mês selecionado — todas elas, das mais recentes para as mais antigas, com nome da categoria, tipo
e valor.

**Cenários de aceitação**:

1. **Dado que** um mês tem 30 transações, **Quando** as transações do mês são pedidas, **Então**
   voltam as 30, sem paginação nem limite.
2. **Dado que** há uma despesa de 50,00 em Mercado, **Quando** as transações do mês são pedidas,
   **Então** ela aparece com o nome de categoria "Mercado", o tipo "saída" e o valor 50,00.
3. **Dado que** há duas transações no mesmo dia, **Quando** as transações do mês são pedidas,
   **Então** a registrada por último vem primeiro.

---

### Casos-limite

- Um mês é escrito como ano com quatro dígitos e mês com dois (`2026-09`); qualquer outra coisa —
  `2026-9`, `2026-13`, `09-2026`, `2026-09-01`, texto — é rejeitada com uma explicação, em todos os
  relatórios.
- Sem mês escolhido, todos os relatórios cobrem o mês atual no horário de São Paulo: às 22:00 de 30
  de setembro em São Paulo ainda é setembro, mesmo que já seja 1º de outubro em UTC.
- As datas das transações são dias do calendário, e não instantes no tempo: uma transação com data
  de 1º de setembro sempre pertence a setembro, qualquer que seja o fuso horário.
- Um mês sem nenhuma transação devolve totais zerados, listas vazias e séries zeradas — nunca um
  erro.
- A taxa de poupança sem receita e a variação percentual a partir de um valor anterior zero são
  informadas como não aplicáveis (sem valor), nunca como zero, infinito ou erro.
- A variação percentual a partir de um saldo anterior negativo é medida em relação ao tamanho desse
  saldo, então um saldo que vai de −100,00 para 50,00 é um aumento de 150%.
- No mês atual, a projeção só conta as despesas com data até hoje, inclusive; uma despesa já
  registrada para um dia posterior do mês não está "gasta até hoje". Hoje conta como dia decorrido,
  então no dia 1º a projeção é a despesa do dia vezes os dias do mês.
- O mês anterior a janeiro é dezembro do ano anterior, tanto no resumo quanto na comparação
  acumulada; uma série de meses atravessa a virada do ano do mesmo jeito.
- Categorias com o mesmo gasto aparecem em ordem alfabética. "Outras" sempre vem por último, como a
  cauda da lista, mesmo quando o seu total é maior que o de algumas das categorias listadas
  individualmente.
- As participações e as variações percentuais são arredondadas para duas casas decimais, então as
  participações podem não somar exatamente 100%.
- Na comparação acumulada, um mês que ainda não começou não tem nenhum dia na sua série; um mês
  passado tem todos.

## Requisitos *(obrigatório)*

### Requisitos funcionais

- **FR-001**: Todo relatório DEVE aceitar um mês opcional no formato `YYYY-MM` e DEVE cobrir o mês
  atual no fuso horário America/Sao_Paulo quando nenhum for informado.
- **FR-002**: Todo relatório DEVE rejeitar um mês que não seja um mês `YYYY-MM` válido, explicando o
  que está errado.
- **FR-003**: O sistema DEVE tratar as datas das transações como dias do calendário: uma transação
  pertence ao mês e ao dia da sua data, nunca deslocada por um fuso horário.
- **FR-004**: O resumo mensal DEVE informar o mês, a receita total (entradas), a despesa total
  (saídas, como valor positivo) e o saldo (receita menos despesa).
- **FR-005**: O resumo mensal DEVE informar a taxa de poupança como o saldo dividido pela receita
  total, em percentual, e DEVE informá-la como não aplicável quando o mês não tiver receita.
- **FR-006**: O resumo mensal DEVE projetar a despesa e o saldo do fim do mês: para o mês atual, a
  despesa do dia 1º até hoje dividida pelos dias decorridos (incluindo hoje) vezes os dias do mês, e
  a receita do mês menos essa despesa; para um mês passado, os valores reais; para um mês futuro,
  zero.
- **FR-007**: O resumo mensal DEVE informar a receita total, a despesa total e o saldo do mês
  anterior e a variação percentual de cada um deles para o mês selecionado, informada como não
  aplicável quando o valor anterior for zero.
- **FR-008**: O relatório de despesas por categoria DEVE listar, para o mês, cada categoria com
  despesas — identificador, nome, cor, valor gasto e participação na despesa total do mês —, do
  maior valor para o menor, com a despesa total do mês.
- **FR-009**: Quando mais de oito categorias tiverem despesas no mês, o relatório de despesas por
  categoria DEVE listar as sete maiores individualmente e somar as demais numa entrada final
  "Outras", sem identificador nem cor.
- **FR-010**: O relatório de receitas vs. despesas DEVE devolver, em ordem cronológica, uma entrada
  por mês para a quantidade de meses escolhida, terminando no mês selecionado — com receita,
  despesa e saldo —, incluindo os meses sem movimentação, zerados.
- **FR-011**: O relatório de receitas vs. despesas DEVE cobrir 12 meses por padrão e DEVE rejeitar uma
  quantidade de meses menor que 1 ou maior que 24, explicando o que está errado.
- **FR-012**: O relatório de despesas acumuladas DEVE devolver, para o mês selecionado e para o mês
  anterior, o total acumulado da despesa em cada dia, do dia 1º ao último dia do mês, e a
  quantidade de dias de cada um dos dois meses.
- **FR-013**: No relatório de despesas acumuladas, a série DEVE parar em hoje no mês atual e NÃO
  DEVE ter nenhum dia num mês que ainda não começou.
- **FR-014**: O relatório de transações do mês DEVE devolver todas as transações do mês, sem
  paginação nem limite, da data mais recente para a mais antiga (na mesma data, a registrada mais
  recentemente primeiro), cada uma com identificador, data, descrição, nome da categoria, tipo
  (entrada ou saída) e valor (como valor positivo).
- **FR-015**: Os valores monetários DEVEM ser decimais exatos; os valores projetados e os
  percentuais DEVEM ser arredondados para duas casas decimais.
- **FR-016**: Cada relatório DEVE trabalhar só com os meses que cobre, somando os valores onde eles
  estão armazenados, em vez de ler todo o histórico de transações a cada solicitação.
- **FR-017**: O comportamento existente de transações e de categorias e os seus contratos NÃO
  DEVEM mudar.

### Entidades principais *(inclua se a feature envolver dados)*

- **Mês do relatório**: O mês do calendário que um relatório cobre, no formato `YYYY-MM`; o padrão é
  o mês atual em São Paulo.
- **Resumo mensal**: A receita total, a despesa total, o saldo, a taxa de poupança, a despesa e o
  saldo projetados de um mês, e os totais do mês anterior com a variação percentual de cada um.
- **Despesa da categoria**: O gasto de uma categoria num mês — nome, cor, valor e participação — ou
  a entrada "Outras", que agrupa as menores.
- **Receita e despesa mensais**: A receita, a despesa e o saldo de um mês dentro de uma série de
  meses.
- **Despesa acumulada**: O total acumulado da despesa de um mês num dia desse mês.
- **Transação do mês**: Uma transação como os relatórios a mostram — data, descrição, nome da
  categoria, tipo e valor positivo.
- **Transação** e **Categoria** *(existentes, de [[002-transaction-management]] e
  [[001-category-management]])*: Os relatórios só as leem; o tipo de uma transação vem do sinal do
  seu valor.

## Critérios de sucesso *(obrigatório)*

### Resultados mensuráveis

- **SC-001**: O usuário consegue ver a receita, a despesa, o saldo, a taxa de poupança, a projeção e
  a comparação com o mês anterior de um mês numa única solicitação.
- **SC-002**: Cada relatório responde em menos de um segundo com um histórico de 10.000 transações.
- **SC-003**: 100% dos meses sem movimentação devolvem zeros ou listas vazias em vez de erros, e 100%
  dos meses ou quantidades de meses inválidos são rejeitados com uma explicação.
- **SC-004**: Um ano de receitas e despesas volta como exatamente 12 meses consecutivos, sem
  lacunas, quaisquer que sejam os meses com movimentação.
- **SC-005**: A soma dos valores do relatório de despesas por categoria é sempre igual à despesa
  total dele e à despesa total do mês no resumo.

## Premissas

- Entradas e saídas seguem o modelo existente: o sinal do valor da transação, sem tipo armazenado.
  Os valores de despesa são informados como positivos, então o saldo é a receita menos a despesa.
- Os percentuais estão na escala 0–100 (25,5 significa 25,5%), arredondados para duas casas
  decimais; isso vale para a taxa de poupança (saldo ÷ receita × 100), as participações das
  categorias e as variações percentuais.
- A taxa de poupança não se aplica — é informada sem valor, e não como 0% — quando o mês não tem
  receita, porque não há de onde poupar; ela pode ser negativa quando a despesa passa da receita.
- A variação percentual é medida em relação ao tamanho do valor anterior, então o seu sinal sempre
  diz se o valor subiu ou desceu.
- Só a despesa é projetada; a receita do mês é considerada já conhecida, porque costuma entrar de
  uma vez, e não diariamente.
- O relatório de despesas por categoria mostra no máximo oito entradas: quando há mais de oito
  categorias, sete mais "Outras".
- Todos os valores estão numa única moeda implícita (BRL), como no resto da aplicação.
- Uso single-tenant, como no resto da aplicação; quem pode ver os relatórios está fora do escopo.
