# Especificação da feature: Gestão de transações

**Branch da feature**: `002-transaction-management`

**Criada em**: 2026-09-22

**Status**: Rascunho

**Entrada**: Descrição do usuário: "Especificação retroativa da capacidade existente de gestão de
transações — criar, visualizar, listar, editar e excluir transações financeiras, cada uma ligada a
exatamente uma categoria — derivada da implementação atual
(Denarius.Domain/Denarius.Application/Denarius.Infrastructure/Denarius.WebAPI) e da sua suíte de
testes automatizados. Segue a mesma abordagem retroativa já usada em 001-category-management: a
spec deve documentar o comportamento atual verificado, e não propor trabalho novo."

**Atualizada em**: 2026-09-24 — História de usuário 3 (filtrar e ordenar a lista de transações)
adicionada junto com a sua implementação: filtros por descrição, mês (com o mesmo significado de
[[001-category-management]]), tipo (todas/entradas/saídas) e categoria; ordenação por data
(padrão), descrição, valor ou nome da categoria.

## Cenários de usuário e testes *(obrigatório)*

### História de usuário 1 - Registrar e manter transações individuais (Prioridade: P1)

Como alguém que acompanha as finanças pessoais, quero registrar uma transação com data, valor e
categoria — e uma observação opcional — e depois editá-la ou removê-la, para manter um registro
fiel do dinheiro que recebi ou gastei.

**Por que esta prioridade**: Nada mais nesta feature é possível enquanto não der para registrar
uma transação. É o mínimo necessário para que o controle de transações tenha alguma utilidade, e
é o que dá às categorias de [[001-category-management]] algo a que se vincular.

**Teste independente**: Pode ser testada por completo criando uma transação numa categoria
existente, confirmando que ela aparece com a data/valor/categoria/descrição escolhidos, editando-a
e excluindo-a — sem precisar de nenhuma outra transação.

**Cenários de aceitação**:

1. **Dado que** existe uma categoria, **Quando** o usuário registra uma transação com data, valor
   e essa categoria, **Então** a transação aparece com esses dados.
2. **Dado que** existe uma transação, **Quando** o usuário muda a sua descrição, data, valor ou
   categoria, **Então** a transação mostra os novos dados em todos os lugares onde aparece.
3. **Dado que** existe uma transação, **Quando** o usuário a exclui, **Então** ela deixa de
   aparecer na lista de transações.
4. **Dado que** o usuário informa valor zero, nenhuma data, uma descrição com mais de 255
   caracteres ou uma categoria inexistente, **Quando** tenta criar ou editar uma transação,
   **Então** a solicitação é rejeitada com uma explicação clara do que está errado.
5. **Dado que** o usuário deixa a descrição em branco, **Quando** cria ou edita uma transação,
   **Então** a transação é salva sem descrição, em vez de ser rejeitada.

---

### História de usuário 2 - Revisar as transações registradas (Prioridade: P2)

Como alguém que revisa as próprias finanças, quero ver todas as transações que registrei, para
conferir o histórico de gastos e de receitas num só lugar.

**Por que esta prioridade**: Registrar transações (História 1) vale pouco se elas nunca puderem
ser revisadas depois. Depende da História 1, mas não acrescenta nenhuma ação nova de ciclo de
vida.

**Teste independente**: Pode ser testada por completo registrando várias transações e confirmando
que todas aparecem quando a lista de transações é exibida, e que a lista fica vazia quando nenhuma
foi registrada.

**Cenários de aceitação**:

1. **Dado que** várias transações foram registradas, **Quando** a lista de transações é exibida,
   **Então** todas as transações registradas aparecem com data, valor, categoria e descrição, das
   mais recentes para as mais antigas.
2. **Dado que** nenhuma transação foi registrada, **Quando** a lista de transações é exibida,
   **Então** aparece uma lista vazia.

---

### História de usuário 3 - Filtrar e reordenar a lista de transações (Prioridade: P3)

Como alguém com um histórico longo de transações, quero buscar pela descrição, ver um único mês,
mostrar só o dinheiro recebido ou só o gasto, mostrar uma única categoria e ordenar por data,
descrição, valor ou categoria, para encontrar rapidamente uma transação ou entender para onde foi
o meu dinheiro.

**Por que esta prioridade**: É só um refinamento da navegação numa lista que já funciona (História
2). Ganha valor quando o histórico cresce, mas não é necessário para a feature ser útil desde o
primeiro dia — a mesma posição que a história de busca/filtro/ordenação ocupa em
[[001-category-management]].

