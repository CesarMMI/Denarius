# Especificação da feature: Gestão de categorias

**Branch da feature**: `001-category-management`

**Criada em**: 2026-09-21

**Status**: Rascunho

**Entrada**: Descrição do usuário: "Especificação retroativa da capacidade existente de gestão de categorias — criar, visualizar, listar, editar e excluir categorias, além das estatísticas de uso por categoria (quantidade de transações e saldo) — derivada da implementação atual e da sua suíte de testes automatizados."

## Cenários de usuário e testes *(obrigatório)*

### História de usuário 1 - Montar e manter uma lista de categorias (Prioridade: P1)

Como alguém que acompanha as finanças pessoais, quero criar categorias com nome e cor e, depois,
renomeá-las, mudar a cor ou removê-las, para organizar minhas transações de um jeito que faça
sentido para mim e manter essa organização correta ao longo do tempo.

**Por que esta prioridade**: Nada mais nesta feature — nem no controle de transações em geral — é
possível enquanto não der para criar categorias. É o mínimo necessário para que as categorias
tenham alguma utilidade.

**Teste independente**: Pode ser testada por completo criando uma categoria, confirmando que ela
aparece com o nome e a cor escolhidos, editando-a e excluindo-a — sem precisar de nenhuma
transação cadastrada.

**Cenários de aceitação**:

1. **Dado que** ainda não existe nenhuma categoria, **Quando** o usuário cria uma categoria com
   nome e cor, **Então** a categoria aparece com esse nome e essa cor.
2. **Dado que** existe uma categoria, **Quando** o usuário a renomeia ou muda a sua cor, **Então**
   a categoria mostra o novo nome e a nova cor em todos os lugares onde aparece.
3. **Dado que** uma categoria não tem transações, **Quando** o usuário a exclui, **Então** ela
   deixa de aparecer na lista de categorias.
4. **Dado que** uma categoria já tem transações registradas nela, **Quando** o usuário tenta
   excluí-la, **Então** a exclusão é recusada e o usuário é informado do motivo.
5. **Dado que** o usuário informa um nome em branco, um nome longo demais ou uma cor inválida,
   **Quando** tenta criar ou editar uma categoria, **Então** a solicitação é rejeitada com uma
   explicação clara do que está errado.

---

### História de usuário 2 - Ver quanto cada categoria é usada (Prioridade: P2)

Como alguém que revisa os próprios gastos, quero que cada categoria mostre quantas transações tem
e quanto elas somam, para ver na hora para onde vai o meu dinheiro sem precisar somar as
transações à mão.

**Por que esta prioridade**: Transforma uma simples lista de rótulos numa visão financeira útil.
Depende da História 1, mas não exige nenhuma ação nova de gestão de categorias.

**Teste independente**: Pode ser testada por completo registrando um conjunto conhecido de
transações numa categoria e confirmando que a lista de categorias informa a quantidade de
transações e o saldo corretos para ela, e zero/zero para uma categoria sem transações.

**Cenários de aceitação**:

1. **Dado que** uma categoria tem três transações que somam um total conhecido, **Quando** a lista
   de categorias é exibida, **Então** essa categoria mostra a quantidade três e o saldo somado
   correto.
2. **Dado que** uma categoria não tem transações, **Quando** a lista de categorias é exibida,
   **Então** ela mostra quantidade zero e saldo zero.
3. **Dado que** o usuário pede o uso de um mês específico, **Quando** a lista de categorias é
   exibida, **Então** só as transações com data dentro desse mês entram nos totais de cada
   categoria.

---

### História de usuário 3 - Filtrar uma lista longa de categorias (Prioridade: P3)

Como alguém com muitas categorias, quero buscar pelo nome, mostrar só as categorias em uso (ou só
as sem uso) e ordenar por nome, atividade ou saldo, para encontrar ou comparar rapidamente as
categorias que me interessam.

**Por que esta prioridade**: É só um refinamento da navegação numa lista que já funciona
(Histórias 1-2). Ganha valor quando a lista de categorias cresce, mas não é necessário para a
feature ser útil desde o primeiro dia.

**Teste independente**: Pode ser testada por completo criando várias categorias com nomes e usos
variados e confirmando que a busca, o filtro em uso/sem uso e cada opção de ordenação, cada um por
si, filtram ou reordenam a lista corretamente.

**Cenários de aceitação**:

1. **Dado que** existem várias categorias, **Quando** o usuário busca por parte de um nome,
   **Então** só aparecem as categorias cujo nome contém esse texto.
2. **Dado que** algumas categorias têm transações e outras não, **Quando** o usuário filtra por
   "em uso" ou "sem uso", **Então** só aparecem as categorias correspondentes.
3. **Dado que** existem categorias com nomes, quantidades de transações e saldos diferentes,
   **Quando** o usuário ordena por nome, atividade ou saldo (em ordem crescente ou decrescente),
   **Então** a lista é ordenada de acordo.

---

### Casos-limite

- Um nome de categoria em branco, só com espaços ou com mais de 100 caracteres é rejeitado de
  imediato, tanto na criação quanto na edição.
- Espaços no início e no fim do nome são removidos automaticamente, em vez de fazerem dele uma
  categoria diferente.
- Uma cor que não seja um código de cor válido é rejeitada de imediato, tanto na criação quanto na
  edição.
