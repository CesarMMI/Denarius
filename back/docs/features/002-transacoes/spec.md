# Gestão de transações

## Objetivo

Registrar, listar, editar e excluir transações financeiras, cada uma ligada a exatamente uma [categoria](../001-categorias/spec.md), e encontrar uma transação ou entender para onde foi o dinheiro filtrando e ordenando a lista. Entidade em [docs/dominio/transacao.md](../../dominio/transacao.md).

## Cenários

1. **Dado que** existe uma categoria, **quando** o usuário registra uma transação com data, valor e essa categoria, **então** ela aparece com esses dados.
2. **Dado que** existe uma transação, **quando** o usuário muda descrição, data, valor ou categoria, **então** ela mostra os novos dados em todos os lugares onde aparece.
3. **Dado que** existe uma transação, **quando** o usuário a exclui, **então** ela deixa de aparecer na lista.
4. **Dado** valor zero, nenhuma data, descrição com mais de 255 caracteres ou categoria inexistente, **quando** o usuário cria ou edita, **então** a solicitação é rejeitada com uma explicação clara. Uma descrição em branco é salva como "sem descrição", e não rejeitada.
5. **Dado que** várias transações foram registradas, **quando** a lista é exibida, **então** todas aparecem com data, valor, categoria e descrição, das mais recentes para as mais antigas; sem transações, a lista vem vazia.
6. **Dado** transações variadas, **quando** o usuário busca por parte da descrição, escolhe um mês, "entradas" ou "saídas" ou uma categoria, **então** só aparecem as que atendem a todos os filtros pedidos.
7. **Dado** transações variadas, **quando** o usuário ordena por data, descrição, valor ou nome da categoria, em ordem crescente ou decrescente, **então** a lista segue a ordem pedida, com o alfabeto do português do Brasil nos textos (FR-018).
8. **Dado** a transação "Farmácia São João", **quando** o usuário busca "farmacia sao", "FARMÁCIA" ou "  são joão  ", **então** ela aparece; a busca "padariá" encontra "Padaria", "acai" e "AÇAÍ" encontram "Açaí", e "farma" encontra "Farmácia" mas não "Mercado".
9. **Dado** um texto buscado com `%`, `_` ou `\`, **quando** a lista é pedida, **então** esses caracteres são procurados como texto, e não como curingas.
10. **Dado** transações com as descrições "Banana", "Água", "agua", "água" e "Agua", **quando** o usuário ordena por descrição crescente, **então** a ordem é "agua", "Agua", "água", "Água", "Banana", e a decrescente é a inversa.
11. **Dado** transações às 00:00 do dia 1 e às 23:59:59,999 do último dia de um mês, **quando** o usuário filtra esse mês, **então** as duas aparecem, e a primeira não aparece no mês anterior.
12. Casos-limite: busca ou combinação sem resultado, ou com categoria inexistente, devolve lista vazia, e não erro; busca em branco equivale a "sem busca" e os espaços das pontas são ignorados; transação sem descrição nunca aparece numa busca por descrição; tipo ou campo de ordenação desconhecido, ou identificador de categoria malformado, é rejeitado com explicação, e não ignorado.

## Requisitos

- **FR-001**: O sistema DEVE permitir criar uma transação informando data, valor e categoria, com descrição opcional.
- **FR-002**: O sistema DEVE rejeitar um valor igual a zero, explicando o que está errado.
- **FR-003**: O valor DEVE poder ser positivo (dinheiro recebido) ou negativo (dinheiro gasto).
- **FR-004**: O sistema DEVE rejeitar uma transação sem data, explicando o que está errado.
- **FR-005**: O sistema DEVE rejeitar uma transação que referencie uma categoria inexistente, explicando o que está errado.
- **FR-006**: O sistema DEVE remover os espaços nas pontas da descrição e tratar uma descrição em branco ou só com espaços como ausência de descrição, em vez de rejeitá-la.
- **FR-007**: O sistema DEVE rejeitar uma descrição com mais de 255 caracteres, explicando o que está errado.
- **FR-009**: O sistema DEVE permitir ver a lista completa das transações registradas.
- **FR-010**: O sistema DEVE permitir editar descrição, data, valor e/ou categoria a qualquer momento, com a mesma validação da criação.
- **FR-011**: O sistema DEVE permitir excluir uma transação de forma definitiva, sem restrição baseada em outros dados.
- **FR-012**: O sistema DEVE rejeitar editar ou excluir uma transação inexistente (ou já excluída), com um resultado claro de "não encontrada".
- **FR-013**: O sistema DEVE permitir buscar na lista por parte da descrição, sem diferenciar maiúsculas de minúsculas nem acentos, depois de remover os espaços das pontas do texto. Todos os caracteres do texto, inclusive `%`, `_` e `\`, valem como texto, sem curingas.
- **FR-014**: O sistema DEVE permitir restringir a lista a um único mês do calendário.
- **FR-015**: O sistema DEVE permitir restringir a lista a todas as transações, só ao dinheiro recebido (valor positivo) ou só ao gasto (valor negativo).
- **FR-016**: O sistema DEVE permitir restringir a lista a uma única categoria.
- **FR-017**: O sistema DEVE aplicar juntos todos os filtros pedidos, mostrando só as transações que atendem a todos.
- **FR-018**: O sistema DEVE permitir ordenar a lista por data, descrição, valor ou nome da categoria, crescente ou decrescente, com padrão data decrescente. Os desempates (data e depois horário de criação, ambos decrescentes, exceto na ordenação por data crescente, em que também são crescentes) e a posição das transações sem descrição (início da ordem crescente, fim da decrescente) são fixos. A ordem alfabética de descrições e nomes de categoria DEVE ser a do português do Brasil e a mesma em qualquer ambiente, em vez de depender da configuração da máquina: compara-se primeiro sem acentos e sem diferenciar maiúsculas; depois a letra sem acento vem antes da acentuada; por fim, a minúscula antes da maiúscula. Só textos idênticos caem nos desempates.
- **FR-019**: A lista filtrada por mês, por categoria ou por ambos NÃO DEVE ter o tempo de resposta dependente da quantidade de transações de outros meses ou categorias. A busca por descrição e o filtro por tipo avaliam só as transações que passaram pelos outros filtros presentes; quando são os únicos filtros, avaliam todas, e o tempo cresce com o total. A ordenação por nome da categoria exige juntar as duas tabelas e não tem meta de tempo própria.
- **FR-020**: Os parâmetros aceitos e as respostas de erro da lista NÃO DEVEM mudar, nem o formato da resposta nem o conjunto de transações devolvido, salvo o FR-013 e o limite do mês das regras abaixo. A ordem alfabética do FR-018 é deliberadamente independente do ambiente: a diferença para a de antes, onde houver, é consequência dele, e não quebra de compatibilidade. O contrato da lista é republicado em `contracts/` (1.3.0, aditivo), dizendo que a busca ignora acentos e qual é a regra de ordem alfabética.
- **FR-021**: Nenhum parâmetro novo é aceito, a validação de entrada atual se mantém, e o texto buscado e as descrições NÃO DEVEM ser registrados em log.

## Regras de negócio e erros

- Campos, limites e o tipo pelo sinal do valor estão em [docs/dominio/transacao.md](../../dominio/transacao.md), inclusive a faixa e a escala do valor (corrigidas em [valor-sem-faixa-e-escala](../../bugs/valor-sem-faixa-e-escala/fix.md)).
- Categoria inexistente na criação ou na edição, e transação inexistente na edição ou na exclusão, respondem "não encontrada". A exclusão nunca é bloqueada: nada depende de uma transação.
- "Mês" é sempre o mês do calendário inteiro, escolhido por qualquer data dentro dele, do início do mês (inclusive) ao início do seguinte (exclusive), com o mesmo significado do recorte das categorias. Uma transação às 23:59:59,999 do último dia é do mês dele; só uma com fração de segundo além disso muda de mês em relação ao comportamento anterior.
- "Entradas" e "saídas" derivam do sinal do valor; não há tipo armazenado. A ordenação por valor usa o valor com sinal: a crescente põe a maior despesa primeiro.
- A ordenação por categoria usa o nome atual dela, então renomear uma categoria muda a posição das suas transações.
- Ignorar acentos vale só para a busca; na ordenação eles só desempatam. A lista não é paginada (volume de finanças pessoais, de centenas a poucos milhares por ano).

## Critérios de sucesso

- **SC-001**: O usuário registra uma transação utilizável (data, valor, categoria) em um único passo, e ela aparece imediatamente na lista.
- **SC-002**: 100% das transações trazem, diretamente, a informação de serem dinheiro recebido ou gasto.
- **SC-003**: O usuário remove de forma definitiva qualquer transação com uma única ação, sem recusa inesperada.
- **SC-004**: O usuário revisa todo o histórico numa única solicitação, sem paginação nem consulta manual.
- **SC-005**: 100% das tentativas de registrar uma transação numa categoria inexistente, ou movê-la para uma, são rejeitadas com explicação.
- **SC-006**: O usuário vê tudo o que gastou numa categoria durante um mês numa única solicitação.
- **SC-007**: Todas as buscas dos cenários 8 a 10 encontram exatamente as transações esperadas.
- **SC-008**: Com 10.000 transações, das quais 200 no mês pedido e 200 na categoria pedida, a lista desse mês e a dessa categoria respondem, cada uma, pelo menos 5 vezes mais rápido que antes da alteração de 2026-10-11, medindo antes e depois no mesmo ambiente, com os mesmos dados e a mediana de 20 requisições. Não há teto absoluto de tempo.
- **SC-009**: Para o mesmo conjunto de transações, cada combinação de filtro e ordenação coberta pelos testes de antes da alteração devolve o mesmo conteúdo, na mesma ordem, antes e depois, exceto o que o FR-013, o limite do mês e a ordem alfabética do FR-018 alteram de propósito.
- **SC-010**: Nenhum consumidor da API precisa mudar: requisições e respostas da lista mantêm o formato.

## Fora de escopo

- Moeda (todos os valores estão numa única moeda implícita) e quem pode gerenciar as transações (uso single-tenant, como no resto da aplicação).
