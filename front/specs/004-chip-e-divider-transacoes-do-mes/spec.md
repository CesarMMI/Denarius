# Especificação da Feature: Chip colorido e divisores por dia em "Transações do mês"

**Branch da feature**: `main` (sem branch dedicada)

**Criada em**: 2026-11-07

**Status**: Rascunho

**Entrada**: "Na listagem 'Transações do mês' do painel de relatórios: (1) a descrição de cada transação em um chip com a cor da categoria, como a categoria na tabela da página de Transações; sem descrição, o chip mostra o nome da categoria; a linha pequena com o nome da categoria abaixo da descrição deixa de existir. (2) O divisor entre linhas segue a mesma lógica da tabela de Transações: transações do mesmo dia não têm divisor entre si. Fora de escopo: o back, a tabela da página de Transações e a ideia de agrupar datas em uma célula só (descartada)."

## Cenários de Usuário e Testes *(obrigatório)*

### User Story 1 - Reconhecer a categoria pela cor na descrição (Prioridade: P1)

Quem olha "Transações do mês" vê a descrição de cada transação dentro de um chip com a cor da categoria, o mesmo visual que a categoria tem na tabela da página de Transações. Dá para reconhecer a categoria de relance, sem uma linha extra com o nome dela.

**Por que esta prioridade**: é o pedido principal; a cor passou a vir do back (feature 004 do back, concluída).

**Teste independente**: abrir o painel de relatórios com transações de categorias de cores diferentes e conferir que cada descrição aparece num chip da cor da sua categoria.

**Cenários de aceitação**:

1. **Dado** uma transação com descrição, **Quando** a lista é exibida, **Então** a descrição aparece dentro de um chip com a cor da categoria da transação, e o nome da categoria não aparece numa linha abaixo.
2. **Dado** uma transação sem descrição, **Quando** a lista é exibida, **Então** o chip mostra o nome da categoria, com a cor dela.
3. **Dado** duas transações da mesma categoria, **Quando** a lista é exibida, **Então** os dois chips têm a mesma cor.
4. **Dado** uma descrição mais larga que a coluna, **Quando** a lista é exibida, **Então** o chip fica dentro do cartão, com o texto cortado por reticências, sem criar rolagem horizontal.

---

### User Story 2 - Divisores só entre dias diferentes (Prioridade: P2)

Na lista de "Transações do mês", o divisor entre linhas aparece só quando o dia muda, como na tabela da página de Transações. Transações do mesmo dia ficam agrupadas visualmente.

**Por que esta prioridade**: melhora a leitura, mas depende da lista já estar no formato novo e não bloqueia o chip.

**Teste independente**: abrir o painel com várias transações no mesmo dia e em dias diferentes e conferir onde há divisor.

**Cenários de aceitação**:

1. **Dado** duas transações seguidas com a mesma data exibida, **Quando** a lista é exibida, **Então** não há divisor entre elas.
2. **Dado** duas transações seguidas com datas exibidas diferentes, **Quando** a lista é exibida, **Então** há divisor entre elas.
3. **Dado** duas transações seguidas do mesmo dia com horários de registro diferentes, **Quando** a lista é exibida, **Então** elas contam como do mesmo dia: a comparação é pelo dia exibido, não pelo instante.

### Casos Limite

- Lista com uma única transação: sem divisor.
- Mês sem transações, carregando ou com erro: as mensagens e o estado atuais não mudam.
- Descrição muito longa: cortada por reticências dentro do chip.
- A lista mostra só as 10 transações mais recentes; o divisor considera as linhas exibidas.

## Requisitos *(obrigatório)*

### Requisitos Funcionais

- **FR-001**: Cada linha de "Transações do mês" MUST exibir a descrição da transação dentro de um chip com a cor da categoria, com o mesmo estilo do chip da categoria na tabela da página de Transações.
- **FR-002**: Sem descrição, o chip MUST mostrar o nome da categoria.
- **FR-003**: O nome da categoria MUST deixar de aparecer numa linha separada abaixo da descrição.
- **FR-004**: A cor do chip MUST ser a que a API devolve para a transação (campo `color` do contrato `back/specs/004-cor-categoria-transacoes-mes/contracts/monthly-transactions-api.yaml`); o front não deve inferi-la de outra fonte.
- **FR-005**: Entre duas linhas seguidas da lista com o mesmo dia, MUST NOT haver divisor; entre linhas com dias diferentes, MUST haver. A lista cobre um único mês, então dia e mês bastam para identificar o dia.
- **FR-006**: O chip MUST respeitar a largura da coluna: descrição longa é cortada por reticências, sem rolagem horizontal nova.
- **FR-007**: Valor, data, ordenação, o limite de 10 linhas, "Ver todas" e os estados vazio, de carga e de erro MUST permanecer como estão.
- **FR-008**: A tabela da página de Transações e o back MUST permanecer sem alteração.

### Entidades-chave

- **Transação do mês**: item da lista, com data, descrição (opcional), nome e cor da categoria, tipo e valor.

## Critérios de Sucesso *(obrigatório)*

### Resultados Mensuráveis

- **SC-001**: Em 100% das linhas, a cor do chip é a cor que a API devolveu para a categoria da transação.
- **SC-002**: Em qualquer sequência de transações, o número de divisores exibidos é igual ao número de mudanças de dia entre as linhas exibidas.
- **SC-003**: A lista com 10 linhas não ganha rolagem horizontal, com descrições de até 255 caracteres (verificação visual; os testes só conferem a estrutura que recebe o corte).
- **SC-004**: O painel de relatórios não faz nenhuma requisição a mais do que antes da mudança.

## Premissas

- A API já devolve `color` em cada item (feature 004 do back, concluída), sempre preenchido e no formato `#RRGGBB` em maiúsculas.
- O chip segue o mesmo padrão visual do chip de categoria da página de Transações; reaproveitar o que já existe é uma decisão do plan.
- O usuário decidiu remover a linha com o nome da categoria; a informação da categoria continua visível pela cor e, sem descrição, pelo nome no chip.
- Acessibilidade: com descrição, a categoria passa a ser indicada só pela cor, o que prejudica quem não distingue cores. O usuário decidiu remover o nome da categoria e aceitou esse risco; recuperar o nome por outro meio (dica de ferramenta ou rótulo de acessibilidade) fica fora de escopo.
- Não há entrada nova nem dado sensível; desktop é o alvo, como no resto do front.
