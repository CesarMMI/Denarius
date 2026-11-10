# Chip colorido e divisores por dia em "Transações do mês"

## Objetivo

Na lista "Transações do mês" do painel de relatórios, reconhecer a categoria de relance pela cor, sem uma linha extra com o nome dela, e separar as linhas só quando o dia muda, como na tabela da página de Transações.

## Cenários

1. **Dado** uma transação com descrição, **quando** a lista é exibida, **então** a descrição aparece dentro de um chip com a cor da categoria, e o nome da categoria não aparece numa linha abaixo.
2. **Dado** uma transação sem descrição, **quando** a lista é exibida, **então** o chip mostra o nome da categoria, com a cor dela.
3. **Dado** duas transações da mesma categoria, **então** os chips têm a mesma cor.
4. **Dado** uma descrição mais larga que a coluna, **então** o chip fica dentro do cartão, com o texto cortado por reticências, sem rolagem horizontal.
5. **Dado** duas transações seguidas com a mesma data exibida, **então** não há divisor entre elas, mesmo com horários de registro diferentes (a comparação é pelo dia exibido); com datas diferentes, há divisor.
6. Casos-limite: uma única transação não tem divisor; o divisor considera só as linhas exibidas (as 10 mais recentes).

## Requisitos

- **FR-001**: Cada linha DEVE exibir a descrição dentro de um chip com a cor da categoria, no mesmo estilo do chip de categoria da tabela de Transações.
- **FR-002**: Sem descrição, o chip DEVE mostrar o nome da categoria.
- **FR-003**: O nome da categoria DEVE deixar de aparecer numa linha separada abaixo da descrição.
- **FR-004**: A cor do chip DEVE ser a que a API devolve para a transação (campo `color`, ver `back/docs/features/004-cor-categoria-transacoes-mes/contracts/monthly-transactions-api.yaml`); o front não a infere de outra fonte.
- **FR-005**: Entre duas linhas seguidas com o mesmo dia NÃO DEVE haver divisor; entre dias diferentes DEVE haver. A lista cobre um único mês, então dia e mês bastam.
- **FR-006**: O chip DEVE respeitar a largura da coluna: descrição longa é cortada por reticências, sem rolagem horizontal nova.
- **FR-007**: Valor, data, ordenação, o limite de 10 linhas, "Ver todas" e os estados vazio, de carga e de erro DEVEM permanecer como estão.
- **FR-008**: A tabela da página de Transações e o back DEVEM permanecer sem alteração.

## Fluxo e estados

A lista mantém os estados vazio, de carregamento e de erro de antes. Risco de acessibilidade aceito pelo usuário: com descrição, a categoria passa a ser indicada só pela cor, o que prejudica quem não distingue cores. Recuperar o nome por outro meio (dica de ferramenta ou rótulo de acessibilidade) está fora de escopo.

## Critérios de sucesso

- **SC-001**: Em 100% das linhas, a cor do chip é a que a API devolveu para a categoria da transação.
- **SC-002**: Em qualquer sequência, o número de divisores é igual ao número de mudanças de dia entre as linhas exibidas.
- **SC-003**: Com 10 linhas e descrições de até 255 caracteres, a lista não ganha rolagem horizontal (verificação visual; os testes conferem só a estrutura que recebe o corte).
- **SC-004**: O painel não faz nenhuma requisição a mais do que antes da mudança.

## Fora de escopo

O back, a tabela da página de Transações e agrupar datas numa célula só (ideia descartada).