**Teste independente**: Pode ser testada por completo registrando várias transações com
descrições, datas, sinais e categorias variados e confirmando que cada filtro e cada opção de
ordenação, por si só, filtra ou reordena a lista corretamente, e que a combinação de filtros
restringe a lista às transações que atendem a todos eles.

**Cenários de aceitação**:

1. **Dado que** existem várias transações, **Quando** o usuário busca por parte de uma descrição,
   **Então** só aparecem as transações cuja descrição contém esse texto, sem diferenciar
   maiúsculas de minúsculas.
2. **Dado que** existem transações com datas em meses diferentes, **Quando** o usuário escolhe um
   mês, **Então** só aparecem as transações com data dentro desse mês do calendário.
3. **Dado que** foram registrados tanto dinheiro recebido quanto dinheiro gasto, **Quando** o
   usuário escolhe "entradas" ou "saídas", **Então** só aparecem as transações positivas ou só as
   negativas; escolher "todas" mostra as duas.
4. **Dado que** existem transações em várias categorias, **Quando** o usuário escolhe uma
   categoria, **Então** só aparecem as transações dessa categoria.
5. **Dado que** o usuário aplica vários filtros ao mesmo tempo, **Quando** a lista é exibida,
   **Então** só aparecem as transações que atendem a todos os filtros.
6. **Dado que** existem transações com datas, descrições, valores e categorias diferentes,
   **Quando** o usuário ordena por data, descrição, valor ou nome da categoria (em ordem crescente
   ou decrescente), **Então** a lista é ordenada de acordo.

---

### Casos-limite

- Um valor de transação exatamente zero é rejeitado de imediato, tanto na criação quanto na
  edição; fora isso, um valor negativo (dinheiro gasto) ou positivo (dinheiro recebido) é aceito
  sem restrição.
- Uma transação sem data (ou com uma data padrão/não definida) é rejeitada de imediato, tanto na
  criação quanto na edição.
- Uma descrição em branco ou só com espaços não é rejeitada — é tratada como "sem descrição", e
  não como erro.
- Espaços no início e no fim de uma descrição informada são removidos automaticamente.
- Uma descrição com mais de 255 caracteres é rejeitada de imediato, tanto na criação quanto na
  edição.
- Criar ou editar uma transação numa categoria inexistente é rejeitado com um resultado claro de
  "não encontrada" para a categoria, tanto na criação quanto na edição.
- Visualizar, editar ou excluir uma transação que não existe (ou que já foi excluída) é rejeitado
  com um resultado claro de "não encontrada".
- A exclusão de uma transação nunca é bloqueada por outros dados — hoje, nada depende de uma
  transação do jeito que as transações dependem das categorias.
- Uma busca ou combinação de filtros sem nenhum resultado devolve uma lista vazia, e não um erro.
- Uma busca por descrição em branco ou só com espaços é tratada como "sem busca", e os espaços no
  início e no fim do texto buscado são ignorados.
- Uma transação sem descrição nunca aparece numa busca por descrição.
- Uma transação com data no primeiro ou no último instante de um mês aparece quando esse mês é
  escolhido.
- A ordenação por valor usa o valor com sinal, então a ordem crescente põe a maior despesa
  primeiro e a decrescente põe a maior receita primeiro.
- Pedir um tipo ou campo de ordenação desconhecido, ou um identificador de categoria malformado, é
  rejeitado com uma explicação clara, em vez de ser ignorado em silêncio.

## Requisitos *(obrigatório)*

### Requisitos funcionais

- **FR-001**: O sistema DEVE permitir que o usuário crie uma transação informando data, valor e
  categoria, com uma descrição opcional.
- **FR-002**: O sistema DEVE rejeitar um valor de transação igual a zero, explicando o que está
  errado.
- **FR-003**: O sistema DEVE permitir que o valor da transação seja positivo (dinheiro recebido)
  ou negativo (dinheiro gasto).
- **FR-004**: O sistema DEVE rejeitar uma transação sem data, explicando o que está errado.
- **FR-005**: O sistema DEVE rejeitar uma transação que referencie uma categoria inexistente,
  explicando o que está errado.
- **FR-006**: O sistema DEVE remover os espaços nas pontas da descrição informada e tratar uma
  descrição em branco ou só com espaços como ausência de descrição, em vez de rejeitá-la.
- **FR-007**: O sistema DEVE rejeitar uma descrição com mais de 255 caracteres, explicando o que
  está errado.