- Visualizar, editar ou excluir uma categoria que não existe (ou que já foi excluída) é rejeitado
  com um resultado claro de "não encontrada".
- A exclusão de uma categoria que ainda tem transações é recusada, em vez de deixar essas
  transações órfãs sem aviso.
- Uma busca ou combinação de filtros sem nenhum resultado devolve uma lista vazia, e não um erro.
- Uma transação com data no primeiro ou no último instante de um mês ainda entra nos totais de uso
  desse mês para a sua categoria.

## Requisitos *(obrigatório)*

### Requisitos funcionais

- **FR-001**: O sistema DEVE permitir que o usuário crie uma categoria informando um nome e uma
  cor.
- **FR-002**: O sistema DEVE rejeitar um nome de categoria vazio, só com espaços ou com mais de 100
  caracteres, explicando o que está errado.
- **FR-003**: O sistema DEVE remover os espaços no início e no fim do nome da categoria, em vez de
  tratá-los como significativos.
- **FR-004**: O sistema DEVE rejeitar um valor de cor que não seja um código de cor válido,
  explicando o que está errado.
- **FR-005**: O sistema DEVE permitir que o usuário veja os detalhes de uma categoria específica.
- **FR-006**: O sistema DEVE permitir que o usuário veja a lista completa de categorias.
- **FR-007**: O sistema DEVE permitir que o usuário renomeie uma categoria e/ou mude a sua cor a
  qualquer momento, com a mesma validação da criação.
- **FR-008**: O sistema DEVE permitir que o usuário exclua uma categoria que não tenha transações
  associadas.
- **FR-009**: O sistema DEVE recusar a exclusão de uma categoria que tenha uma ou mais transações
  associadas, e DEVE explicar o motivo.
- **FR-010**: O sistema DEVE rejeitar qualquer tentativa de visualizar, editar ou excluir uma
  categoria inexistente, com um resultado claro de "não encontrada".
- **FR-011**: Para cada categoria exibida na lista, o sistema DEVE informar a quantidade de
  transações associadas e o saldo resultante (a soma dos valores dessas transações).
- **FR-012**: O sistema DEVE permitir que o usuário restrinja a quantidade de transações e o saldo
  informados de uma categoria a um único mês do calendário.
- **FR-013**: O sistema DEVE permitir que o usuário filtre a lista para mostrar só as categorias
  com pelo menos uma transação, ou só as que não têm nenhuma.
- **FR-014**: O sistema DEVE permitir que o usuário busque na lista de categorias por parte do
  nome.
- **FR-015**: O sistema DEVE permitir que o usuário ordene a lista de categorias por nome,
  quantidade de transações ou saldo, em ordem crescente ou decrescente, com o padrão nome em ordem
  crescente.

### Entidades principais *(inclua se a feature envolver dados)*

- **Categoria**: Um rótulo definido pelo usuário para agrupar transações, com um nome e uma cor
  para identificação visual, além das datas de criação e da última atualização. Uma categoria
  pode existir sem nenhuma transação.
- **Transação** *(entidade existente, de outra feature)*: Cada transação pertence a exatamente uma
  categoria. A quantidade de transações e o saldo de uma categoria são derivados inteiramente das
  transações que a referenciam.

## Critérios de sucesso *(obrigatório)*

### Resultados mensuráveis

- **SC-001**: O usuário consegue criar uma categoria utilizável (nome e cor) em um único passo, e
  ela aparece imediatamente na sua lista de categorias.
- **SC-002**: O usuário consegue dizer a quantidade de transações e o saldo de qualquer categoria
  exibida sem fazer nenhum cálculo manual — os números aparecem para 100% das categorias da lista.
- **SC-003**: Tentar excluir uma categoria ainda em uso nunca resulta em histórico de transações
  perdido ou órfão — 100% dessas tentativas são bloqueadas.
- **SC-004**: Numa lista de qualquer tamanho, o usuário consegue localizar uma categoria
  específica digitando parte do nome, ou filtrando só as categorias em uso (ou só as sem uso).
- **SC-005**: O usuário consegue apontar a sua categoria mais ativa ou menos ativa, num mês
  específico ou em todo o período, com uma única ação de ordenação — sem precisar cruzar
  transações individuais.

## Premissas

Estas premissas documentam o comportamento atual e verificado da capacidade (esta é uma spec
retroativa), e não padrões em aberto escolhidos para uma feature nova:

- Os nomes de categoria não precisam ser únicos; hoje, duas categorias podem ter o mesmo nome.
- Uma cor é qualquer código de cor hexadecimal válido; não há paleta fixa nem lista de cores
  nomeadas, só a exigência de validade.
- Não existe um fluxo de "reatribuir as transações e depois excluir" — as transações de uma
  categoria precisam ser removidas ou movidas para outra categoria antes que ela possa ser
  excluída.
- Um "mês", no recorte do uso, é sempre um mês do calendário inteiro (do primeiro ao último
  instante), e não uma janela móvel de 30 dias.
- A lista de categorias não é paginada; todas as categorias vêm juntas. Esta spec não supõe que
  isso mude quando a quantidade de categorias crescer.
- Quem pode gerenciar as categorias (um único usuário ou vários usuários/contas, permissões) está
  fora do escopo — esta spec supõe o mesmo modelo de uso single-tenant do resto da aplicação.
