# Pesquisa: Cor da categoria nas transações do mês

## Origem da cor

- **Decisão**: o repositório devolve `(Transaction, Category)`, renomeando `GetWithCategoryNameAsync` para `GetWithCategoryAsync`; o output lê `Category.Name` e `Category.Color.HexCode`.
- **Motivo**: segue `SumExpensesByCategoryAsync`, que já devolve `Category`; o `Join` já traz a linha da categoria, então o custo é praticamente o mesmo (traz as colunas da categoria, não só o nome), e o quickstart (item 4) é a verificação do SC-003. A cor sai atual a cada consulta (FR-003).
- **Alternativas**: (a) `(Transaction, string, Color)`: carrega menos colunas, mas foge do padrão de `SumExpensesByCategoryAsync`; fica como opção se a medição do SC-003 reprovar; (b) segunda consulta por categoria: viola SC-003.

## Formato e nulidade

- **Decisão**: `color` é `string` não nula, `#` + 6 dígitos hexadecimais em maiúsculas (`Color.Normalize` já garante); `pattern: '^#[0-9A-F]{6}$'` no contrato.
- **Motivo**: toda transação tem categoria (FK `Restrict`, `TransactionConfiguration.cs:27-33`) e a cor é obrigatória no value object. O `null` de `CategoryExpenseOutput.color` existe só para "Outras", que não se aplica aqui.

## Contrato

- **Decisão**: contrato novo em `contracts/monthly-transactions-api.yaml` (só a operação e o schema alterados); o da 003 não é reescrito (FR-005).
- **Alternativa**: editar o da 003: reescreve o contrato de uma feature entregue e perde o histórico.

## Compatibilidade

- Serialização por propriedade: um cliente que ignora campos desconhecidos continua funcionando (aditivo). Os campos existentes, a ordenação e os status não mudam (FR-004).