- **FR-008**: O sistema DEVE permitir que o usuário veja os detalhes de uma transação específica.
- **FR-009**: O sistema DEVE permitir que o usuário veja a lista completa de transações
  registradas.
- **FR-010**: O sistema DEVE permitir que o usuário edite a descrição, a data, o valor e/ou a
  categoria de uma transação a qualquer momento, com a mesma validação da criação.
- **FR-011**: O sistema DEVE permitir que o usuário exclua uma transação de forma definitiva, sem
  restrição baseada em outros dados.
- **FR-012**: O sistema DEVE rejeitar qualquer tentativa de visualizar, editar ou excluir uma
  transação inexistente, com um resultado claro de "não encontrada".
- **FR-013**: O sistema DEVE permitir que o usuário busque na lista de transações por parte da
  descrição, sem diferenciar maiúsculas de minúsculas.
- **FR-014**: O sistema DEVE permitir que o usuário restrinja a lista de transações a um único mês
  do calendário.
- **FR-015**: O sistema DEVE permitir que o usuário restrinja a lista de transações a todas as
  transações, só ao dinheiro recebido (valor positivo) ou só ao dinheiro gasto (valor negativo).
- **FR-016**: O sistema DEVE permitir que o usuário restrinja a lista de transações a uma única
  categoria.
- **FR-017**: O sistema DEVE aplicar juntos todos os filtros pedidos, mostrando só as transações
  que atendem a todos eles.
- **FR-018**: O sistema DEVE permitir que o usuário ordene a lista de transações por data,
  descrição, valor ou nome da categoria, em ordem crescente ou decrescente, com o padrão data em
  ordem decrescente (mais recentes primeiro).

### Entidades principais *(inclua se a feature envolver dados)*

- **Transação**: Uma movimentação de dinheiro registrada, com data, valor (positivo ou negativo),
  referência obrigatória a uma categoria, descrição opcional e as datas de criação e da última
  atualização.
- **Categoria** *(entidade existente, de [[001-category-management]])*: Cada transação pertence a
  exatamente uma categoria, que já precisa existir quando a transação é criada ou muda de
  categoria.

## Critérios de sucesso *(obrigatório)*

### Resultados mensuráveis

- **SC-001**: O usuário consegue registrar uma transação utilizável (data, valor, categoria) em um
  único passo, e ela aparece imediatamente na sua lista de transações.
- **SC-002**: O usuário consegue saber se qualquer transação registrada foi dinheiro recebido ou
  gasto sem nenhuma consulta adicional — 100% das transações trazem essa informação diretamente.
- **SC-003**: O usuário consegue remover de forma definitiva qualquer transação que não queira mais
  com uma única ação, sem recusa inesperada.
- **SC-004**: O usuário consegue revisar todo o histórico de transações registradas numa única
  solicitação, sem paginação nem consulta manual.
- **SC-005**: Tentar registrar uma transação numa categoria inexistente, ou movê-la para uma, nunca
  dá certo em silêncio — 100% dessas tentativas são rejeitadas com uma explicação.
- **SC-006**: O usuário consegue ver tudo o que gastou numa categoria durante um mês numa única
  solicitação, sem cruzar o histórico completo à mão.

## Premissas

Estas premissas documentam o comportamento atual e verificado da capacidade (esta é uma spec
retroativa), e não padrões em aberto escolhidos para uma feature nova:

- A lista de transações permite busca, filtro e ordenação (História de usuário 3), mas não é
  paginada — todas as transações que atendem aos filtros vêm juntas.
- Um "mês", no filtro, é sempre um mês do calendário inteiro (do primeiro ao último instante),
  escolhido informando qualquer data dentro dele — o mesmo significado que
  [[001-category-management]] usa no recorte do uso das categorias.
- "Entradas" e "saídas" são derivadas do sinal do valor; nenhum tipo de transação é armazenado à
  parte.
- A ordenação por categoria usa o nome atual da categoria, então renomear uma categoria muda a
  posição das suas transações nessa ordenação.
- Uma transação pode ser movida para qualquer categoria existente a qualquer momento, pela edição;
  não há restrição análoga à proteção de exclusão das categorias, porque hoje nada depende de uma
  transação do jeito que as transações dependem das categorias.
- Não há campo de moeda — supõe-se que todos os valores estão numa única moeda implícita, como no
  resto da aplicação.
- Quem pode gerenciar as transações (um único usuário ou vários usuários/contas, permissões) está
  fora do escopo — esta spec supõe o mesmo modelo de uso single-tenant do resto da aplicação.
