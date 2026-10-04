# Modelo de dados: Gestão de categorias

## Category

Representa um agrupamento de transações definido pelo usuário.

| Campo | Tipo | Regras |
|---|---|---|
| `Id` | `Guid` | Gerado na criação (`Guid.NewGuid()`); imutável. |
| `Name` | `string` | Obrigatório; com trim; 1-100 caracteres depois do trim; lança `DomainException` caso contrário. |
| `Color` | `Color` (value object) | Obrigatório; precisa ser normalizável para um código hexadecimal válido (veja abaixo). |
| `CreatedAt` | `DateTime` (UTC) | Definido uma vez, na criação; imutável. |
| `UpdatedAt` | `DateTime` (UTC) | Definido na criação; atualizado por `Update()`. |

Comportamento:

- `Category(name, color)` — cria uma categoria nova; valida e faz o trim de `name` e valida
  `color`.
- `Update(name, color)` — valida de novo e substitui `Name`/`Color`; atualiza `UpdatedAt`.
- Sem soft delete nem campo de status: uma categoria ou existe ou foi excluída fisicamente (sujeito
  à proteção de transações abaixo).

## Color (value object)

Não é específico de Category, mas Category é hoje o seu único consumidor.

| Campo | Tipo | Regras |
|---|---|---|
| `HexCode` | `string` | Normalizado para `#RRGGBB` em maiúsculas. Aceita entrada com ou sem `#` inicial e a forma abreviada de 3 dígitos (`F00` → `#FF0000`). Qualquer valor que não case com `^#[0-9A-F]{6}$` depois da normalização lança `DomainException`. |

A igualdade é por valor (`HexCode`).

## Relacionamento: Category ↔ Transaction

- Uma `Category` tem muitas `Transaction`s; cada `Transaction.CategoryId` referencia exatamente uma
  `Category`.
- Garantido no banco: `FK_Transactions_Categories_CategoryId`, `ON DELETE RESTRICT` — o próprio
  banco recusa excluir uma linha de `Category` referenciada por alguma linha de `Transaction`.
- Garantido de novo na aplicação: o `DeleteCategoryUseCase` consulta antes o
  `ITransactionRepository.ExistsByCategoryIdAsync`, para que a restrição apareça como um `400`
  limpo (`DomainException`) em vez de um erro cru de violação de restrição.
- `Category` não guarda referência/coleção das suas transações (sem navigation property) — a
  quantidade/saldo exibidos por categoria são calculados na leitura, e não armazenados (veja
  abaixo).

## Derivado (não persistido): uso por categoria

Calculado pelo `ListCategoriesUseCase` para cada `Category`, sem ser armazenado na entidade:

| Campo | Tipo | Definição |
|---|---|---|
| `TransactionCount` | `int` | Quantidade de `Transaction`s com o mesmo `CategoryId` (dentro do mês do calendário pedido, se o `dateRef` for informado). |
| `Balance` | `decimal` | Soma do `Value` dessas transações. |
| `CanDelete` | `bool` | `true` quando nenhuma `Transaction` tem este `CategoryId`, em qualquer mês (ignora o `dateRef`); é a regra de exclusão. |

## Mapeamento de persistência (tabela `Categories`)

| Coluna | Tipo | Observações |
|---|---|---|
| `Id` | `uuid` | Chave primária. |
| `Name` | `character varying(100)` | `NOT NULL`. |
| `Color` | `character varying(7)` | `NOT NULL`; guarda `Color.HexCode` por meio de um value converter do EF Core. |
| `CreatedAt` | `timestamp with time zone` | `NOT NULL`. |
| `UpdatedAt` | `timestamp with time zone` | `NOT NULL`. |

Sem transições de estado além de criar/atualizar/excluir — `Category` não tem workflow nem campo
de status.
