# Especificação da Feature: Cor da categoria nas transações do mês

**Branch da feature**: `main` (sem branch dedicada)

**Criada em**: 2026-11-07

**Status**: Rascunho

**Entrada**: "No relatório 'Transações do mês', cada item deve passar a retornar também a cor da categoria da transação (mudança de contrato aditiva, sem quebrar clientes existentes; mesmo formato de cor do relatório de despesas por categoria, campo `color`). Motivo: o front vai exibir a descrição da transação num chip com a cor da categoria. Fora de escopo: a listagem `/transactions` e qualquer mudança no front."

## Cenários de Usuário e Testes *(obrigatório)*

### User Story 1 - Ver a cor da categoria em cada transação do mês (Prioridade: P1)

Quem consome o relatório "Transações do mês" recebe, em cada item, a cor da categoria da transação, ao lado do nome da categoria que já vinha. Assim, o front pode pintar cada transação com a cor da sua categoria sem uma segunda consulta.

**Por que esta prioridade**: é a única mudança da feature; sem ela o front não consegue exibir a transação com a cor da categoria.

**Teste independente**: consultar as transações de um mês com transações em categorias de cores diferentes e conferir que cada item traz a cor da própria categoria.

**Cenários de aceitação**:

1. **Dado** um mês com transações em categorias de cores diferentes, **Quando** o relatório de transações do mês é consultado, **Então** cada item traz a cor da categoria da sua transação, no mesmo formato usado pelo relatório de despesas por categoria.
2. **Dado** uma categoria cuja cor foi alterada depois de criada, **Quando** o relatório é consultado, **Então** os itens dessa categoria trazem a cor atual.
3. **Dado** um cliente que só conhece os campos anteriores do relatório, **Quando** ele consulta o relatório, **Então** continua recebendo todos os campos anteriores, com os mesmos nomes, tipos e valores, e o campo novo não interfere.

### Casos Limite

- Mês sem transações: a resposta continua sendo uma lista vazia.
- Várias transações da mesma categoria: todas trazem a mesma cor.
- Toda transação pertence a uma categoria existente, então todo item tem cor; não há caso sem cor.

## Requisitos *(obrigatório)*

### Requisitos Funcionais

- **FR-001**: Cada item do relatório "Transações do mês" MUST incluir a cor da categoria da transação.
- **FR-002**: A cor MUST ter o mesmo formato da cor já devolvida pelo relatório de despesas por categoria (`#` seguido de 6 dígitos hexadecimais em maiúsculas, como em `color` do relatório de despesas por categoria).
- **FR-003**: A cor devolvida MUST ser a cor atual da categoria no momento da consulta.
- **FR-004**: A mudança MUST ser aditiva: os campos, os tipos, a ordenação, os filtros e os status do relatório MUST permanecer como estão.
- **FR-005**: O contrato do relatório (`back/specs/003-financial-reports/contracts/reports-api.yaml`) MUST ser estendido por um contrato desta feature, em `contracts/`, que descreva o campo novo como obrigatório e não nulo, sem reescrever o contrato da feature já entregue.
- **FR-006**: A listagem de transações (`/transactions`) e os demais relatórios MUST permanecer inalterados.

### Entidades-chave

- **Transação do mês**: item do relatório, com data, descrição, nome da categoria, tipo (entrada ou saída) e valor. Ganha a cor da categoria.
- **Categoria**: tem nome e cor; é a origem do nome e da cor exibidos na transação.

## Critérios de Sucesso *(obrigatório)*

### Resultados Mensuráveis

- **SC-001**: Em 100% dos itens do relatório de qualquer mês, a cor devolvida é igual à cor da categoria da transação.
- **SC-002**: Nenhum cliente que consumia o relatório antes da mudança precisa de alteração para continuar funcionando: 100% dos testes de contrato anteriores continuam passando sem modificação.
- **SC-003**: O relatório de um mês continua sendo obtido com uma única consulta ao banco por requisição, sem consulta adicional por transação, e o tempo de resposta com a mesma massa de dados não passa de 110% do tempo anterior à mudança.

## Premissas

- A cor vem da categoria, que já a guarda sempre preenchida; por isso o campo novo nunca é nulo (diferente de "Outras" no relatório por categoria, que não se aplica aqui).
- Quem consome o relatório é o front do Denarius, que fará a sua própria feature depois desta, num ciclo separado.
- A cor está disponível junto com o nome da categoria, sem acesso extra por item.
- Não há entrada nova, a cor não é dado financeiro sensível e o endpoint, a validação e o tratamento de erros continuam como estão.
