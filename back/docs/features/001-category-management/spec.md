# Gestão de categorias

Spec retroativa, derivada da implementação e dos testes.

## Objetivo

Criar, listar, editar e excluir categorias (nome e cor) e ver, em cada uma, quantas transações tem e quanto elas somam, com busca, filtro de uso e ordenação. Assim o usuário organiza as transações do seu jeito e vê para onde vai o dinheiro sem somar à mão.

## Cenários

1. **Dado** nenhuma categoria, **quando** o usuário cria uma com nome e cor, **então** ela aparece com esse nome e essa cor.
2. **Dado** uma categoria, **quando** o usuário a renomeia ou muda a cor, **então** ela mostra o novo nome e a nova cor em todos os lugares onde aparece.
3. **Dado** uma categoria sem transações, **quando** o usuário a exclui, **então** ela sai da lista.
4. **Dado** uma categoria com transações, **quando** o usuário tenta excluí-la, **então** a exclusão é recusada com o motivo.
5. **Dado** um nome em branco, longo demais ou uma cor inválida, **quando** o usuário cria ou edita, **então** a solicitação é rejeitada com explicação clara.
6. **Dado** uma categoria com três transações, **quando** a lista é exibida, **então** ela mostra quantidade três e o saldo somado; sem transações, mostra zero e zero.
7. **Dado** um mês pedido, **quando** a lista é exibida, **então** só as transações com data nesse mês entram nos totais.
8. **Dado** várias categorias, **quando** o usuário busca por parte do nome, filtra por "em uso" ou "sem uso" ou ordena, **então** a lista reflete a escolha.
9. Casos-limite: busca ou filtro sem resultado devolve lista vazia, e não erro; transação no primeiro ou no último instante do mês entra nos totais desse mês; espaços nas pontas do nome são removidos.

## Requisitos

- **FR-001**: O sistema DEVE permitir criar uma categoria informando nome e cor.
- **FR-002**: O sistema DEVE rejeitar nome vazio, só com espaços ou com mais de 100 caracteres, explicando o erro.
- **FR-003**: O sistema DEVE remover os espaços no início e no fim do nome.
- **FR-004**: O sistema DEVE rejeitar uma cor que não seja um código de cor válido, explicando o erro.
- **FR-005**: O sistema DEVE permitir ver os detalhes de uma categoria específica. Desvio: o endpoint de detalhe foi removido em `085573b` (sem chamadores; o front edita a partir da lista), e hoje os dados da categoria saem só pela lista (FR-006).
- **FR-006**: O sistema DEVE permitir ver a lista completa de categorias.
- **FR-007**: O sistema DEVE permitir renomear e/ou mudar a cor a qualquer momento, com a mesma validação da criação.
- **FR-008**: O sistema DEVE permitir excluir uma categoria sem transações.
- **FR-009**: O sistema DEVE recusar a exclusão de uma categoria com uma ou mais transações, explicando o motivo.
- **FR-010**: O sistema DEVE rejeitar, com resultado claro de "não encontrada", editar ou excluir (e, onde houver, ver) uma categoria inexistente.
- **FR-011**: Para cada categoria da lista, o sistema DEVE informar a quantidade de transações e o saldo (soma dos valores).
- **FR-012**: O sistema DEVE permitir restringir quantidade e saldo a um único mês do calendário.
- **FR-013**: O sistema DEVE permitir filtrar só as categorias com pelo menos uma transação, ou só as sem nenhuma.
- **FR-014**: O sistema DEVE permitir buscar por parte do nome, sem diferenciar maiúsculas nem acentos.
- **FR-015**: O sistema DEVE permitir ordenar por nome, quantidade de transações ou saldo, crescente ou decrescente, com padrão nome crescente.

## Regras de negócio e erros

- Entrada inválida (nome, cor) e exclusão de categoria em uso: 400. Categoria inexistente: 404. As regras permanentes estão em [categoria](../../dominio/categoria.md) e os erros em [api.md](../../api.md).
- Nomes não precisam ser únicos. A cor é qualquer hexadecimal válido, sem paleta fixa.
- Um "mês" é o mês do calendário inteiro, e não uma janela de 30 dias.
- Não existe "reatribuir e excluir": as transações precisam ser removidas ou movidas antes.
- A lista não é paginada.

## Critérios de sucesso

- **SC-001**: O usuário cria uma categoria utilizável (nome e cor) em um único passo, e ela aparece de imediato na lista.
- **SC-002**: Quantidade e saldo aparecem para 100% das categorias da lista, sem cálculo manual.
- **SC-003**: 100% das tentativas de excluir uma categoria em uso são bloqueadas, sem transação perdida ou órfã.
- **SC-004**: Em uma lista de qualquer tamanho, o usuário localiza uma categoria digitando parte do nome ou filtrando por em uso/sem uso.
- **SC-005**: O usuário aponta a categoria mais ou menos ativa, em um mês ou em todo o período, com uma única ação de ordenação.

## Fora de escopo

Múltiplos usuários, contas e permissões (modelo single-tenant, como no resto da aplicação) e paginação da lista.
