# Modelo de dados: Gestão de transações

## Transaction

Representa uma movimentação de dinheiro registrada.

| Campo | Tipo | Regras |
|---|---|---|
| `Id` | `Guid` | Gerado na criação (`Guid.NewGuid()`); imutável. |
| `Description` | `string?` | Opcional; com trim; em branco/só espaços é normalizada para `null`, em vez de ser rejeitada; até 255 caracteres depois do trim, lança `DomainException` se for maior. |
| `Date` | `DateTime` | Obrigatório; `default(DateTime)` (não definido) lança `DomainException`. |
| `Value` | `decimal` | Obrigatório; qualquer valor diferente de zero — positivo (dinheiro recebido) ou negativo (dinheiro gasto); `0` lança `DomainException`. |
| `CategoryId` | `Guid` | Obrigatório; `Guid.Empty` lança `DomainException` na entidade. A existência da `Category` referenciada é verificada uma camada acima (veja abaixo). |
| `CreatedAt` | `DateTime` (UTC) | Definido uma vez, na criação; imutável. |
| `UpdatedAt` | `DateTime` (UTC) | Definido na criação; atualizado por `Update()`. |

Comportamento:

- `Transaction(description, date, value, categoryId)` — cria uma transação nova; valida e
  normaliza `description` e valida `date`, `value` e `categoryId`.
- `Update(description, date, value, categoryId)` — valida de novo e substitui os quatro campos;
  atualiza `UpdatedAt`. Uma validação que falha deixa a instância existente inalterada (a exceção
  é lançada antes de qualquer campo ser reatribuído).
- Sem soft delete nem campo de status: uma transação ou existe ou foi excluída fisicamente, sem
  proteção baseada em outros dados (ao contrário de `Category`, que recusa a exclusão enquanto é
  referenciada).

## Relacionamento: Transaction → Category

- Cada `Transaction.CategoryId` referencia exatamente uma `Category`
  ([001-category-management](../001-category-management/data-model.md)); uma `Category` tem muitas
  `Transaction`s.
- Garantido no banco: `FK_Transactions_Categories_CategoryId`, `ON DELETE RESTRICT` — espelha a
  direção descrita em
  [001-category-management/data-model.md](../001-category-management/data-model.md), mas do lado
  que referencia.
- Garantido de novo na aplicação: `CreateTransactionUseCase` e `UpdateTransactionUseCase` chamam
  `ICategoryRepository.GetByIdAsync(categoryId)` antes de persistir, para que uma categoria
  inválida/inexistente apareça como um `404` limpo (`NotFoundException`) em vez de um erro cru de
  violação de restrição.
- `Transaction` não tem navigation property de volta para `Category`; só o `CategoryId` cru é
  armazenado. Hoje, nada mais no domínio referencia uma `Transaction` — excluí-la é incondicional.

## Mapeamento de persistência (tabela `Transactions`)

| Coluna | Tipo | Observações |
|---|---|---|
| `Id` | `uuid` | Chave primária. |
| `Description` | `character varying(255)` | Anulável. |
| `Date` | `timestamp with time zone` | `NOT NULL`. |
| `Value` | `numeric(18,2)` | `NOT NULL`. |
| `CategoryId` | `uuid` | `NOT NULL`; FK → `Categories.Id`, `ON DELETE RESTRICT`; indexado (`IX_Transactions_CategoryId`). |
| `CreatedAt` | `timestamp with time zone` | `NOT NULL`. |
| `UpdatedAt` | `timestamp with time zone` | `NOT NULL`. |

Sem transições de estado além de criar/atualizar/excluir — `Transaction` não tem workflow nem
campo de status e (ao contrário de `Category`) nenhum caso de uso lhe acrescenta campos
derivados/calculados.

## Consulta da lista (`ListTransactionsInput`)

Entrada da camada Application para o `ListTransactionsUseCase` (História de usuário 3); não é
persistida. Todos os campos são opcionais, e os filtros se combinam com AND.

| Campo | Tipo | Padrão | Regras |
|---|---|---|---|
| `Description` | `string?` | `null` | Com trim; busca parcial, sem diferenciar maiúsculas de minúsculas, em `Transaction.Description`. `null`/em branco = sem filtro. Transações sem descrição nunca aparecem. |
| `DateRef` | `DateTime?` | `null` | Qualquer data dentro do mês desejado; mantém as transações com `Date` do primeiro instante desse mês até o último tick, inclusive. `null` = todos os meses. |
| `Type` | `TransactionType` | `All` | `All` = sem filtro; `In` = `Value > 0`; `Out` = `Value < 0`. |
| `CategoryId` | `Guid?` | `null` | Mantém só as transações com esse `CategoryId`. `null` = todas as categorias. Um id que não corresponde a nenhuma categoria só produz uma lista vazia. |
| `OrderBy` | `TransactionOrderField` | `Date` | `Date`, `Description`, `Value` (com sinal) ou `CategoryName` (o nome atual da categoria referenciada). Empates são desfeitos pela `Date` mais recente e depois pelo `CreatedAt`; ordenando por `Date`, eles seguem a direção dela. |
| `Ascending` | `bool` | `false` | `false` = decrescente; o padrão é mais recentes primeiro. Vale para o campo de `OrderBy`. |

A saída continua sendo `IEnumerable<TransactionOutput>`, com o formato inalterado — `CategoryName`
é só uma chave de ordenação e não é adicionado à resposta.
