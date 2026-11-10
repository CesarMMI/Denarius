# Cor da categoria nas transações do mês

## Objetivo

No relatório "Transações do mês", cada item passa a trazer a cor da categoria da transação, para o front pintar a transação sem uma segunda consulta. É uma mudança de contrato aditiva, sem quebrar clientes existentes.

## Cenários

1. **Dado** um mês com transações em categorias de cores diferentes, **quando** o relatório é consultado, **então** cada item traz a cor da categoria da sua transação, no mesmo formato do relatório de despesas por categoria.
2. **Dado** uma categoria cuja cor foi alterada depois de criada, **quando** o relatório é consultado, **então** os itens dela trazem a cor atual.
3. **Dado** um cliente que só conhece os campos anteriores, **quando** ele consulta o relatório, **então** recebe todos os campos de antes, com os mesmos nomes, tipos e valores.
4. Casos-limite: mês sem transações continua devolvendo lista vazia; várias transações da mesma categoria trazem a mesma cor.

## Requisitos

- **FR-001**: Cada item do relatório "Transações do mês" DEVE incluir a cor da categoria da transação.
- **FR-002**: A cor DEVE ter o formato `#` seguido de 6 dígitos hexadecimais em maiúsculas, o mesmo de `color` no relatório de despesas por categoria.
- **FR-003**: A cor DEVE ser a atual da categoria no momento da consulta.
- **FR-004**: A mudança DEVE ser aditiva: campos, tipos, ordenação, filtros e status do relatório permanecem como estão.
- **FR-005**: O contrato do relatório (`docs/features/003-financial-reports/contracts/reports-api.yaml`) DEVE ser estendido por um contrato desta feature, em `contracts/`, que descreva o campo novo como obrigatório e não nulo, sem reescrever o da feature já entregue.
- **FR-006**: A listagem `/transactions` e os demais relatórios DEVEM permanecer inalterados.

## Regras de negócio e erros

A cor nunca é nula: toda transação pertence a uma categoria, que sempre tem cor (o `null` de "Outras", no relatório por categoria, não se aplica aqui). Não há entrada nova, e a validação e o tratamento de erros do endpoint continuam como estão.

## Critérios de sucesso

- **SC-001**: Em 100% dos itens do relatório de qualquer mês, a cor devolvida é igual à cor da categoria da transação.
- **SC-002**: 100% dos testes de contrato anteriores continuam passando sem modificação.
- **SC-003**: O relatório de um mês continua sendo obtido com uma única consulta ao banco, sem consulta por transação, e o tempo de resposta com a mesma massa de dados não passa de 110% do anterior.

## Fora de escopo

A listagem `/transactions` e qualquer mudança no front, que consome o campo numa feature própria.
